# Agent Instructions

## Workflow Scope
- Scan only text files needed for the current task.
- Prefer `Assets/`, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/`.
- Allowed extensions: `*.cs`, `*.asmdef`, `*.asmref`, `*.csproj`, `*.shader`, `*.cginc`, `*.hlsl`, `*.json`, `*.yml`, `*.yaml`, `*.xml`, `*.md`, `*.txt`.

## Scan Constraints
- Do not scan Unity/build/generated folders: `Library/`, `Temp/`, `Obj/`, `Logs/`, `Builds/`, `.vs/`, `.idea/`, `.git/`, `UserSettings/`, `MemoryCaptures/`, `Recordings/`, `IL2CPPBuildCache/`, `Bee/`, `Beerifacts/`, `bld/`.
- Do not open heavy/binary files (`*.dll`, `*.exe`, images, audio/video, archives, etc.).
- Do not scan files larger than 1 MB unless explicitly asked.
- Do not run unfiltered full-repo recursive dumps.

## Default Scan Strategy
1. Read `Packages/manifest.json` and `ProjectSettings/ProjectVersion.txt`.
2. Scan `Assets/` only where needed.
3. Expand scope only when required.

## Architecture Contract
- Follow existing project architecture first. Do not invent parallel patterns when equivalent core mechanics already exist.
- Composition root is `Assets/_Main/Scripts/Core/GameRoot.cs`: service registration and controller wiring must be consistent with it.
- Controllers:
  - Orchestrate flow and subscriptions.
  - Implement lifecycle interfaces (`IActivatable`, `IPreloadable`, etc.) where needed.
  - Subscribe in `Activate`, unsubscribe in `Deactivate` (strict symmetry).
  - Avoid storing business state that belongs to models.
- Models:
  - Source of gameplay state and domain events.
  - Prefer controller communication through model state/events.
  - Must not depend on controllers/views.
- Views:
  - Render state and forward user input.
  - Avoid business decisions in view classes.
- Services:
  - Global reusable systems (resource loading, UI, factory, config, localization, audio, notifications, etc.).
  - Use registered services/interfaces; do not duplicate service behavior in controllers.
- Utils:
  - Static, simple, stateless helper logic only.
  - No DI, no lifecycle, no orchestration side effects.

## Dependency Rules
- Prefer constructor injection and explicit dependency passing.
- Do not introduce new direct controller-to-controller dependencies without clear need.
- Default: coordinate through shared model/events; direct controller link is allowed only for tight local orchestration when no model event is suitable.
- Do not use static `Locator` in new code if dependency can be injected/passed via existing constructor chain.
- If a parent already has `ServiceLocator`, pass concrete dependencies down instead of resolving repeatedly in children.

## Resources And Constants
- Use `IResourceService` + `ResourcePaths` for resource loading in gameplay/UI flow.
- Do not hardcode resource path strings where `ResourcePaths` entry exists.
- `Assets/PlatformCore/Services/Factory/ResourcePaths.cs` is generated; do not edit manually.
- Localization keys/constants should be centralized in `GlobalConstants` when used across classes.

## UI Rules
- Use `UIBaseElement` + `BaseContextController<T>` + `IUIService` for UI flows.
- Each UI element must have prefab `Assets/Resources/UI/<TypeName>.prefab` with matching component.
- UI controllers should preload via `OnPreloadAsync`/`IUIService.PreloadAsync<T>()`, then show/hide through context.
- Do not create runtime canvases for regular UI flows.
- Do not use runtime reference auto-resolution in views (`Ensure*`/`Resolve*` patterns for hierarchy lookup, `transform.Find`, `GetComponentInChildren` as fallback, runtime auto-creation of missing UI nodes). Required references must be on the same object or assigned explicitly via `[SerializeField]` in prefab/inspector.
- For style selectors in inspector, use shared serialized types (`TextStyleReference`, `ColorStyleReference`) with common `PropertyDrawer`; do not add per-class custom editors only to render style dropdowns.

## Code Rules
- No duplicate side-effect calls across layers.
- No duplicate guards: keep validation in one responsible place.
- Null checks in Unity style: `if (!obj)` instead of `obj == null`.
- Always use braces for `if` blocks.
- Follow existing naming/style conventions of surrounding code.
- Do not add fallback chains.
- Do not add extra fail-fast guards/exceptions by default. Prefer minimal code and Unity-native failure behavior unless explicit validation/exception handling is requested.

## Collaboration Rules
- For non-trivial changes, ask for confirmation before implementation.
- Prefer the smallest viable change.
- Follow KISS and YAGNI: avoid extra abstraction, fallback systems, and speculative configurability unless explicitly requested.
- Prefer root-cause fixes over symptom patches: identify and validate why the bug happens before applying a fix.
- Avoid workaround logic that masks ordering/state issues if the real source can be corrected safely.
- If multiple approaches exist, propose one minimal recommended path first.
