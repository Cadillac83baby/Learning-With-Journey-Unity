# Loading Screen Audio and Progress Bar Checkpoint

Date: 2026-09-08

## Audio verification

The saved DSHMENT loading audio decodes successfully as 48 kHz stereo PCM WAV, 4.25 seconds long. Measured levels are mean -20.8 dB and peak -0.5 dB. The Unity loading controller plays the clip once, with looping disabled, and holds the Splash screen for the clip length plus 0.15 seconds (about 4.4 seconds).

This verifies the file and code configuration. Final audible verification must be run in Unity Play mode on the user's computer.

## Progress bar

- Runtime: `Assets/LearningWithJourney/Scripts/UI/SplashLoadingBarV1.cs`
- Editor installer: `Assets/LearningWithJourney/Editor/LWJApplyGreenLoadingBarV1.cs`
- Unity menu: **Learning with Journey → Apply Green Loading Bar V1**
- Bar: dark green track with bright green fill, white-green edge, and smooth unscaled-time fill.
- When `SplashControllerV2` has the DSHMENT clip connected, the fill duration is automatically set to at least the clip duration plus the tail.

## User test

1. Pull main in the personal terminal.
2. If needed, run **Apply Black Branded Loading Screen V2** after installing the DSHMENT audio.
3. Run **Apply Green Loading Bar V1**.
4. Play from AccessGate/Splash and confirm the tag plays once, the bar fills green, and the next scene loads after the audio finishes.
