using Dynamsoft.MRZScannerBundle.Maui;

namespace ScanMRZ;

public partial class MainPage : ContentPage
{
    private const string PortraitPlaceholderImage = "portrait_placeholder.jpg";
    private readonly Dictionary<string, ImageSource?> _imageMap = new();
    private bool _isPortraitPlaceholderShown;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnScanMRZ(object sender, EventArgs e)
    {
        // Initialize the license.
        // The license string here is a trial license. Note that network connection is required for this license to work.
        // You can request an extension via the following link: https://www.dynamsoft.com/customer/license/trialLicense?product=cvs&utm_source=samples&package=mobile
        var config = new MRZScannerConfig("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9");
        var result = await MRZScanner.Start(config);

        if (result.ResultStatus == EnumResultStatus.Finished && result.Data is not null)
        {
            LblStatus.IsVisible = false;
            PopulateResult(result);
        }
        else if (result.ResultStatus == EnumResultStatus.Canceled)
        {
            ResultView.IsVisible = false;
            LblStatus.Text = "Scan canceled";
            LblStatus.IsVisible = true;
        }
        else
        {
            ResultView.IsVisible = false;
            LblStatus.Text = result.ErrorString ?? "Unknown error";
            LblStatus.IsVisible = true;
        }
    }

    private void PopulateResult(MRZScanResult result)
    {
        var data = result.Data!;

        // Header
        LblName.Text = $"{data.FirstName} {data.LastName}";
        LblSexAge.Text = $"{char.ToUpper(data.Sex[0])}{data.Sex[1..].ToLower()}, {data.Age} years old";
        LblExpiry.Text = $"Expiry: {data.DateOfExpire}";

        // Portrait
        try
        {
            var portrait = result.GetPortraitImage();
            _isPortraitPlaceholderShown = portrait is null;
            var src = portrait?.ToImageSource() ?? ImageSource.FromFile(PortraitPlaceholderImage);
            ImgPortrait.Source = src;
            _imageMap[nameof(ImgPortrait)] = src;
        }
        catch
        {
            _isPortraitPlaceholderShown = true;
            var placeholder = ImageSource.FromFile(PortraitPlaceholderImage);
            ImgPortrait.Source = placeholder;
            _imageMap[nameof(ImgPortrait)] = placeholder;
        }

        // Document images
        SetImage(ImgMrzProcessed, () => result.GetDocumentImage(EnumDocumentSide.MRZ)?.ToImageSource());
        SetImage(ImgOppositeProcessed, () => result.GetDocumentImage(EnumDocumentSide.Opposite)?.ToImageSource());
        SetImage(ImgMrzOriginal, () => result.GetOriginalImage(EnumDocumentSide.MRZ)?.ToImageSource());
        SetImage(ImgOppositeOriginal, () => result.GetOriginalImage(EnumDocumentSide.Opposite)?.ToImageSource());

        // Adjust image grid layout based on available images
        AdjustImageGrid(ProcessedImages, ImgMrzProcessed, BorderMrzProcessed, ImgOppositeProcessed, BorderOppositeProcessed);
        AdjustImageGrid(OriginalImages, ImgMrzOriginal, BorderMrzOriginal, ImgOppositeOriginal, BorderOppositeOriginal);

        // Personal Info
        ValGivenName.Text = data.FirstName;
        ValSurname.Text = data.LastName;
        ValDob.Text = data.DateOfBirth;
        ValGender.Text = $"{char.ToUpper(data.Sex[0])}{data.Sex[1..].ToLower()}";
        ValNationality.Text = data.NationalityRaw;

        // Document Info
        ValDocType.Text = data.DocumentType switch
        {
            "MRTD_TD1_ID" => "ID (TD1)",
            "MRTD_TD2_ID" => "ID (TD2)",
            "MRTD_TD3_PASSPORT" => "Passport (TD3)",
            _ => data.DocumentType
        };
        ValDocNumber.Text = data.DocumentNumber;
        ValExpiry.Text = data.DateOfExpire;

        // Raw MRZ Text
        ValMrzText.Text = data.MrzText;

        // Show result
        ResultView.IsVisible = true;
    }

    private void SetImage(Image imageView, Func<ImageSource?> getSource)
    {
        try
        {
            var src = getSource();
            imageView.Source = src;
            _imageMap[imageView.StyleId ?? imageView.GetHashCode().ToString()] = src;
        }
        catch { }
    }

    private static void AdjustImageGrid(Grid grid, Image imgFirst, Border borderFirst, Image imgSecond, Border borderSecond)
    {
        bool hasFirst = imgFirst.Source is not null;
        bool hasSecond = imgSecond.Source is not null;

        if (hasFirst && hasSecond)
        {
            // Two images: two-column layout (default)
            grid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            };
            grid.HeightRequest = 140;
            borderFirst.SetValue(Grid.ColumnProperty, 0);
            borderFirst.SetValue(Grid.ColumnSpanProperty, 1);
            borderSecond.IsVisible = true;
        }
        else
        {
            // One image: full width, taller
            grid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star)
            };
            grid.HeightRequest = 200;

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

    private void OnImagePointerPressed(object? sender, PointerEventArgs e)
    {
        if (sender is not Image image || image.Source is null)
            return;

        if (ReferenceEquals(image, ImgPortrait) && _isPortraitPlaceholderShown)
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
}
