# Android — v1.0.0

1. Extract this ZIP into a new folder and open the project in Unity 6000.3.23f1.
2. In Unity Hub, ensure that editor has Android Build Support, Android SDK & NDK Tools, and OpenJDK installed.
3. Choose **Deal Night > Build Android APK**. Wait for platform switching and compilation. Output: `Builds/Android/DealNight-1.0.0.apk`.
4. Copy the APK to your phone, open it in Files, and allow installation from that source if Android asks. Alternatively enable Developer options > USB debugging, connect by USB, accept the phone's prompt, then choose **Build Android and Run on Phone**.
5. Verify **v1.0.0** in Settings. Keep the same signing key when updating an existing installation. Do not uninstall if you want to retain saved preferences.

Check on your phone: title buttons fit, opponent groups are readable, page arrows reveal additional groups, tapping opens individual cards, and holding View board fully hides the menu until release. Check both landscape orientations and any screen cutout.

Windows layout is unchanged. Unity compilation and device tests were not possible in the delivery environment.
