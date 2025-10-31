# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# Camera Auto Exposure Controller

> Quick overview: Automatic camera exposure adjustment driven by scene luminance, with target luminance band, asymmetric reaction speeds, and shutter/ISO bias.

Automatic exposure adjustment is applied based on measured scene luminance. Shutter and ISO multipliers are adjusted toward a configurable luminance window using separate speeds for dark‑to‑light and light‑to‑dark transitions, with optional speed scaling and bias. When the image is within the target range, ISO reduction is favored to limit noise.

![screenshot](Documentation/Screenshot.png)

## Features
- Auto exposure toggle
  - Enable/disable automatic adjustments at runtime
- Target luminance band
  - Minimum/maximum target luminance and a tolerance band to avoid oscillation
- Asymmetric response and speed scaling
  - Independent rates for dark→light and light→dark changes
  - Additional scale factor based on distance from target (up to a maximum)
- Shutter and ISO bias
  - Bias multipliers to prefer shutter or ISO when brightening
  - ISO gently reduced when exposure is already in range to minimize noise
- Companion component integration
  - Works with `CameraLuminanceCalculator` to read current luminance
  - Works with `CameraPhysicalPropertiesController` to apply shutter/ISO multipliers
- Lightweight runtime
  - Simple per‑frame adjustment in `Update`, no Post‑Processing requirement

## Requirements
- Unity 6000.0+ (per package manifest)
- Components on the same Camera:
  - `CameraLuminanceCalculator` (provides current scene luminance)
  - `CameraPhysicalPropertiesController` (applies shutter/ISO multipliers)
- A Camera configured for the scene you wish to meter; no specific pipeline dependency

## Usage
1) Add to a Camera
   - Select your Camera and add `CameraAutoExposureController`
   - The required components (`CameraLuminanceCalculator`, `CameraPhysicalPropertiesController`) will also be present/added
2) Configure settings
   - Toggle Auto Exposure on/off
   - Set Target Luminance: `MinTargetLuminance`, `MaxTargetLuminance`, and `LuminanceTolerance`
   - Tune response: `AdjustmentSpeedLightToDark`, `AdjustmentSpeedDarkToLight`, and `MaxAdjustmentSpeed`
   - Bias toward shutter/ISO as desired via `ShutterBias` and `IsoBias`
3) Play and observe
   - Exposure multipliers are adjusted each frame to steer luminance into the target band
   - When within range, ISO is gradually reduced to limit noise

## How It Works
- Luminance sampling
  - Current luminance is read from `CameraLuminanceCalculator` each frame (scaled internally for controller use)
- Target evaluation
  - A target luminance is selected by clamping the current value into `[MinTarget, MaxTarget]`
  - If the absolute difference exceeds `LuminanceTolerance`, an adjustment step is applied
- Adjustment step
  - A scale factor increases with distance from target (clamped by `MaxAdjustmentSpeed`)
  - If too dark, shutter and ISO multipliers are increased (with `ShutterBias`/`IsoBias`)
  - If too bright, shutter and ISO multipliers are decreased (favoring reduced ISO)
  - Multipliers are clamped to `[0, 1]` and applied via `CameraPhysicalPropertiesController`
- Noise handling
  - When luminance sits within the target band (with a small extra tolerance), ISO is gently decreased

## Notes and Limitations
- Single‑source control: If another system also modifies exposure (e.g., Post‑Processing auto exposure), results may conflict; use one system at a time
- Normalization: Shutter/ISO multipliers are normalized (0..1); their physical meaning depends on `CameraPhysicalPropertiesController`
- Tuning guidance: Prefer lower ISO (noise reduction) and adjust shutter first for brightness changes, using biases to reflect this preference
- Performance: Lightweight `Update`‑based logic; no allocations per frame

## Files in This Package
- `Runtime/CameraAutoExposureController.cs` – Auto exposure control loop and settings
- `Runtime/UnityEssentials.CameraAutoExposureController.asmdef` – Runtime assembly definition

## Tags
unity, camera, exposure, auto‑exposure, luminance, shutter, iso, photometric, rendering, runtime
