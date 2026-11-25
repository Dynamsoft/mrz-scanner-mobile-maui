using Dynamsoft.MRZScannerBundle.Maui;

namespace ScanMRZ;

// Home page: runs the scanner, opens ResultPage for a finished scan and reports anything else here.
public partial class MainPage : ContentPage
{
    // Android can destroy and rebuild this page while the scanner is open (e.g. under memory pressure),
    // so a new result waits here until a page that is still on screen can show it.
    private static MRZScanResult? _pendingResult;
    private static event Action? PendingResultChanged;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        PendingResultChanged += ShowPendingResult;
        ShowPendingResult();
    }

    protected override void OnDisappearing()
    {
        PendingResultChanged -= ShowPendingResult;
        base.OnDisappearing();
    }

    private async void OnScanMRZ(object sender, EventArgs e)
    {
        LblStatus.IsVisible = false;
        BtnOpenSettings.IsVisible = false;
        await ScanAsync();
    }

    // Also run by ResultPage's Re-scan button once it is back on this page.
    internal static async Task ScanAsync()
    {
        // A trial license, so it needs a network connection. Request your own at
        // https://www.dynamsoft.com/customer/license/trialLicense/?product=mrz&utm_source=samples&package=maui
        var config = new MRZScannerConfig("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9");

        // Every setting below is commented out at the opposite of its default — uncomment to try.

        // Restrict recognition to one document family. Default is EnumDocumentType.All, which reads both.
        //config.DocumentType = EnumDocumentType.Passport;
        // Your own Capture Vision template (app asset, path or inline JSON); outranks DocumentType.
        //config.TemplateFile = "MyTemplate.json";

        // Scanner controls. Every one of these is visible by default.
        //config.IsCloseButtonVisible = false;        // Leaves no way out of the scanner.
        //config.IsTorchButtonVisible = false;        // Hides the torch toggle.
        //config.IsCameraToggleButtonVisible = false; // Hides the front/back camera toggle.
        //config.IsBeepButtonVisible = false;         // Hides the beep toggle.
        //config.IsVibrateButtonVisible = false;      // Hides the vibrate toggle.
        //config.IsFormatSelectorVisible = false;     // Hides the ID / Both / Passport selector.
        //config.IsGuideFrameVisible = false;         // Hides the guide frame and its prompt.

        // Both off by default; the buttons above toggle them, so this is just the opening state.
        //config.IsBeepEnabled = true;
        //config.IsVibrateEnabled = true;

        // Crops come back by default, the full camera frame does not.
        //config.ReturnDocumentImage = false;
        //config.ReturnPortraitImage = false;
        //config.ReturnOriginalImage = true;

        // Suppress the scanner's own permission dialog; either way it arrives as EnumResultStatus.Exception.
        //config.IsCameraPermissionPromptEnabled = false;

        _pendingResult = await MRZScanner.Start(config);
        PendingResultChanged?.Invoke();
    }

    private async void ShowPendingResult()
    {
        // A page whose window went away with the old Android activity leaves the result to its replacement.
        if (Window is not { } window || Application.Current?.Windows.Contains(window) != true)
        {
            PendingResultChanged -= ShowPendingResult;
            return;
        }
        if (_pendingResult is not { } result) return;
        _pendingResult = null;

        switch (result.ResultStatus)
        {
            case EnumResultStatus.Finished when result.Data is not null:
                await Navigation.PushAsync(new ResultPage(result));
                break;
            case EnumResultStatus.Finished:
                // Nothing to show, so report it here rather than open an empty result page.
                ShowStatus("Scan returned no data");
                break;
            case EnumResultStatus.Canceled:
                // The user closed the scanner. There is no data and nothing went wrong.
                ShowStatus("Scan canceled");
                break;
            default:
                ShowStatus(result.ErrorString ?? "");
                // Only a user denial is fixable in Settings; a restricted camera has no toggle there.
                BtnOpenSettings.IsVisible = result.ErrorCode == (int)EnumErrorCode.CameraPermissionDenied;
                break;
        }
    }

    private void ShowStatus(string message)
    {
        LblStatus.Text = message;
        LblStatus.IsVisible = true;
    }

    // The scanner's own permission prompt is gone by the time this shows, so keep a route to Settings.
    private void OnOpenSettings(object sender, EventArgs e) => AppInfo.ShowSettingsUI();
}
