# Deal Night — Version 1.0.0

This is the first numbered release after the 1.x development builds. Windows and Android share the same rules and tutorial, with the existing phone-specific layout retained.

## Open and build

1. Extract into a fresh folder and add `DealNight-v1.0.0-Release` in Unity Hub.
2. Open with Unity 6000.3.23f1 and open `Assets/Scenes/Main.unity`.
3. Verify Settings shows **v1.0.0**.
4. Windows: choose **Deal Night > Build Windows**. Keep the entire `Builds/Windows` folder together.
5. Android: install Android Build Support, SDK & NDK Tools, and OpenJDK for this editor in Unity Hub. Choose **Deal Night > Build Android APK** or **Build Android and Run on Phone**. Output: `Builds/Android/DealNight-1.0.0.apk`. Android version code is 201 so this can update the earlier development build with the same signing key.

This ZIP contains the Unity source project, not a precompiled EXE or APK. Unity is unavailable in the delivery environment.

## Learn to play

**Guided tutorial** on the main menu contains 15 staged practice scenes and 43 actual game actions against **Dr. Noisewater**. The coach's moves, responses and scene cards are scripted. A first-play invitation follows name/piece setup; existing players are offered the tutorial when they first choose New game in this release. Skipping remains possible, and the tutorial can be replayed from the menu.

Each scene explicitly explains its setup. Perform the requested action through the normal game controls. Use **Show this step** to locate the appropriate card or response screen. Unrelated actions are held back so the sequence stays teachable. Completing a step shows the outcome before proceeding. The tutorial uses the normal rules validator; it is separate from regular matches.

**Rules & how to play** is now an illustrated, tabbed table guide: Basics, Bank & payment, Rent, Actions & defense, Wilds & buildings.

## Clarity and feedback

- Remaining plays centered directly above your hand; player/turn centered at the top.
- Exhaustion dims the hand container, including cards revealed after draw animations.
- Rent shows named colors and ONE versus ALL opponents. Any-color rent and universal property wilds use a full-color gradient.
- Charge suggestions consider collectible value, bank shortfall, possible property/building payments and double-rent costs. They do not see hidden hands or promise a specific received property.
- Incoming payments show your bank. Rejected actions explain the protected outcome and whether a counter would restore the original action.
- Wildcard assignment uses a small card preview and an explicit assignment sentence.
- Slower bank-gain feedback; default music volume 15% (formerly 20%). Existing saved volume choices are preserved.
- Expanded pool of 126 opponent names; regular matches still exclude your name and duplicates within a match.

## Validation

39 engine checks passed, including all 43 tutorial actions, rent metadata and recommendation cases. Thirty seeded AI matches completed (4,518 commands). Card conservation is checked through tutorial actions. See TEST-RESULTS.md.

Unity compilation, actual UI rendering and phone/Windows interaction could not be tested here. Review the manual checks before distributing a build.

This remains an unofficial personal fan game. Distribution rights have not been cleared.
