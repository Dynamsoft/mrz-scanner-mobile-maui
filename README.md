# Dynamsoft MRZ Scanner for .NET MAUI

This repository hosts the .NET MAUI samples for the **Dynamsoft MRZ Scanner** — a ready-to-use scanning component for Android and iOS apps. With minimal setup, a single call to `MRZScanner.Start` from the [`Dynamsoft.MRZScannerBundle.Maui`](https://www.nuget.org/packages/Dynamsoft.MRZScannerBundle.Maui) NuGet package opens the built-in scanner UI, reads the Machine Readable Zone (MRZ) on passports and ID cards, and returns the holder's information when the scanner closes.

This repo contains two runnable samples that demonstrate launching the scanner, handling results, and displaying the extracted data — from a minimal single-page app up to a complete **`ScanMRZ`** app.

## Supported Document Types

The SDK recognizes three ICAO Machine Readable Travel Document (MRTD) formats: **TD1** (3-line MRZ ID cards), **TD2** (2-line MRZ ID cards), and **TD3** (2-line MRZ passports). For other MRTD types, contact the [Dynamsoft Support Team](https://www.dynamsoft.com/company/contact/).

## System Requirements

- **.NET 10** SDK or later, with the `maui` workload.
- **.NET MAUI 10.0.20** or later.

### Android

- Supported OS: **Android 5.0** (API Level 21) or higher.
- Supported ABI: **armeabi-v7a**, **arm64-v8a**, **x86** and **x86_64**.
- JDK: **17** or later.

### iOS

- Supported OS: **iOS 16** or higher — required by the Core ML models the scanner ships.
- Supported ABI: **arm64** (devices); **arm64** and **x86_64** (simulators).
- Development Environment: a Mac with the Xcode version your .NET iOS workload requires, and its command line tools.

> [!NOTE]
> Each .NET for iOS release names the Xcode it requires in its [release notes](https://github.com/dotnet/macios/releases). Apple's [Xcode system requirements](https://developer.apple.com/support/xcode/) list the macOS each Xcode needs.

### Development Environment

- **Visual Studio Code** with the **C# Dev Kit** and **.NET MAUI** extensions, on macOS or Windows.
- **Visual Studio 2026** on Windows. Building for iOS from Windows needs a paired Mac.

### Building the samples in this repository

The requirements above are what the **SDK** needs when you add it to your own app. The samples target `net10.0-android` and `net10.0-ios`, reference `Dynamsoft.MRZScannerBundle.Maui` **3.6.2000** from nuget.org, and take MAUI itself from your .NET SDK's `maui` workload. Both deploy to **iOS 16** and **Android 5.0**, the SDK's own minimums.

## Add the SDK

Add the [`Dynamsoft.MRZScannerBundle.Maui`](https://www.nuget.org/packages/Dynamsoft.MRZScannerBundle.Maui) package to your MAUI app. It brings in the Android and iOS bindings and the Capture Vision packages it depends on:

```bash
dotnet add package Dynamsoft.MRZScannerBundle.Maui --version 3.6.2000
```

or, in your `.csproj`:

```xml
<PackageReference Include="Dynamsoft.MRZScannerBundle.Maui" Version="3.6.2000" />
```

The package supports **Android and iOS** only. The MAUI template's `.csproj` has three `<TargetFrameworks>` lines that also add Mac Catalyst and Windows. Replace all three with this one, as both samples do:

```xml
<TargetFrameworks>net10.0-android;net10.0-ios</TargetFrameworks>
```

The SDK also requires **iOS 16** or later. The MAUI 10 template sets the iOS minimum to 15.0 and the build doesn't warn about the difference, so raise it in your `.csproj`:

```xml
<SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">16.0</SupportedOSPlatformVersion>
```

Then configure each platform:

- **iOS** — declare `NSCameraUsageDescription` in `Platforms/iOS/Info.plist`. Without it, the app is terminated the moment the scanner opens the camera.

  ```xml
  <key>NSCameraUsageDescription</key>
  <string>Open Camera to Scan MRZ.</string>
  ```

  `ScanMRZ` also declares `NSPhotoLibraryUsageDescription` and `NSPhotoLibraryAddUsageDescription`, because it can save the scanned images to Photos. You only need them if your app does the same.

- **Android** — no manual camera setup is needed: the SDK's manifest merges the `CAMERA` permission into your app. The license is verified online, so keep the `INTERNET` permission that the MAUI template already declares in `Platforms/Android/AndroidManifest.xml`:

  ```xml
  <uses-permission android:name="android.permission.INTERNET" />
  ```

For full details, see [Add the SDK](https://www.dynamsoft.com/mrz-scanner/docs/mobile/programming/maui/user-guide/index.html#step-2-add-the-sdk) in the user guide.

## Samples

### ScanMRZBasic

The smallest app that scans an MRZ and shows the data — one page that launches the scanner, handles all three result statuses, and lists the parsed fields, any fields that failed their check digits, and the raw MRZ text. It is the MAUI project template plus a single page: the whole integration is in [ScanMRZBasic/MainPage.xaml.cs](ScanMRZBasic/MainPage.xaml.cs), about 50 lines. Start here if you are integrating the SDK for the first time.

### ScanMRZ

A complete end-to-end app that scans the MRZ on a passport or ID card and displays the extracted holder information, document images, and raw MRZ text. On top of what `ScanMRZBasic` covers, it adds:

- A separate result page with the portrait and the processed document images. Turn on `ReturnOriginalImage` and the full camera frames appear in a second tab.
- Per-field validation: fields that fail their check digits are marked, and tapping one explains the warning.
- Saving images: long-press an image to save it to Photos (iOS) or the gallery (Android 10 or later).
- Camera-permission recovery: if the camera is denied, the home page shows the error with an **Open Settings** button.
- Every `MRZScannerConfig` option, commented out at the opposite of its default in [ScanMRZ/MainPage.xaml.cs](ScanMRZ/MainPage.xaml.cs) — uncomment one to try it.

The scan flow and configuration are in [ScanMRZ/MainPage.xaml.cs](ScanMRZ/MainPage.xaml.cs); the result display is in [ScanMRZ/ResultPage.xaml.cs](ScanMRZ/ResultPage.xaml.cs).

Each sample declares its own application ID / bundle identifier (`com.dynamsoft.maui.scanmrzbasic` and `com.dynamsoft.maui.scanmrz`), so you can install them side by side on one device and compare them.

A physical device is required to run either sample — the iOS simulator and most Android emulators do not expose a camera to the SDK.

## Run the Samples

```bash
git clone https://github.com/Dynamsoft/mrz-scanner-mobile-maui.git
cd mrz-scanner-mobile-maui

# Once per machine, if the MAUI workload isn't installed yet (on macOS you may need sudo)
dotnet workload install maui
```

**iOS signing.** The app must be signed before it installs on an iPhone. Sign in to your Apple Developer account in Xcode under **Settings → Accounts**, and make sure a development certificate and provisioning profile exist for the sample's bundle identifier and your device. The simplest way is to let Xcode create them through **Automatically manage signing**, with your team selected and the device connected.

### From an IDE

- **Visual Studio Code** — open the repository folder. Choose the sample with **.NET MAUI: Select Startup Project** in the Command Palette (the run configuration's name does not change it), pick your device with **.NET MAUI: Select Startup Device**, then start debugging. Alternatively, open a sample's own folder, which has only one project to start.
- **Visual Studio 2026** — open the sample's `.csproj`, select your connected device, then start debugging.

### From the command line

Run these from the sample's folder, for example `cd ScanMRZ`.

**Android** — connect a device with USB debugging enabled (`adb devices` lists it), then run:

```bash
dotnet build -t:Run -f net10.0-android
```

**iOS** — connect the iPhone, build, then run:

```bash
dotnet build -f net10.0-ios -p:RuntimeIdentifier=ios-arm64
dotnet build -t:Run -f net10.0-ios -p:RuntimeIdentifier=ios-arm64
```

With more than one device connected, add `-p:_DeviceName=<UDID>` to the run command. `xcrun xctrace list devices` shows each device's UDID in parentheses.

## License

A valid license key is required to use the SDK. Both samples ship with a time-limited public trial license, which needs a network connection, passed to `MRZScannerConfig` in [ScanMRZ/MainPage.xaml.cs](ScanMRZ/MainPage.xaml.cs) and [ScanMRZBasic/MainPage.xaml.cs](ScanMRZBasic/MainPage.xaml.cs):

```csharp
var config = new MRZScannerConfig("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9");
```

To use your own, request a free 30-day trial via the [Request a Trial License](https://www.dynamsoft.com/customer/license/trialLicense/?product=mrz&utm_source=samples&package=maui) link and replace that string. For production license setup, see the [License Activation guide](https://www.dynamsoft.com/mrz-scanner/docs/mobile/programming/maui/user-guide/license-activation.html).

The sample code itself is distributed under the terms in [LICENSE.md](LICENSE.md).

## Documentation

- [User Guide](https://www.dynamsoft.com/mrz-scanner/docs/mobile/programming/maui/user-guide/index.html) — build an MRZ scanning app with the MAUI SDK step by step.
- [Customization Guide](https://www.dynamsoft.com/mrz-scanner/docs/mobile/programming/maui/user-guide/customize-mrz-scanner.html) — configure the scanner through `MRZScannerConfig`: document types, UI controls, and returned images.
- [API Reference](https://www.dynamsoft.com/mrz-scanner/docs/mobile/programming/maui/api-reference/) — `MRZScanner`, `MRZScannerConfig`, `MRZScanResult`, `MRZData`, and the enums they use.
- [Release Notes](https://www.dynamsoft.com/mrz-scanner/docs/mobile/programming/maui/release-notes/maui-3.html)

## Contact Us

For any questions or feedback, you can either [contact us](https://www.dynamsoft.com/company/contact/) or [submit an issue](https://github.com/Dynamsoft/mrz-scanner-mobile-maui/issues/new).
