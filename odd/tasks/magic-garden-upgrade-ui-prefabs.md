# Editable upgrade UI prefabs

## Objective
Refactor only Magic Garden upgrade presentation on branch Upgrades. Replace all runtime visual generation in UpgradePanel with serialized prefab-backed UGUI/TextMeshPro Views/Presenter. Integrate existing Canvas Game UI and existing scene EventSystem. No gameplay/core/balance changes, commits, PRs, or new art.

## Baseline
HEAD 9d642f72. Dirty before task: .gitignore, Proyecto-Final/Packages/manifest.json, Proyecto-Final/Packages/packages-lock.json, Proyecto-Final/ProjectSettings/ProjectSettings.asset. Preserve all. Unity 2022.3.62f3; matching local Editor available. Existing native test suite 32 passing (24 core, eight integration). Earlier Play Mode confirmation applies to previous implementation, not this refactor.

## Scope and acceptance
Four separately editable prefabs: level-up panel, level-up card, special-upgrade panel, special-upgrade item. Existing Canvas prefab statically contains panels/presenter. Three horizontal fixed serialized card slots. Shop rows instantiate only item prefab. TMP text and UGUI widgets bound through serialized references; sliced Images ready for Inspector sprites; buttons support normal/highlighted/pressed/selected/disabled via standard transitions. No runtime new Canvas/EventSystem/Button/Image/Text/TMP/GameObject visual hierarchy construction. Presentation/interaction separated from runtime selection/purchase/modal/pause/input logic. Preserve costs refresh, disabled purchases, empty/fewer offer handling, pending levels, shop close/Escape/tutorial closure contracts.

## Route and delivery
Read-only mapping delegated (4+ files). One implementation writer (multiple nontrivial scripts and prefab assets) with narrow surfaces. Unity Editor-only authoring helper may create persistent assets, never runtime generation; bind references using actual AssetDatabase objects. Independent verifier according to read-only assessment. Estimated multi-file UI refactor includes necessary prefab YAML/generated metadata; no size-driven code golf. All changes uncommitted, no delivery actions. Engram unavailable, mirror pending.

## Tasks
- [x] U1 Inspect existing UpgradePanel/Runtime/UIFlow/Crafting compatibility and Canvas/EventSystem/prefab wiring.
- [x] U2 Implement serialized Views/Presenter and prefab-only rendering without core/gameplay changes.
- [x] U3 Create four .prefab assets and matching metadata; statically connect to existing Canvas prefab.
- [x] U4 Add UI asset/binding and interaction regression tests; compile and execute EditMode with persistent evidence.
- [x] U5 Independently review diff/references/scope; report results, limitations and exact art-editable prefab paths.

## Validation
TDD configuration has no explicit enabled source; ordinary functional tests required, prefer tests-first where runnable; never fabricate RED evidence. Native Unity compilation/import and EditMode via Unity 2022.3.62f3 batchmode. Use ignored project Library/MagicGardenUIValidation for persistent logs/results (not Temp). Check Windows process ownership and locks before native calls; never kill user Editor/remove locks. If Editor owns project, report blocker and provide Editor authoring execution entrypoint; do not claim generated/wired prefabs without actual assets and serialized refs. Core tests unchanged. UI tests use reflection to reference Assembly-CSharp while Editor tests remain in existing test asmdef.

## Progress and next step
Implementation, native validation and independent read-only verification complete. Verifier muymnceq-c-ilvz found no source-level blocker; confirmed all four prefab GUID/fileIDs and serialized references, scope preservation, settings baseline SHA, 43/43 native tests and compilation log. Parent also spot-checked persisted test XML, git diff --check and exact settings baseline comparison. UpgradeRuntime binds the existing scene-owned UpgradePanel presenter once instead of creating visuals. Four serialized Views own display and UI events; level-up cards are fixed nested prefab instances, and only shop rows instantiate the authored item prefab (reused on refresh). UIFlowController, CraftingUIManager, core, balance, original tests and Game scene YAML are unchanged. The existing Canvas contains the active presenter, inactive nested panels and a hidden legacy crafting panel; the scene's existing EventSystem is retained.

Art-editable assets (each has a persisted .meta):
- `Proyecto-Final/Assets/Prefabs/UI/Upgrades/LevelUpPanel.prefab`
- `Proyecto-Final/Assets/Prefabs/UI/Upgrades/LevelUpCard.prefab`
- `Proyecto-Final/Assets/Prefabs/UI/Upgrades/SpecialUpgradePanel.prefab`
- `Proyecto-Final/Assets/Prefabs/UI/Upgrades/SpecialUpgradeItem.prefab`

Explicit one-shot `UpgradeUIPrefabAuthoring.Generate` executed successfully in Unity 2022.3.62f3; it has no automatic entrypoint and refuses to overwrite existing UI assets. Authoring used actual AssetDatabase objects, the existing LiberationSans SDF TMP font and PrefabUtility saves, with no new art/sprites. `authoring.log` records these prefab GUIDs/root fileIDs:
- LevelUpPanel: `7a9f90bb2b8fba1449bee9d1d549d770` / `409761589013857790`
- LevelUpCard: `2528d375d8d953a44ba3bf3a40794dc5` / `1148178998566213947`
- SpecialUpgradePanel: `f49f68fd54ae81e45ae22f4f3cdc1521` / `1285734613627579429`
- SpecialUpgradeItem: `2af567fd67bb007459d8a1149ece9655` / `6332052736431292355`

Observed native evidence in ignored `Proyecto-Final/Library/MagicGardenUIValidation/`:
- `compile.log`: successful exit code 0, no C# errors; warnings in existing scripts.
- `authoring.log`: successful exit code 0 and four persistent asset identities.
- `editmode-results.xml`: 43 passed, zero failed/skipped (24 original core, eight original integration, eleven new UI cases).
- UI cases validate metadata/fileIDs, TMP/serialized references, sliced backgrounds, standard button states, three horizontal nested card source references, inherited scene Canvas wiring and existing EventSystem, empty/fewer choices, prefab shop rows, refreshed eligibility/cost labels, idempotent callbacks and missing-reference rejection.
- Licensing-token-unavailable diagnostics occurred without blocking native runs; headless testing also logged a graphics/probe diagnostic. No interactive Play Mode result is claimed.

Before native calls, exact settings content and baseline diffs were captured in `ProjectSettings.baseline.asset`, `ProjectSettings.baseline.diff` and `packages-ignore.baseline.diff`. Settings stayed identical through compilation/authoring, then EditMode added ten native platform fields. Work stopped for human authorization; after explicit approval, only those ten additions were removed using targeted edits, not whole-file restoration. Final settings match the pre-native snapshot byte-for-byte (SHA-256 `561bdb51462a88b913f961094eb3b1d6ed28acd73c223ae9e85379b2a25ae025`). Pre-existing .gitignore/Packages changes remain untouched. Six generated Canvas trailing-whitespace additions were removed without changing values, GUIDs or fileIDs; `git diff --check` then passed. Unity was not rerun for these settings/whitespace-only corrections.

Remaining manual validation: Play Mode layout/readability, focus/navigation, Escape/modal/tutorial closure, queued choices, timeScale and input suspension/resumption. These were not exercised for this UI refactor; prior feature Play Mode approval is not reused as evidence. Native authoring helper refuses overwrites to protect future art editing; a partial generation failure requires deliberate manual cleanup before retry. No commits, staging, branch or dependency changes. Deliver the four prefab paths above as the Inspector art-editing entrypoints. Engram mirror remains pending because the tool is unavailable.
