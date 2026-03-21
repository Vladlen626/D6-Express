# Stylized Opaque Shaders

This folder contains two hand-written URP opaque stylized shaders for WebGL / low-end hardware:

- `Game/StylizedOpaqueSimple`: baked GI focused version (lightmaps + light probes), no realtime shadow map sampling.
- `Game/StylizedOpaqueShadowed`: runtime shadowed version with main Directional Light realtime shadow receiving + ShadowCaster pass for casting.

Both keep the same non-PBR stylized look: one base texture, one tint color, soft ramp lighting, shadow tint, and minimal controls.

## Intended Scope

- Static and dynamic props with readable forms and soft stylized shading.
- Painted/pastel look with gentle shadow tint.
- Single-pass forward opaque material in URP.
- Baked lighting workflow: static lightmaps, dynamic light probes, and a Meta pass for bake contribution.
- Optional main directional realtime shadows via `Game/StylizedOpaqueShadowed`.

## Intentionally Not Supported

- Additional lights
- Additional-light realtime shadows
- Realtime shadow maps in `Game/StylizedOpaqueSimple` (use `Game/StylizedOpaqueShadowed` when needed)
- PBR workflows (metallic/smoothness/specular)
- Normal map / emission / rim light / clear coat
- Transparency, cutout, outline, triplanar, parallax
- Extra passes or shader keyword feature toggles

## Why It Is Cheaper Than General Lit

- Lightweight forward shading with one stylized main-light pass
- `Game/StylizedOpaqueShadowed` adds only a minimal ShadowCaster pass for realtime casting
- Main directional light only
- `Game/StylizedOpaqueSimple` uses baked GI only (no realtime shadow sampling)
- No additional light loops
- No reflection probe sampling
- No PBR BRDF calculations
- Minimal interpolators and small fragment math
- Mostly `half` precision math for mobile/WebGL friendliness

## Which Shader To Use

- Use `Game/StylizedOpaqueSimple` when you want the cheapest stylized opaque result and do not need realtime shadow maps.
- Use `Game/StylizedOpaqueShadowed` when objects must cast realtime shadows onto other surfaces and receive the main directional shadow.
- Prefer `Simple` as default for static/background props, and `Shadowed` for gameplay-critical actors/props that need realtime shadow interaction.

## Exposed Properties

- `_BaseMap` (Base Map): single albedo texture, with standard tiling/offset.
- `_BaseColor` (Base Color): global tint multiplied with texture color.
- `_ShadowColor` (Shadow Color): tint used in shadowed side of the soft ramp.
- `_Cull` (Cull): per-material culling mode (`Back` default, `Off` for double-sided rendering).
- `_LightWrap` (Light Wrap): softens transition around terminator for rounded forms.
- `_RampSoftness` (Ramp Softness): controls smoothness width of light/shadow blend.
- `_AmbientStrength` (Ambient Strength): small ambient lift so shadows are not dead.
- `_TextureInfluence` (Texture Influence): blends between flat tint (`0`) and full texture (`1`).
- `_SaturationBoost` (Saturation Boost): optional final saturation control without extra variants.

## Recommended Scene Usage

- Use for broad stylized environment props, clutter, and low-cost characters/objects.
- Keep lighting simple with a clear main directional key light.
- Use `_Cull = Off` for thin/leaf-like meshes that must render both sides.
- Mark static bakeable meshes as lightmapped; keep moving props on Light Probes.
- Prefer consistent material palettes and avoid high-frequency texture detail.
- Duplicate materials and mainly tweak `_BaseMap` and `_BaseColor`; adjust stylized controls only when needed.

## Recommended Base Texture Import Style

- Soft painted textures
- Low detail noise
- Low contrast
- No baked harsh shadows

## Next Possible Upgrades

- cheap rim light
- foliage cutout as a separate shader
- optional quality-tiered soft shadow filtering for `StylizedOpaqueShadowed`
