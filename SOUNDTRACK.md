# Original soundtrack revision — v1.9.0

| Track | Duration | Tempo | Arrangement focus |
| --- | --- | --- | --- |
| Lobby at Dusk | 1:39 | 120 BPM | Muted-brass-like lead, piano punches and walking bass |
| Penny Arcade Promenade | 1:42 | 116 BPM | Reed-like phrases with plucked rhythm accents |
| The Velvet Exchange | 1:58 | 100 BPM | Restrained acoustic-style piano ensemble |
| Sidewalk Serenade | 1:46 | 112 BPM | Reed-like lead and short piano answers |
| Last Tram Home | 2:03 | 96 BPM | Spacious muted-brass-like evening swing |

The new arrangements replace the earlier five tracks. They use synthesized acoustic-style instruments, sparse call-and-response phrases, softened brush-like percussion, mid-focused voicing and modest room ambience. They are original deterministic compositions, not recordings of a live band.

## Reference analysis and limits

The three user-uploaded Monopoly Tycoon MP4s were decoded for computational rhythm, dynamics and spectral analysis. The 1930s recording yielded tempo candidates near 117.5 BPM and its half-time equivalent; the menu recording near 123 BPM; the 1940s estimate was less certain, with peaks near 99.4 and 136 BPM. Those are estimates, not verified tempos. The references were predominantly bass/midrange in measured spectral energy.

These measurements informed a warmer period-swing direction. They cannot establish instrumentation, musical similarity or listening quality on their own. Direct auditory evaluation was unavailable. The new music was not transcribed from the references, and the generator never reads or samples them. No uploaded recording is included in this project.

## Playback and verification

Five stereo 32 kHz OGG files, approximately 9 minutes 8 seconds total. Full decoded durations match generated durations and samples pass finite-value/clipping checks. See soundtrack-report.json. Music retains streaming import settings and two-second transitions.

Defaults: enabled, shuffle, 20% music volume and response ducking. Existing saved settings are retained. Enable/disable, volume, mode, track picker and Next remain in Settings > Music & soundtrack.

The reproducible generator is Tools/compose_soundtrack.py and requires numpy, scipy and ffmpeg when rerendering. The Windows game does not require those tools.

Unity playback and the Windows build have not been executed here. Musical feel still needs user listening feedback.
