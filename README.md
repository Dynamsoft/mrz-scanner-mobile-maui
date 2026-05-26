# Dynamsoft MRZ Scanner — .NET MAUI Sample (ScanMRZ)

This repo hosts **ScanMRZ**, a complete .NET MAUI sample app demonstrating the Dynamsoft MRZ Scanner ready-to-use scanning UI. It scans the Machine Readable Zone (MRZ) on passports and ID cards and renders the parsed fields, captured document images, and portrait photo.

The sample is built around `MRZScanner.Start` from the [`Dynamsoft.MRZScannerBundle.Maui`](https://www.nuget.org/packages/Dynamsoft.MRZScannerBundle.Maui) NuGet package, which packages the camera UI, scanning logic, and result delivery into a single launch call.

## Features

- Scans the MRZ on passports and ID cards through the ready-to-use `MRZScanner` UI.
- Parses the MRZ into structured fields — name, date of birth, document number, expiry, nationality, and more.
- Returns the portrait image extracted from the document, plus processed and original captures of both the MRZ side and the opposite side.

The full scan flow lives in a single file: [ScanMRZ/MainPage.xaml.cs](ScanMRZ/MainPage.xaml.cs).

## Quick Start

```bash
# 1. Clone the repo
git clone https://github.com/Dynamsoft/mrz-scanner-mobile-maui-dev.git
cd mrz-scanner-mobile-maui-dev/ScanMRZ

# 2. Install the .NET MAUI workload (if you haven't already)
dotnet workload install maui

# 3. Restore NuGet dependencies
dotnet restore
```

All remaining commands in the sections below run from the `ScanMRZ` directory.

> [!IMPORTANT]
> Both platforms must be run on a **physical device**. The iOS simulator and most Android emulators do not expose a working camera to the SDK.

## Configure Your License

A valid license key is required to use the SDK. The sample ships with a public trial license string in [ScanMRZ/MainPage.xaml.cs:21](ScanMRZ/MainPage.xaml.cs#L21):

```csharp
var config = new MRZScannerConfig("DLS2eyJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSJ9");
```

This trial string is time-limited and **requires a network connection**. To request your own 30-day trial, visit [Request a Trial License](https://www.dynamsoft.com/customer/license/trialLicense?product=mrz&utm_source=samples&package=mobile), then replace the license value in `MainPage.xaml.cs` with the key you receive.

## Run the Sample

The sample targets `net10.0-android` and `net10.0-ios`. You can run it from an IDE or the command line.

### iOS

The iOS project must be signed before it will install on a device:

1. Open Xcode and sign in to your Apple Developer account under **Settings → Accounts**.
2. Ensure a development provisioning profile and signing certificate exist for the app's bundle id (`com.dynamsoft.scanmrz`) and your test device. The simplest way is to let Xcode generate them via **Automatically manage signing** with your team selected and the device connected.

The csproj is already configured for automatic provisioning ([ScanMRZ/ScanMRZ.csproj](ScanMRZ/ScanMRZ.csproj)):

```xml
<CodesignProvision>Automatic</CodesignProvision>
<CodesignKey>Apple Development</CodesignKey>
```

Connect a physical iOS device, then launch using either option below.

**Option A — Run from an IDE.** In VS Code (with the **C# Dev Kit** and **.NET MAUI** extensions) or Visual Studio 2022, select your connected device and start debugging.

**Option B — Run from the command line:**

```bash
dotnet build -t:Run -f net10.0-ios -p:RuntimeIdentifier=ios-arm64
```

### Android

Connect a physical Android device with USB debugging enabled, then run:

```bash
dotnet build -t:Run -f net10.0-android
```

You can list connected devices with `adb devices`.

## Supported Document Types

The SDK supports the three ICAO Machine Readable Travel Document (MRTD) formats:

- **TD1** — ID cards with a 3-line MRZ.
- **TD2** — ID cards with a 2-line MRZ.
- **TD3** — passports with a 2-line MRZ.

For support for other MRTD types, contact the [Dynamsoft Support Team](https://www.dynamsoft.com/contact/).

## System Requirements

The toolchain versions below correspond to what this sample is pinned to (**.NET 10**, `Dynamsoft.MRZScannerBundle.Maui` **3.4.1310**). If you adapt the sample to a different .NET version, your SDK, workload, and Xcode requirements are dictated by *that* version — check its release requirements rather than the values here.

- .NET SDK **10.0** with the `maui` workload installed.
- **Android**
  - Supported OS: Android 5.0 (API Level 21) or higher.
  - Supported ABIs: armeabi-v7a, arm64-v8a, x86, x86_64.
  - JDK 17.
- **iOS**
  - Supported OS: iOS 15.0 or higher.
  - Supported ABIs: arm64.
  - Development environment: Xcode 16.1 or higher (a matching .NET-for-iOS SDK is required for newer Xcode releases).

## Project Structure

```
ScanMRZ/
├── MainPage.xaml                          # Scan button + result UI layout
├── MainPage.xaml.cs                       # Scan flow + result handling (entire app logic)
├── App.xaml / AppShell.xaml               # MAUI app + shell entry points
├── MauiProgram.cs                         # MAUI host builder (fonts, logging)
├── Platforms/
│   ├── Android/                           # Android head (AndroidManifest.xml, MainActivity)
│   └── iOS/                               # iOS head (Info.plist, AppDelegate)
├── Resources/
│   └── Images/portrait_placeholder.jpg    # Fallback when no portrait is captured
└── ScanMRZ.csproj                         # Pinned to Dynamsoft.MRZScannerBundle.Maui 3.4.1310
```

## Platform Configuration

The sample is already configured for both platforms. The notes below explain what is wired up, in case you adapt the sample code into your own project.

- **iOS** — [ScanMRZ/Platforms/iOS/Info.plist](ScanMRZ/Platforms/iOS/Info.plist) declares `NSCameraUsageDescription`. Without this, the app would crash the moment the scanner tries to open the camera.

  ```xml
  <key>NSCameraUsageDescription</key>
  <string>Open Camera to Scan MRZ.</string>
  ```

- **Android** — no manual camera permission changes are required. The SDK's manifest merges the `CAMERA` permission automatically. The trial license needs network access, so the `INTERNET` permission in [ScanMRZ/Platforms/Android/AndroidManifest.xml](ScanMRZ/Platforms/Android/AndroidManifest.xml) is left in place:

  ```xml
  <uses-permission android:name="android.permission.INTERNET" />
  ```

## Documentation

- **[User Guide](https://www.dynamsoft.com/mrz-scanner/docs/mobile/programming/maui/user-guide/index.html)** — step-by-step walkthrough of building an MRZ scanning app with the MAUI SDK from scratch.
- **[Customization Guide](https://www.dynamsoft.com/mrz-scanner/docs/mobile/programming/maui/user-guide/customize-mrz-scanner.html)** — deeper customization of `MRZScannerConfig`, including document-type filtering, UI button visibility, and image capture settings.
- **[API Reference](https://www.dynamsoft.com/mrz-scanner/docs/mobile/programming/maui/api-reference/)** — full type definitions for `MRZScanner`, `MRZScannerConfig`, `MRZScanResult`, and `MRZData`.

## Support

For help, integration questions, or custom requirements, contact the [Dynamsoft Support Team](https://www.dynamsoft.com/contact/).

## License

See [LICENSE.md](LICENSE.md). The sample code in this repository is distributed under the terms of the [Dynamsoft Software License Agreement](https://www.dynamsoft.com/company/license-agreement/).
