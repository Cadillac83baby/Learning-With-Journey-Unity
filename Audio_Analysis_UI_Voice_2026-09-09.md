# Learning with Journey UI Voice Audio Review

Date: 2026-09-09

## Review result

All 10 uploaded MP3 files decoded successfully at 44.1 kHz. No hard digital clipping or full-scale discontinuity was detected in the decoded audio. The click/pop scan found no sample jumps above 0.35 (normalized full scale) in any file.

The first processed pass was too aggressive for the intended game timing. The replacement copies below are a gentler revision made from the original uploads: they keep the natural beginning, wait for a full 1.0 second of silence before trimming the empty tail, add 150 ms of lead-in and 350 ms of safety tail, and reduce level only slightly. This keeps word endings and natural pauses intact while avoiding the original 10-second dead air.

## Processed clips

| Clip | Original | Processed | Original peak | Processed peak |
|---|---:|---:|---:|---:|
| MENU_welcome | 14.16 s | 5.30 s | -5.3 dBFS | -6.2 dBFS |
| Menu_Counting | 12.23 s | 3.34 s | -4.7 dBFS | -5.6 dBFS |
| Menu_Matching | 4.37 s | 3.89 s | -3.7 dBFS | -1.6 dBFS |
| Menu_choose | 4.52 s | 3.92 s | -5.6 dBFS | -3.6 dBFS |
| Menu_letters | 3.69 s | 3.21 s | -5.7 dBFS | -3.6 dBFS |
| Name_ask | 13.54 s | 4.60 s | -5.0 dBFS | -5.9 dBFS |
| Name_help | 13.20 s | 4.34 s | -6.6 dBFS | -4.6 dBFS |
| Name_ready | 13.20 s | 4.23 s | -6.8 dBFS | -4.6 dBFS |
| Nav_home | 3.74 s | 3.24 s | -3.2 dBFS | -4.1 dBFS |
| Parent_welcome | 3.77 s | 3.21 s | -6.1 dBFS | -7.1 dBFS |

## Unity mapping

Processed clips are installed under `Assets/LearningWithJourney/Resources/JourneyVoice/UI/`.

- Main Menu welcome: `MENU_welcome`
- Main Menu cues: `Menu_choose`, `Menu_Counting`, `Menu_letters`, `Menu_Matching`
- Name Setup: `Name_ask`, `Name_help`, `Name_ready`
- Parent Zone: `Parent_welcome`
- Home navigation: `Nav_home`

The game still keeps the child's entered name local-only. No child names or recordings are sent anywhere.

Final listening verification should still be performed in Unity Play mode on the target computer/device, with volume and mute tested.
