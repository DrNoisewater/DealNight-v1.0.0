# Version 1.0.0 — First release

Development version numbers are replaced by the requested 1.0.0 release label; Android build code advances to 201 for updates.

- Added 15 deterministic tutorial scenes with 43 validated actions against Dr. Noisewater, main-menu access, first-play invitation, step guidance, and outcome explanations.
- Replaced plain rules text with five illustrated guide tabs.
- Moved remaining plays above the human hand and centered player/turn in the header.
- Fixed draw-animation opacity overriding exhaustion: the whole hand now dims together.
- Corrected rent grammar, named each allowed color, distinguished one-target and all-target charges, and carried exact card identity/scope through incoming effects.
- Added rainbow backgrounds for any-color rent and universal property wildcards; paired-color rent also uses its two-color gradient.
- Added wildcard assignment previews, public-state charge recommendations, bank shortfall explanations, and your bank balance on incoming payment requests.
- Rejected actions explain what was prevented and clarify that countering restores the original action.
- Removed extra label spacing in player status/history, added right-mouse-button icon hint, slowed bank gains, lowered fresh-install music default to 15%, and expanded random names.

Existing Windows/Android build commands, game rules and music tracks retained. Existing saved volume is not overwritten.

# v1.10.0 — Android readability and touch controls

- Reduced outer, header and center spacing while respecting screen cutouts.
- Smaller hand region gives opponents more space.
- Compact property summaries show color, count, completion and rent; tap to inspect full cards.
- Group-page arrows prevent crowded boards from shrinking every group indefinitely.
- Title menu centers within the available height with a single-line phone title.
- Hold-to-view uses dedicated pointer capture, hides the entire overlay, and restores on release, cancellation or focus loss. The invisible overlay still blocks accidental board moves.
- Restored Android build commands; version code 200, app version 1.10.0.
- Windows visual rules and music assets retained.

Validation: checked source isolation against v1.9.0 and archive integrity. Unity compilation and real-device touch/rendering remain unverified.

# Deal Night 1.9.0 — Windows soundtrack revision

- Replaced all five music tracks with newly composed synthesized period-style swing arrangements, informed by computational analysis of the uploaded references.
- Warmer piano/plucked rhythm, muted-brass-like and reed-like voices, shorter phrases, breathing room and soft percussion. No reference audio is shipped or sampled.
- About 9 minutes 8 seconds of music. Default volume is 20% for fresh installations; saved volume and playback preferences stay intact.
- Windows build menu now switches back from Android and waits for recompilation before building, then opens the output folder.
- Android menus are deferred behind an opt-in scripting symbol, with older instructions retained separately.

All five OGG files decoded to their full expected lengths with no clipping. No gameplay changes or additional game simulations. Unity compilation/playback and Windows executable testing were unavailable; direct listening evaluation was also unavailable. The local Mono parser could not start during the final source check, so no new C# compilation claim is made.
