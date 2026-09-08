# Learning with Journey — Main Menu Bookbag Across Scenes

Date: 2026-09-08

## Approved decision

Use the approved Main Menu Journey bookbag artwork on every scene where Journey carries a bookbag. Preserve each scene's existing RectTransform position, size, parent, scale, and gameplay listeners.

## Implementation

Editor tool:

- `Assets/LearningWithJourney/Editor/LWJMatchMainMenuBookbagAllScenesV1.cs`
- Unity menu: **Learning with Journey → Match Main Menu Bookbag Across All Scenes V1**
- Git commit: `dae1b771e09bb9116db8bb459180613d3f5232b8`

The tool applies the Main Menu design layers: purple shoulder straps, top handle, pink flap, darker front pocket, highlight, gold edge, and white **J** badge. It creates a timestamped `LWJSceneBackups/MainMenuBookbagAllScenes-...` backup before saving scenes.

## Test procedure

1. In the personal terminal:
   `cd "C:\\Users\\burks\\Learning-With-Journey-Unity"`
2. Run `git pull`.
3. In Unity, stop Play mode.
4. Choose the editor menu item above.
5. Test MainMenu, Library, BookReader, ParentZone, RewardsRoom, ABCWorld, CountingWorld, and AlphabetMatchWorld in Game view.
6. Confirm each bag matches the Main Menu design while remaining correctly positioned.

This environment cannot run Unity, so final visual and interaction verification must be completed in the user's Unity editor.
