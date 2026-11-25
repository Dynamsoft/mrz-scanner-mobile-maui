using Dynamsoft.CodeParser.Maui;
using Dynamsoft.Core.Maui;
using Dynamsoft.MRZScannerBundle.Maui;
using System.Runtime.Versioning;

namespace ScanMRZ;

// Shows a finished scan. Re-scan and Return home both go back to MainPage, which runs the scanner.
public partial class ResultPage : ContentPage
{
    private const string PortraitPlaceholderImage = "portrait_placeholder.jpg";
    private bool _isPortraitPlaceholderShown;

    public ResultPage(MRZScanResult result)
    {
        InitializeComponent();
        PopulateResult(result);
    }

    private async void OnRescan(object? sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
        await MainPage.ScanAsync();
    }

    private async void OnReturnHome(object? sender, EventArgs e) => await Navigation.PopToRootAsync();

    private void PopulateResult(MRZScanResult result)
    {
        var data = result.Data!;

        // Header
        LblName.Text = $"{data.FirstName} {data.LastName}";
        LblSexAge.Text = $"{FormatSex(data.Sex)}, {data.Age} years old";
        LblExpiry.Text = $"Expiry: {data.DateOfExpire}";

        // Images. Reading them is cheap, so the layout is settled now and only the slow conversion
        // to ImageSource waits (see LoadImagesAsync). The portrait is returned by default, but is
        // null when none could be cropped, e.g. a TD1/TD2 ID scanned on its MRZ side only.
        var portrait = TryGetImage(result.GetPortraitImage);
        var mrzDocument = TryGetImage(() => result.GetDocumentImage(EnumDocumentSide.MRZ));
        var oppositeDocument = TryGetImage(() => result.GetDocumentImage(EnumDocumentSide.Opposite));
        var mrzOriginal = TryGetImage(() => result.GetOriginalImage(EnumDocumentSide.MRZ));
        var oppositeOriginal = TryGetImage(() => result.GetOriginalImage(EnumDocumentSide.Opposite));

        if (portrait is null)
            ShowPortraitPlaceholder();
        AdjustImageGrid(ProcessedImages, mrzDocument is not null, BorderMrzProcessed, oppositeDocument is not null, BorderOppositeProcessed);
        AdjustImageGrid(OriginalImages, mrzOriginal is not null, BorderMrzOriginal, oppositeOriginal is not null, BorderOppositeOriginal);
        UpdateImageSectionVisibility(
            hasProcessed: mrzDocument is not null || oppositeDocument is not null,
            hasOriginal: mrzOriginal is not null || oppositeOriginal is not null);
        LoadImagesAsync((ImgPortrait, portrait), (ImgMrzProcessed, mrzDocument), (ImgOppositeProcessed, oppositeDocument),
            (ImgMrzOriginal, mrzOriginal), (ImgOppositeOriginal, oppositeOriginal));

        // Personal Info. The header above stays unmarked: tinting "gender, age" on one field's
        // status would implicate both, so the rows carry the validation instead.
        ApplyField(ValGivenName, data.FirstName, data.GetFieldValidationStatus("firstName"));
        ApplyField(ValSurname, data.LastName, data.GetFieldValidationStatus("lastName"));
        ApplyField(ValDob, data.DateOfBirth, data.GetFieldValidationStatus("dateOfBirth"));
        ApplyField(ValGender, FormatSex(data.Sex), data.GetFieldValidationStatus("sex"));
        ApplyField(ValNationality, data.Nationality, data.GetFieldValidationStatus("nationality"));

        // Document Info. The type comes from the MRZ layout itself, so it has no check digit.
        ApplyField(ValDocType, data.DocumentType switch
        {
            "MRTD_TD1_ID" => "ID (TD1)",
            "MRTD_TD2_ID" => "ID (TD2)",
            "MRTD_TD3_PASSPORT" => "Passport (TD3)",
            _ => data.DocumentType
        });
        ApplyField(ValDocNumber, data.DocumentNumber, data.GetFieldValidationStatus("documentNumber"));
        ApplyField(ValExpiry, data.DateOfExpire, data.GetFieldValidationStatus("dateOfExpire"));

        // Tappable too: a failure across a whole MRZ line can flag the text when no single field did.
        ApplyField(ValMrzText, data.MrzText, data.GetFieldValidationStatus("mrzText"));
    }

    private static readonly Color WarningAmber = Color.FromArgb("#FFC107");

    // Shows the value, or "N/A" when empty. A value that failed its check digit turns amber and
    // underlined, gets a warning icon and opens an explanation when tapped, as in native ScanMRZ.
    private void ApplyField(Label label, string value, EnumValidationStatus status = EnumValidationStatus.VS_NONE)
    {
        label.Text = string.IsNullOrEmpty(value) ? "N/A" : value;
        if (status != EnumValidationStatus.VS_FAILED || label.Parent is not HorizontalStackLayout cell) return;

        label.TextColor = WarningAmber;
        label.TextDecorations = TextDecorations.Underline;
        // The icon has no text of its own and colour alone isn't a cue, so spell it out.
        SemanticProperties.SetDescription(label, $"{label.Text}, validation failed");

        var iconSize = Math.Round(label.FontSize * 1.2);
        var icon = new Image { Source = "warning_circle.png", WidthRequest = iconSize, HeightRequest = iconSize, VerticalOptions = LayoutOptions.Center };
        cell.Add(icon);
        // The stack measures its children without a width limit, so cap the text to leave room for the icon.
        cell.SizeChanged += (_, _) => label.MaximumWidthRequest = Math.Max(0, cell.Width - iconSize - cell.Spacing);
        cell.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(ShowValidationInfo) });
    }

    private async void ShowValidationInfo() =>
        await DisplayAlertAsync("Field validation warning",
            "This value doesn't match its check digit. The document may be invalid or altered.", "OK");

    // Scans that fail validation are returned too, so the sex field can be empty.
    private static string FormatSex(string sex) =>
        string.IsNullOrWhiteSpace(sex) ? "N/A" : $"{char.ToUpper(sex[0])}{sex[1..].ToLower()}";

    // Shows tabs only when both image sets came back; a single set gets a plain header and no
    // images collapse the section. ReturnOriginalImage is false by default, so "Processed" often stands alone.
    private void UpdateImageSectionVisibility(bool hasProcessed, bool hasOriginal)
    {
        var showsTabs = hasProcessed && hasOriginal;

        ImageTabs.IsVisible = showsTabs;
        LblImagesHeader.IsVisible = !showsTabs && (hasProcessed || hasOriginal);
        LblImagesHeader.Text = hasProcessed ? "Processed Image(s)" : "Original Image(s)";
        ProcessedImages.IsVisible = hasProcessed;
        OriginalImages.IsVisible = hasOriginal && !hasProcessed;
    }

    private static void AdjustImageGrid(Grid grid, bool hasFirst, Border borderFirst, bool hasSecond, Border borderSecond)
    {
        if (hasFirst && hasSecond)
        {
            // Two images: two-column layout (default)
            grid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            };
            grid.HeightRequest = 160;
            borderFirst.SetValue(Grid.ColumnProperty, 0);
            borderFirst.SetValue(Grid.ColumnSpanProperty, 1);
            borderSecond.IsVisible = true;
        }
        else
        {
            // One image: full width
            grid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star)
            };
            grid.HeightRequest = 160;

            if (hasFirst)
            {
                borderFirst.SetValue(Grid.ColumnProperty, 0);
                borderFirst.SetValue(Grid.ColumnSpanProperty, 1);
                borderSecond.IsVisible = false;
            }
            else if (hasSecond)
            {
                borderSecond.SetValue(Grid.ColumnProperty, 0);
                borderSecond.SetValue(Grid.ColumnSpanProperty, 1);
                borderFirst.IsVisible = false;
            }
            else
            {
                borderFirst.IsVisible = false;
                borderSecond.IsVisible = false;
                grid.HeightRequest = 0;
            }
        }
    }

    private CancellationTokenSource? _longPressCts;
    private Image? _pressedImage;
    private static readonly TimeSpan LongPressDuration = TimeSpan.FromMilliseconds(500);

    // Android saves through MediaStore's RelativePath, which needs Android 10 (API 29). Older versions
    // would need the storage permission instead, so the sample doesn't offer saving there.
    [SupportedOSPlatformGuard("android29.0")]
    private static bool CanSaveImages => !OperatingSystem.IsAndroid() || OperatingSystem.IsAndroidVersionAtLeast(29);

    private void OnImagePointerPressed(object? sender, PointerEventArgs e)
    {
        if (sender is not Image image || image.Source is null)
            return;

        if (ReferenceEquals(image, ImgPortrait) && _isPortraitPlaceholderShown)
            return;

        if (!CanSaveImages)
            return;

        _pressedImage = image;
        _longPressCts?.Cancel();
        _longPressCts = new CancellationTokenSource();
        var cts = _longPressCts;

        _ = Task.Delay(LongPressDuration, cts.Token).ContinueWith(async t =>
        {
            if (t.IsCanceled) return;
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (_pressedImage != image) return;
                var save = await DisplayActionSheetAsync("Save image to photo library?", "Cancel", null, "Save");
                if (save == "Save")
                    await SaveImageToGalleryAsync(image);
            });
        });
    }

    private void OnImagePointerReleased(object? sender, PointerEventArgs e)
    {
        _longPressCts?.Cancel();
        _longPressCts = null;
        _pressedImage = null;
    }

    private async Task SaveImageToGalleryAsync(Image image)
    {
        try
        {
            var screenshot = await image.CaptureAsync();
            if (screenshot is null) return;

            using var stream = await screenshot.OpenReadAsync();
            using var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);
            var bytes = memStream.ToArray();

            var fileName = $"MRZ_{DateTime.Now:yyyyMMdd_HHmmss}.png";
#if IOS
            await SaveToGalleryiOS(bytes);
#elif ANDROID
            if (CanSaveImages)
                await SaveToGalleryAndroid(bytes, fileName);
#endif
            await DisplayAlertAsync("Saved", "Image saved to photo library.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Failed to save image: {ex.Message}", "OK");
        }
    }

#if IOS
    private static async Task SaveToGalleryiOS(byte[] bytes)
    {
        // Check / request photo library add permission
        var status = Photos.PHPhotoLibrary.GetAuthorizationStatus(Photos.PHAccessLevel.AddOnly);
        if (status == Photos.PHAuthorizationStatus.NotDetermined)
        {
            status = await Photos.PHPhotoLibrary.RequestAuthorizationAsync(Photos.PHAccessLevel.AddOnly);
        }

        if (status != Photos.PHAuthorizationStatus.Authorized && status != Photos.PHAuthorizationStatus.Limited)
        {
            throw new UnauthorizedAccessException("Photo library access was denied. Please grant permission in Settings.");
        }

        var tcs = new TaskCompletionSource<bool>();
        var uiImage = new UIKit.UIImage(Foundation.NSData.FromArray(bytes));
        uiImage.SaveToPhotosAlbum((_, error) =>
        {
            if (error is not null)
                tcs.TrySetException(new Exception(error.LocalizedDescription));
            else
                tcs.TrySetResult(true);
        });
        await tcs.Task;
    }
#endif

#if ANDROID
    [SupportedOSPlatform("android29.0")]
    private async Task SaveToGalleryAndroid(byte[] bytes, string fileName)
    {
        var context = Platform.CurrentActivity ?? Platform.AppContext;
        var contentValues = new Android.Content.ContentValues();
        contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
        contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
        contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, "Pictures/ScanMRZ");

        var uri = context.ContentResolver!.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri!, contentValues);
        if (uri is not null)
        {
            using var outputStream = context.ContentResolver.OpenOutputStream(uri);
            if (outputStream is not null)
            {
                await outputStream.WriteAsync(bytes, 0, bytes.Length);
                await outputStream.FlushAsync();
            }
        }
    }
#endif

    private void OnProcessedTab(object? sender, EventArgs e)
    {
        ProcessedImages.IsVisible = true;
        OriginalImages.IsVisible = false;
        BtnProcessed.TextColor = Colors.White;
        BtnProcessed.FontAttributes = FontAttributes.Bold;
        BtnOriginal.TextColor = Color.FromArgb("#888888");
        BtnOriginal.FontAttributes = FontAttributes.None;
        UnderlineProcessed.BackgroundColor = Colors.White;
        UnderlineOriginal.BackgroundColor = Colors.Transparent;
    }

    private void OnOriginalTab(object? sender, EventArgs e)
    {
        ProcessedImages.IsVisible = false;
        OriginalImages.IsVisible = true;
        BtnOriginal.TextColor = Colors.White;
        BtnOriginal.FontAttributes = FontAttributes.Bold;
        BtnProcessed.TextColor = Color.FromArgb("#888888");
        BtnProcessed.FontAttributes = FontAttributes.None;
        UnderlineOriginal.BackgroundColor = Colors.White;
        UnderlineProcessed.BackgroundColor = Colors.Transparent;
    }

    private void ShowPortraitPlaceholder()
    {
        _isPortraitPlaceholderShown = true;
        ImgPortrait.Source = ImageSource.FromFile(PortraitPlaceholderImage);
    }

    // ToImageSource() copies and PNG-encodes on the calling thread, and the originals are full camera
    // frames, so the page opens with the text and each image appears once it has been converted off
    // the UI thread. Until then an image has no source, so long-press saving ignores it.
    private async void LoadImagesAsync(params (Image Target, ImageData? Data)[] images)
    {
        foreach (var (target, data) in images)
        {
            if (data is null) continue;
            // An image that fails to convert is left out rather than failing the whole result.
            var source = await Task.Run(() =>
            {
                try { return data.ToImageSource(); }
                catch { return null; }
            });
            if (source is not null)
                target.Source = source;
            else if (ReferenceEquals(target, ImgPortrait))
                ShowPortraitPlaceholder();
        }
    }

    private static ImageData? TryGetImage(Func<ImageData?> getImage)
    {
        try { return getImage(); }
        catch { return null; }
    }
}
