# Manual Exerciser Animation Setup

This folder contains the animation assets for the Manual Exerciser building.

## Structure

```
anim/
├── assets/
│   └── manualexerciser/
│       ├── build.yaml          # Defines the sprite frames
│       ├── anim.yaml           # Defines the animation states
│       └── [PNG files]         # Your recolored PNG frames
└── preview.png                 # Preview image (optional)
```

## Setup Instructions

1. **Place your PNG files** in the `anim/assets/manualexerciser/` folder
   - Name them: `manualexerciser_0.png`, `manualexerciser_1.png`, etc.
   - Frame 0 should be the "off" state (idle)
   - Frames 1-N should be the spinning animation loop

2. **Update `build.yaml`**:
   - Adjust the frame count to match your actual PNG files
   - The time values are normalized (0.0 to 1.0) for a full rotation
   - If you have N frames, divide 1.0 by N for each frame's time increment

3. **Update `anim.yaml`**:
   - The "off" state uses frame 0
   - The "working" animation loops through frames 1-N
   - Adjust frame numbers to match your actual frame count

4. **Compile the animation**:
   - Use the ONI KAnim tools to compile the YAML files into `.bytes` files
   - The tools will generate `manualexerciser_anim.bytes` and `manualexerciser_build.bytes`

5. **Update the building config**:
   - Change `ManualExerciserConfig.cs` to use `"manualexerciser_kanim"` instead of `"generatormanual_kanim"`

## Frame Naming Convention

Based on typical ONI animations:
- `manualexerciser_0.png` = Off/idle state
- `manualexerciser_1.png` through `manualexerciser_N.png` = Working/spinning frames

The exact frame count depends on your animation. A typical spinning animation might have 8, 16, 24, or 30 frames for a smooth loop.

## Notes

- The animation structure matches the ManualGenerator, just with different colored sprites
- Frame timing is normalized (0.0 to 1.0) - adjust based on your desired animation speed
- The "working" animation should loop seamlessly (last frame should transition smoothly to first frame)

