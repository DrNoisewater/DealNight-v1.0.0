# Deal Night on Android — v1.8.0

This ZIP contains the Unity project, not a prebuilt APK. Build the APK on your computer using Unity 6000.3.23f1. Android builds and phone interaction have not been executed in the delivery environment.

## One-time computer setup

1. Open Unity Hub > Installs. Find Unity 6000.3.23f1, open its gear/three-dot menu and choose Add modules.
2. Install Android Build Support, including Android SDK & NDK Tools and OpenJDK. Use the versions bundled by Unity Hub.
3. Extract the project into a new folder and add DealNight-v1.8.0 in Unity Hub. Open it and wait for import to finish. Exit Play Mode before building.

## Simplest installation: copy the APK

1. In Unity choose Deal Night > Build Android APK. The command switches the project to Android, waits for compilation, and builds. First builds can take several minutes and may download additional Android components.
2. When complete, the file manager opens to Builds/Android/DealNight-1.8.0.apk inside your project.
3. Connect the unlocked phone by USB. Choose File transfer in the phone's USB notification. Copy the APK to the phone's Download folder. You can also transfer it with Quick Share.
4. On the phone open Files or My Files > Downloads and tap the APK.
5. If Android asks, allow this file-manager app to install unknown apps, then return and tap Install. Menu wording varies by phone. You are installing your own locally built APK; keep normal security checks enabled.
6. Open Deal Night from the app drawer. Hold the phone sideways. Confirm Settings displays v1.8.0.

USB debugging is not needed for this copy-and-install method. No Google Play account or Play Store publication is needed.

## Alternative: build and install directly over USB

1. Enable Developer options on the phone. Common path: Settings > About phone > tap Build number seven times. On Samsung, Build number is usually under Software information.
2. In Developer options enable USB debugging.
3. Connect with a data-capable USB cable, unlock the phone, and approve the Allow USB debugging prompt for your computer.
4. In Unity open File > Build Profiles, select Android, and select your connected phone under Run Device. Refresh the device list if needed.
5. Choose Deal Night > Build Android and Run on Phone. Unity builds the APK, installs it on the selected device and launches it.
6. If the phone is not detected, check the cable/USB mode and authorization prompt. Windows may need the phone manufacturer's ADB USB driver. Multiple connected devices require selecting the intended Run Device first.

## Touch controls

- Tap a hand card to inspect and play/bank it. Tap groups to inspect them.
- Tap an incoming action card to read its contextual explanation.
- Tap hint icons to open their help. Tooltips retain the five-second timeout.
- Press and hold Hold to view board below a dialog to hide the dialog completely. Release to restore it. Choices remain blocked while it is hidden.
- Payment/discard cards are tapped to select them; payment still requires confirmation.
- Android Back closes an optional dialog or opens Pause. Required payment/reaction/discard decisions cannot be skipped.
- Layout is landscape, with safe-area padding for cutouts and gesture bars. Settings can be swiped vertically on small screens.
- Backgrounding the app pauses play. Match save/resume after Android kills the process is not implemented; finish a match before fully closing the app.

## Build configuration and updates

The build menu creates a single APK with ARM64 and ARMv7 support, IL2CPP, minimum Android 8.0/API 26, automatic target SDK, landscape rotation and the game icon. Package name: com.noisewaterlabs.dealnight. These are configured targets, not a guarantee of compatibility with every device.

For personal testing Unity uses debug signing unless you configure a custom keystore. Keep using the same signing key/computer to install updates over your existing app. Different signing keys cannot update the same installed package. Keep future Android version codes increasing (current code: 180). Store publishing and production signing are separate steps.

Windows building remains available through Deal Night > Build Windows. Switch back to Windows in Build Profiles if you want Windows as the active Editor target.

## Official references

- Unity Android setup: https://docs.unity3d.com/6000.3/Documentation/Manual/android-sdksetup.html
- Unity Android build/run: https://docs.unity3d.com/6000.3/Documentation/Manual/android-BuildProcess.html
- Android USB device setup: https://developer.android.com/studio/run/device
- Android app installation: https://support.google.com/pixelphone/answer/7391672
