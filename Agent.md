# Agent Instructions

## Workflow Scope
- Scan only text files needed to understand code and project settings.
- Prefer these locations: `Assets/`, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/`.
- Allowed extensions: `*.cs`, `*.asmdef`, `*.asmref`, `*.csproj`, `*.shader`, `*.cginc`, `*.hlsl`, `*.json`, `*.yml`, `*.yaml`, `*.xml`, `*.md`, `*.txt`.

## What To Avoid Scanning
- Do not scan Unity/build cache or generated folders: `Library/`, `Temp/`, `Obj/`, `Logs/`, `Builds/`, `.vs/`, `.idea/`, `.git/`, `UserSettings/`, `MemoryCaptures/`, `Recordings/`, `IL2CPPBuildCache/`, `Bee/`, `Beerifacts/`, `bld/`.
- Do not open binary/heavy files. Ignore by extension: `*.dll`, `*.exe`, `*.pdb`, `*.so`, `*.dylib`, `*.apk`, `*.aab`, `*.ipa`, `*.unitypackage`, `*.zip`, `*.7z`, `*.rar`, `*.psd`, `*.ai`, `*.blend`, `*.fbx`, `*.obj`, `*.dae`, `*.wav`, `*.mp3`, `*.ogg`, `*.flac`, `*.mp4`, `*.mov`, `*.avi`, `*.mkv`, `*.png`, `*.jpg`, `*.jpeg`, `*.tga`, `*.tiff`, `*.webp`, `*.exr`, `*.pdf`.
- Do not scan files larger than 1 MB unless explicitly asked.
- Do not run recursive listings over the entire repo without filters. Avoid commands that dump thousands of lines.

## Default Scan Strategy
1. Read `Packages/manifest.json` and `ProjectSettings/ProjectVersion.txt` first.
2. Then scan `Assets/` only as needed using the allowed extensions.
3. Anything else only when required and only in text form.

## Code Rules
- No duplicate calls: if method `A` already calls `B`, do not call `B` again outside `A` (especially if `B` has side effects).
- No duplicate guards: if a validation exists inside the callee, remove the outer guard and keep the check in one place.
- Null checks use Unity style: `if (!obj)` instead of `obj == null`.
- Follow project code style.
- `if` statements always use braces. No single-line `if (cond) do;`.

## Utils Placement
- When adding static calculation helpers, first look for an existing `Utils` class near the target class.
- If none exists, create a new `Utils` class in the project style and place helpers there.
