# Copilot instructions for the 3DCR/Artifact project

This file gives targeted, discoverable guidance for AI coding agents working in this Unity project.

1) Purpose and quick start
- Project is a Unity scene-based DCR (Dynamic Condition Response) visualiser. Open the project in the Unity Editor (add the `artifact` folder in Unity Hub) and open the sample scene `Assets/Scenes/Office_English_Abstract`.
- The project must be run inside the Unity Editor — many resources are loaded with `Resources.Load` and some JSON is generated at editor-time by `Model.ParseXmlFile`.

2) High-level architecture
- Pattern: simple MVC implemented under `Assets/Scripts/MVC`.
  - `Controller` (Assets/Scripts/MVC/Controller.cs): orchestrates app state, subscribes to `View` events and updates `Model`.
  - `Model` (Assets/Scripts/MVC/Model.cs): parses DCR XML (`Resources/GameData/*.xml`) into runtime structures and stores history (`ModelState`). JSON is created via `ParseXmlFile` and written to `Assets/GameData` in-editor (`#if UNITY_EDITOR`).
  - `View` (Assets/Scripts/MVC/View.cs): creates `ViewActivity` gameobjects and exposes subscription methods like `SubscribeToActivityExecuted`.
- Effects and visuals live under `Assets/Scripts/Effects` and are controlled centrally by `EffectsController`.

3) Key data flows & integration points
- Game data (DCR graphs) are stored as XML in `Assets/GameData` (Resources). `Model.ParseXmlFile` uses `Resources.Load<TextAsset>("GameData/<name>")` -> converts to JSON and calls `ProcessJsonFile`.
- Scene hierarchy names are important. Code depends on exact child names via `transform.Find(...)` and `FileStrings` constants (see `Assets/Resources/GameData/FileStrings.cs`). Do not rename hierarchy nodes unless you update `FileStrings` and any `transform.Find()` calls.
- Activity creation: `Controller.Run` -> `Model.ProcessJsonFile` -> `View.CreateActivities`. `View.CreateActivities` looks up objects under `ViewActivityContainer` by the human-readable label from `ActivityLabels` (so labels correspond to child GameObject names).

4) Project-specific conventions and patterns
- Centralized resource/file keys: `FileStrings` holds almost all resource names/paths (skyboxes, lights, glitter objects, etc.). Use it when adding or finding objects.
- Use of `Resources.Load` for assets (Materials, Lights, TextAssets). Paths follow `FileStrings` constants.
- Events: View exposes `SubscribeTo...` methods (not raw C# events) — prefer using those methods when hooking behaviour.
- Scene-driven structure: many controllers expect specific children on the same GameObject (e.g. `Controller` expects a child `Model` and a `View` component in children). Inspect `Assets/Scenes/*` to confirm the layout before changing scripts.

5) Dependencies and build notes
- Newtonsoft JSON is used via Unity package `com.unity.nuget.newtonsoft-json` (check `Packages/manifest.json`). TextMeshPro and Unity Visual Scripting are also listed.
- No CLI build script — use the Unity Editor to run and build. Typical steps:
  - Open Unity Hub -> Add the `artifact` folder -> Open the project.
  - Open `Assets/Scenes/Office_English_Abstract` and press Play in the Editor.

6) Files & locations to inspect for feature changes
- MVC core: `Assets/Scripts/MVC/{Controller.cs,Model.cs,View.cs,ViewActivity.cs}`
- Scene / resource names: `Assets/Resources/GameData/FileStrings.cs`
- Effects: `Assets/Scripts/Effects/EffectsController.cs`, `PulsatingLight.cs`, `ParticleColorCycler.cs`
- Game data: `Assets/GameData/*.xml` and `Assets/Resources/GameData/*.json` (JSON created by `Model.ParseXmlFile` while running in the editor)

7) Quick examples (concrete patterns)
- To find the GameObject used for an activity label "MyActivityLabel":
  - Open the scene and look under `View/ViewActivityContainer/MyActivityLabel` — `View.CreateActivities` expects that child to exist.
- To change skybox/light selection logic: modify `View.UpdateGlobalEnvironment` and `FileStrings` constants (View uses `Resources.Load<Material>` and `Resources.Load<Light>` with `FileStrings` paths).

8) What to avoid / gotchas
- Avoid renaming GameObjects or Resources without updating `FileStrings` and corresponding `transform.Find(...)` calls. Many lookups are string-based.
- `Model.ParseXmlFile` writes JSON into `Assets/GameData` only when running in the Editor (`#if UNITY_EDITOR`). Don't expect JSON files to be created at runtime in player builds.

9) When you need more context
- If a change touches scene hierarchy or resource names, open the scene in the Editor and verify the required children exist. If you can't open Unity locally, ask the repo maintainer for a screenshot of the `Hierarchy` under the root `Controller` GameObject.

If anything here is unclear or you'd like extra examples (e.g. a short checklist for renaming a resource or adding a new activity), tell me which section to expand and I will iterate.
