# Version 1.0.0 validation

Core C# compiled with Mono; 39 rule checks passed. The tutorial traversed all 15 scenes / 43 actions using the real Game.Execute validator and card conservation assertions. Thirty seeded AI matches completed: 4,518 total commands, longest 332 commands.

The presentation source produced no C# parser errors in a syntax pass; Unity assemblies are unavailable, so this is NOT a Unity API/compile verification. No EXE/APK or UI runtime test was possible here.

## Manual checks still required in Unity

- Open project without Console errors; verify v1.0.0 in settings and title.
- Fresh profile: enter name/piece, accept and decline tutorial invitations; replay from menu.
- Complete tutorial using ordinary buttons; try a wrong action, Help, pause/resume, and exit/restart. Verify Dr. Noisewater always appears.
- At zero plays after Pass Go, every hand card remains dim after its animation finishes.
- Check centered plays/turn text, history spacing and pointer/touch peek on both platforms.
- Check rent colors, charge scope and amounts in inspect, choices, confirmations and incoming screens.
- Check No/counter/No rejection summaries, set previews and bank balance on payment requests.
- Inspect all illustrated rules tabs at desktop and phone sizes.
- Check full-color gradients, wildcard assignment preview, delayed bank gains, default music at 15% on a fresh profile and preserved volume on an existing profile.
- Build Windows and Android from the menu; Android package code is 201.

## Automated results

```
PASS Deck
PASS Initial
PASS PlayLimit
PASS HandLimit
PASS Payment
PASS Overpay
PASS EmptyPayment
PASS Wildcards
PASS Steals
PASS Swap
PASS BuildingsAndBreaker
PASS RentDouble
PASS Reactions
PASS Reshuffle
PASS WinTiming
PASS Privacy
PASS Birthday
PASS Universal
PASS BuildingRestrictions
PASS ExactDistribution
PASS ValuedWildBank
PASS PropertyPayment
PASS InsufficientSelectionRejected
PASS RentTargets
PASS MultiTargetNo
PASS Consolidate
PASS DormantBuildings
PASS ResponsiveGrid
PASS PlayerIdentity
PASS WildTransferAndRotate
PASS PlacementDecisions
PASS SmartPayments
PASS SameColorWildcardMoveIsFree
PASS OpponentIdentities
PASS AnimationSpeedSteps
PASS ContextPayment
Tutorial actions: 43
PASS GuidedTutorial
PASS ChargeRecommendations
PASS ChargeTargetRanking
PASS 30 seeded AI matches; max commands=332; total commands=4518
```
