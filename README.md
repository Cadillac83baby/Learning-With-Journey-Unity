# Learning with Journey — Unity Production Build

This repository is the production Unity project for **Learning with Journey**.

## Product direction

This build is intentionally different from the earlier HTML/CSS prototype. It is structured as a real Unity mobile game so Journey can become a properly animated 2D/2.5D character rather than a flat image being moved around the screen.

### Core games

1. **Counting World** — numbers 1–20, animated objects, spoken prompts, correct/try-again reactions.
2. **ABC World** — letter recognition, sounds, beginning-letter vocabulary, spoken prompts.
3. **Alphabet Match World** — memory-card matching between letters and pictures whose names begin with those letters.

### Supporting areas

- Main Menu
- Rewards Room
- Learning Library
- Parent Zone
- Stars, Journey Coins, levels, streaks, daily rewards, badges, saved progress
- Journey voice-prompt hooks
- Android-first mobile layout

## Journey character standard

Journey should **not** be animated as one PNG cutout. The production character is intended to use Unity 2D Animation with a layered character rig: head/face/hair, eyes and eyelids, mouth shapes, torso, upper/lower arms, hands, thighs, lower legs and shoes. See `Docs/JourneyCharacterRig.md`.

The code already exposes animation states for:

- Idle
- Walk
- Wave
- Talk
- Point
- Think
- Clap
- Celebrate
- Try Again
- Jump

## Recommended Unity version

Use **Unity 6 LTS**. If Unity Hub offers to upgrade the project to a newer Unity 6 LTS patch, that is expected.

## First open

1. Clone this repository.
2. Add the project folder in Unity Hub.
3. Open it in Unity 6 LTS.
4. The editor bootstrap creates the initial scenes and Build Settings entries automatically if they are missing.
5. Open `Assets/LearningWithJourney/Scenes/MainMenu.unity` and press Play.

## Android

The project is designed for portrait Android deployment. After the prototype scenes are validated, use **File → Build Profiles → Android**, switch platform, then build an Android App Bundle (`.aab`) for Google Play.

## Important

Do not commit passwords, Play signing keys, D-U-N-S information, tax information, API secrets, or private account credentials to this repository.
