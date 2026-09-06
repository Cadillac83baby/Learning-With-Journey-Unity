# Learning with Journey — Project Checkpoint

Saved: 2026-09-06

## Approved / current screens
- Main Menu: approved baseline. Do not redesign unless requested.
- Counting World: approved current baseline. Different object each round, 10 levels, 5 successful rounds per level, Journey speech, saved progress, canonical backpack placement.
- ABC World: approved current baseline. Clear A–Z pictures, Journey speaks letter/word/phrase, 10 levels, 5 successful rounds per level, readable text, TAP LETTER placement fixed.
- Alphabet Match: built and functional. Uses Learning with Journey branded card backs generated directly in Unity UI (no sprite dependency). Current polish pass: V7 larger logo treatment.
- Rewards: current active screen. Scene remains named RewardsRoom for routing compatibility, but visible title is REWARDS. Current code includes treasure opening, prize reveal, stars, Journey Coins, level, progress markers, Journey speech, and bottom navigation.

## Latest Rewards work
- Base builder: `Assets/LearningWithJourney/Editor/LWJRewardsBuilderV1.cs`
- Runtime: `Assets/LearningWithJourney/Scripts/UI/RewardsScreenControllerV1.cs`
- Visual polish V2: rewards/trophy room background + more dimensional chest.
- Layout/speech V3: cleaner Rewards layout, real speech-bubble styling with tail, reduced clutter, improved opened-chest/prize positioning.
- Most recent runtime update also tightened chest-lid opening so it does not float too far away.

## Visual rules to preserve
- Journey keeps her current character design.
- Keep backpack over the damaged screen-right shorts/leg area; Main Menu position is canonical.
- Android and Apple/iOS should receive the same premium graphics/features. Android is current build/test priority, but code/UI should stay cross-platform and iOS-ready.
- Use commercial preschool mobile-game quality: glossy, dimensional, colorful, readable, touch-friendly.
- Do not generate new mockup/reference images unless Davida explicitly asks. Make requested visual changes in Unity code.

## Remaining screen order
1. Finish/approve Rewards
2. Library
3. Parent Zone
4. Supporting polish / settings / release preparation

## Unity startup
```powershell
cd "C:\Users\burks\Learning-With-Journey-Unity"
git pull
& "C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe" -projectPath "C:\Users\burks\Learning-With-Journey-Unity"
```

## Important project info
- Repository: `Cadillac83baby/Learning-With-Journey-Unity`
- Unity: 6.3 LTS (6000.3.23f1)
- Portrait target: 1080x1920
- Scenes: MainMenu, CountingWorld, ABCWorld, AlphabetMatchWorld, RewardsRoom, Library, ParentZone

This checkpoint is intentionally committed to GitHub so work can be resumed even if the local computer loses power.
