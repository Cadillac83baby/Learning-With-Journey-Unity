# Learning with Journey UI Voice Audio Review

Date: 2026-09-09

## Review result

All 10 uploaded MP3 files decoded successfully at 44.1 kHz. No hard digital clipping or full-scale discontinuity was detected in the decoded audio. The click/pop scan found no sample jumps above 0.35 (normalized full scale) in any file.

The source exports had unusually long trailing silence—roughly 10.4 seconds on the longer menu/name clips and about 2 seconds on the shorter clips. That would make the game wait unnecessarily after the spoken line. The processed copies remove only leading/trailing silence longer than 0.35 seconds, preserve natural pauses inside the line, convert to mono for consistent voice playback, and normalize near -16 LUFS with a -1.5 dB true-peak ceiling.

## Processed clips

| Clip | Original | Processed | Original peak | Processed peak |
|---|---:|---:|---:|---:|
| MENU_welcome | 14.16 s | 3.68 s | -5.3 dBFS | -2.7 dBFS |
| Menu_Counting | 12.23 s | 1.41 s | -4.7 dBFS | -2.2 dBFS |
| Menu_Matching | 4.37 s | 2.12 s | -3.7 dBFS | -1.2 dBFS |
| Menu_choose | 4.52 s | 2.22 s | -5.6 dBFS | -2.8 dBFS |
| Menu_letters | 3.69 s | 1.88 s | -5.7 dBFS | -3.2 dBFS |
| Name_ask | 13.54 s | 2.93 s | -5.0 dBFS | -3.0 dBFS |
| Name_help | 13.20 s | 2.69 s | -6.6 dBFS | -4.1 dBFS |
| Name_ready | 13.20 s | 2.46 s | -6.8 dBFS | -3.2 dBFS |
| Nav_home | 3.74 s | 1.49 s | -3.2 dBFS | -1.6 dBFS |
| Parent_welcome | 3.77 s | 1.65 s | -6.1 dBFS | -3.6 dBFS |

## Unity mapping

Processed clips are installed under `Assets/LearningWithJourney/Resources/JourneyVoice/UI/`.

- Main Menu welcome: `MENU_welcome`
- Main Menu cues: `Menu_choose`, `Menu_Counting`, `Menu_letters`, `Menu_Matching`
- Name Setup: `Name_ask`, `Name_help`, `Name_ready`
- Parent Zone: `Parent_welcome`
- Home navigation: `Nav_home`

The game still keeps the child's entered name local-only. No child names or recordings are sent anywhere.

Final listening verification should still be performed in Unity Play mode on the target computer/device, with volume and mute tested.
