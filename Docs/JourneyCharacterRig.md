# Journey Character Rig Specification

## Goal

Journey must read as a real animated game character, not a flat PNG cutout. Use the approved illustrated Journey design as the visual reference, but rebuild the production character as layered artwork suitable for Unity 2D skeletal animation.

## Required layers

### Head / face
- Back hair / puff
- Front hair / edges
- Head / skin
- Left eyebrow
- Right eyebrow
- Left eye white/iris/pupil
- Right eye white/iris/pupil
- Left eyelid
- Right eyelid
- Nose
- Mouth: closed smile
- Mouth: open smile
- Mouth: A/Ah
- Mouth: E
- Mouth: O
- Mouth: M/B/P

### Body
- Torso / pink shirt
- Shorts
- Left upper arm
- Left forearm
- Left hand
- Right upper arm
- Right forearm
- Right hand
- Left thigh
- Left lower leg
- Left shoe
- Right thigh
- Right lower leg
- Right shoe

Optional secondary-motion layers: shirt hem, hair curls, bow/accessories if used.

## Skeleton

Recommended bone hierarchy:

```text
Root
└── Hips
    ├── Torso
    │   ├── Neck
    │   │   └── Head
    │   ├── LeftUpperArm
    │   │   └── LeftForearm
    │   │       └── LeftHand
    │   └── RightUpperArm
    │       └── RightForearm
    │           └── RightHand
    ├── LeftThigh
    │   └── LeftLowerLeg
    │       └── LeftFoot
    └── RightThigh
        └── RightLowerLeg
            └── RightFoot
```

## Animator parameters

Create Trigger parameters with these exact names so the included `JourneyAnimatorController.cs` works without code changes:

- Idle
- Walk
- Wave
- Talk
- Point
- Think
- Clap
- Celebrate
- TryAgain
- Jump

Create one Bool:

- Talking

## Animation clips

### Idle
2–4 second loop. Breathing, blink, tiny head/hand movement. No sliding of the entire character.

### Walk
Natural alternating steps, arm swing, body rise/fall and foot contact. Use root movement only if the scene needs Journey to physically travel.

### Wave
Actual shoulder/elbow/wrist movement. Keep face engaged with a smile and blink.

### Talk
Small head gestures, blink, hand gesture. Mouth shapes can be driven separately for lip sync.

### Point
Journey points toward the current game object, letter, number, or matching card.

### Think
Curious head tilt and hand/chin gesture.

### Clap
Hands visibly meet and separate. Use for correct answers.

### Celebrate
Happy bounce/jump, arms up, stars/particles triggered by gameplay code.

### Try Again
Supportive reaction, not sad or punitive. Gentle shrug/head tilt and encouraging smile.

### Jump
Short joyful jump used for level-up or major reward moments.

## Voice / lip sync

Journey's real recordings should be imported as AudioClips. `JourneyAnimatorController.Speak()` already turns the `Talking` bool on for the duration of the clip. For production lip sync, add mouth-shape animation driven by phoneme timing or amplitude analysis while preserving the `Talking` bool as the master state.

## Import quality

- Keep original character art at high resolution.
- Do not upscale small raster images for final production.
- Use Sprite Mode: Multiple for a layered sheet, or import each body part as a separate transparent PNG.
- Compression: None or High Quality during character development.
- Filter Mode: Bilinear.
- Mesh Type: Tight where safe; avoid clipping curls, hands, or shoes.
- Keep Pixels Per Unit consistent across all character layers.
