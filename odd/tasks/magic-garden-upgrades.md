# Magic Garden upgrades

## Objective and scope
Implement the user's complete run stat upgrades and separate cauldron special upgrades on the current Develop-Testing branch only, preserving combat/day-night/mana/plant/inventory behavior except requested changes. Unity 2022.3.62f3. User approved full estimated 1,200–2,200 authored lines without commits or PRs. Infernum is treated as the requested Scorpion reward target (25 XP) following the user's clarification; preserve its name. Skeleton 15, Gnome 20. No new enemies/weapons/plants/waves/loadouts/assets/plugins.

## Baseline and constraints
Existing dirty files: .gitignore; Proyecto-Final/Packages/manifest.json; Proyecto-Final/Packages/packages-lock.json. Preserve these. All gameplay state runtime, never mutate ScriptableObjects. No runtime LINQ or unnecessary singleton. Dedicated level-up flow, not Paused. Weighted unique compatible current-run offers; max stacks; fixed Quantity +1, rare weight; configurable rarity/scaling. Cumulative XP thresholds 0/100/250/450/750/1200/1800/2600/3500/4500; no extrapolated L11+ balance. Atomic aggregated resource spending and composed special effects, no per-upgrade flags. UI horizontal three cards with affected target/icon/name/stat/bonus/rarity/current/result.

## Route and delivery
Exploration delegated (4+ files), implementation delegated to one writer (multiple nontrivial files), independent verification after native read-only assessment. All edits remain uncommitted. Forecast 20–35+ files / 1,200–2,200 lines is advisory, not a size cap. Technical artifacts in English. Engram unavailable: mirror pending. Local feature document is continuity record.

## Tasks
- [x] T1 Verify branch/version/baseline and map current architecture (two read-only exploration handoffs).
- [x] T2 Implement and test runtime stat/scaling/rarity/selection and cumulative XP core.
- [x] T3 Integrate enemy pickups, spells/plants/player stats, queued level-up and dedicated interactive UI (Play Mode validated by user).
- [x] T4 Replace cauldron seed crafting with configurable composed special upgrades and atomic inventory costs (Play Mode validated by user).
- [x] T5 Verify Unity compilation, relevant tests, diagnostics, wiring, and complete diff; fix introduced defects (32 EditMode tests passed; Play Mode validated by user).
- [x] T6 Prepare delivery report with architecture/file inventory/decisions/native 32-test results and explicit interactive Editor limitations.

## Checks and evidence
Unity executable discovered: C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe. Nested project path: C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final. Unity Test Framework 1.1.33 exists, no project tests/asmdefs. Avoid indiscriminate asmdef migration; establish runnable test boundary for pure upgrade core plus integration tests. TDD: no project/session enabled configuration found; ordinary functional checks required, tests-first where runnable, never claim RED without observed failure. Runner: Unity batchmode EditMode -runTests, with results/logs inside project-local ignored output (not workspace root). Use a batch compilation/import check as well; detect existing Editor project lock and never close the user's Editor. Preserve any pre-existing failures in report.

## Acceptance
All requested stat and special flows implemented, wired to current scene/prefabs or deterministic bootstrap that works with their serialized references, no manual hidden configuration required for initial system. Meaningful tests for stacking/scaling/rarity/quantity/selection/filters/XP/multiple levels/effects/atomic spending. No introduced compilation errors. Do not claim play-mode/UI/animation verification without actually executing it.

## Progress
T2 implemented and observed passing: 24 pure-core NUnit cases via the installed Mono alternate runner, zero failures and compiler diagnostics. Eight reflection-based Unity integration tests initially compiled with installed Unity references without execution; the native follow-up below now observes all eight passing alongside the 24 core cases. The initial alternate runner exposed a null-callback XP-loop stall, which was fixed before subsequent passing runs. Two initial syntax-check attempts needed netstandard and JSONSerializeModule references; the corrected compiler command passes.

T3/T4 source implementation is present: cumulative XP/pending choices, once-only pickup drops, per-target player/spell/plant queries, dedicated modal plus horizontal cards, automatic scene bootstrap, data-driven special upgrades and guarded aggregate material transactions. These checkboxes remain open because interactive current-scene/play-mode acceptance has not been observed. Native EditMode integration validation now passes, as recorded below. Default player prefab progression is L1/0 XP; existing unlock lists and 125 XP night reward are preserved. Gameplay documentation and manual acceptance checklist live in `Proyecto-Final/Assets/Scripts/Upgrades/README.md`.

T5 partial for full feature acceptance: native Unity compilation/import now exits 0 with no C# errors, and persistent EditMode XML reports 32 passed, zero failed/inconclusive/skipped (24 core plus eight integration). Initial runs were deferred while Editor PID 2300 owned this project; after the user closed it, the correction worker observed only Hub unity.exe serve and ran both authorized commands. No Editor killed or lock removed. FireSpell CS0114 was pre-existing per parent HEAD verification, not introduced by the upgrades; the explicit override removes it without invoking the base lifetime. Twenty-six other C# warning sites remain; a full HEAD warning baseline was not rerun. No play-mode/UI/animation checks executed. Baseline .gitignore and Packages files were not edited by this correction.

Necessary adaptations documented for parent acceptance: Piercing already has unlimited unique in-range hits, so +1 pierce adds one post-range hit with configurable grace and traversed-hit damage while preserving the baseline. Removing cauldron seed crafting otherwise strands new plant unlocks; configurable one-time initial seed delivery preserves existing counts and defers when inventory is full. Gated/typing/Wait tutorial instructions defer pending choices instead of destroying the existing tutorial UI lifecycle. Raw interaction/boss/lunar input managers are temporarily suspended while a choice owns the modal.

## Next step
Native verifier muykgdf4-5-691s ran after user closed Editor: compilation exit 0, Tundra success, no C# errors; EditMode finished but result XML in Temp disappeared, so totals unverified. Native Unity added ten defaults to previously clean ProjectSettings.asset. Read-only incident diagnosis completed, parent inspected actual diff and git HEAD for lifetime methods. FireSpell hiding warning pre-existed (base already virtual in HEAD), so preserve Fire's independent lifetime by explicit override without base call. Correction worker muykorbm-7-t2g8 is authorized to make that narrow fix, rerun native compile/EditMode with persistent Library/MagicGardenValidation outputs, and surgically reverse ONLY the ten validation-generated settings additions afterward. No full settings restore allowed. Independent alternate core compiler/runner passed 24/24, test syntax compiler zero diagnostics, no confirmed blocking source defect. Native assessment unassessable due untracked files; independent verification required and performed. Parent spot check passed 24/24 and diff --check, branch Develop-Testing. Current full-feature status remains partial pending interactive scene/play-mode acceptance; the bounded correction and native compile/EditMode follow-up below have now passed, without claiming the actual scene/UI has been played. Engram mirror remains pending (tool unavailable).

## Final disposition
Parent read the persisted native test XML: Passed, total 32, failed/inconclusive/skipped 0. Parent checked final FireSpell diff and git diff --check (exit 0); ProjectSettings has no content hunks despite its residual status marker. Automated compilation/tests and independent source/diff review completed. User subsequently confirmed Play Mode was validated and explicitly authorized committing the implementation on the new upgrades branch (actual current Git spelling: Upgrades). Interactive acceptance is user-reported, not an agent-run test. T3/T4/T5 closed on that confirmation. Exact implementation file inventory below is the delivery file list. Commit excludes baseline .gitignore/Packages changes and ProjectSettings status marker. No push or PR authorized.

## Native validation correction (follow-up)

Scoped correction only: `Proyecto-Final/Assets/Scripts/Spells/FireSpell.cs` now declares `protected override void Awake()`, retains the rb cache, and comments why it intentionally does **not** invoke base.Awake. FireRoutine and ProjectileLifetime are unchanged in this correction. Parent verified HEAD Spell.Awake was virtual and HEAD FireSpell.Awake private: CS0114 was pre-existing. Calling base would add the generic lifetime and could destroy a flame/controller early.

Windows process ownership was checked before and after native execution:
```powershell
Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" | Select-Object ProcessId, CommandLine | Format-List
```
Observed only PID 4304, `C:\Program Files\Unity Hub\resources\unity.exe serve`, not a project Editor. No process termination or lock removal occurred. Created project `Library/MagicGardenValidation` as authorized; all new logs/results persist there instead of Temp.

Foreground compile/import command, **exit 0**, native log ends with successful batchmode exit and return code 0:
```bash
'/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe' -batchmode -nographics -projectPath 'C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final' -quit -logFile 'C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final/Library/MagicGardenValidation/compile.log'
```
Observed no `error CS` and no CS0114. The log contains 26 distinct remaining C# warning sites (CS0108/CS0067/CS0414), printed twice by compilation/reporting, none in FireSpell. No newly introduced diagnostic is attributed to the one-method correction; the broader HEAD warning baseline was not independently rerun. Non-blocking native licensing-token-unavailable and shutdown Curl 42 messages also appear; license update and compilation still succeeded.

Foreground EditMode command, **exit 0**, deliberately without -quit:
```bash
'/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe' -batchmode -nographics -projectPath 'C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final' -runTests -testPlatform EditMode -testResults 'C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final/Library/MagicGardenValidation/editmode-results.xml' -logFile 'C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final/Library/MagicGardenValidation/editmode.log'
```
Read the complete persisted XML: test-run `result="Passed"`, `testcasecount="32"`, `total="32"`, `passed="32"`, `failed="0"`, `inconclusive="0"`, `skipped="0"`; UpgradeCoreTests 24/24 and UpgradeIntegrationTests 8/8. Result artifact is `Proyecto-Final/Library/MagicGardenValidation/editmode-results.xml`; accompanying native logs are `compile.log` and `editmode.log` in the same directory. The test log confirms saving this exact XML. Headless ambient-probe, Mono shutdown-thread and debugger-agent listen diagnostics occur, but no test failure or C# error is observed. Native tests do not establish interactive game/UI behavior.

ProjectSettings cleanup was authorized only for validation-generated additions. The diff was inspected before editing and again after both native runs; it consisted exclusively of the same ten added defaults:
```text
androidAutoRotationBehavior: 1
androidPredictiveBackSupport: 1
audioSpatialExperience: 0
visionOSBundleVersion: 1.0
tvOSBundleVersion: 1.0
iOSSimulatorArchitecture: 0
tvOSSimulatorArchitecture: 0
metalCompileShaderBinary: 0
switchDisableHTCSPlayerConnection: 0
syncCapabilities: 0
```
Removed only those lines using targeted edits, preserving intervening settings and all other files. Post-cleanup `git diff -- Proyecto-Final/ProjectSettings/ProjectSettings.asset`, `--numstat` and `--raw` produce **no content/mode hunks**. `git status --short` still marks that file M with LF-to-CRLF advisories; `git ls-files --eol` reports `i/lf w/lf attr/text=auto`. This residual status/normalization marker is reported rather than hidden or addressed through unauthorized full-file normalization/index changes. No additional settings edits were attempted. `git diff --check` exits 0; baseline .gitignore/Packages dirt and all existing implementation changes remain. No commits, staging, installations or branch changes.

TDD remains disabled: RED not active; GREEN not active; the native observed validation is ordinary functional verification. Bounded correction/native validation status is completed; the parent still owns full feature disposition and manual play-mode acceptance. Existing task IDs/checklist are preserved.

## Writer verification commands and observed evidence (initial historical run)

- `git status --short`: Develop-Testing baseline retained; task edits/new upgrade/test files visible, no staging.
- `git diff --check`: exit 0; no whitespace errors, CRLF advisories for existing baseline files and rewritten CraftingUIManager.
- `git diff --stat`: inspected; includes three pre-existing dirty files and excludes untracked new sources. Exact final output is in writer handoff.
- `git diff`: inspected in full plus scoped portions after output truncation; no dependency/branch/git-terminal actions performed.
- Read-only process discovery: PID 2300 matches Proyecto-Final; PID 4304 does not. Temp/UnityLockfile exists.

Alternate core compiler (exit 0, zero diagnostics):
```bash
'/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/MonoBleedingEdge/bin/mono.exe' 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/MonoBleedingEdge/lib/mono/4.5/mcs.exe' -langversion:latest -define:MAGIC_GARDEN_STANDALONE -out:Proyecto-Final/Temp/magic-garden-core-checks.exe -r:Proyecto-Final/Library/PackageCache/com.unity.ext.nunit@1.0.6/net35/unity-custom/nunit.framework.dll Proyecto-Final/Assets/Scripts/Upgrades/Core/UpgradeCore.cs Proyecto-Final/Assets/Tests/Editor/Upgrades/UpgradeCoreTests.cs Proyecto-Final/Assets/Tests/Editor/Upgrades/StandaloneCoreChecks.cs
```
Alternate core runner (exit 0; observed log **24 passed, zero failed**, NOT Unity proof):
```bash
MONO_PATH='C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final/Library/PackageCache/com.unity.ext.nunit@1.0.6/net35/unity-custom' '/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/MonoBleedingEdge/bin/mono.exe' Proyecto-Final/Temp/magic-garden-core-checks.exe > Proyecto-Final/Temp/magic-garden-core-checks.log
```
Test-harness syntax compiler (exit 0, zero diagnostics; eight integration cases compile but did not execute):
```bash
'/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/MonoBleedingEdge/bin/mono.exe' 'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/MonoBleedingEdge/lib/mono/4.5/mcs.exe' -langversion:latest -target:library -out:Proyecto-Final/Temp/magic-garden-test-syntax.dll -r:Proyecto-Final/Library/PackageCache/com.unity.ext.nunit@1.0.6/net35/unity-custom/nunit.framework.dll -r:'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/MonoBleedingEdge/lib/mono/4.5/Facades/netstandard.dll' -r:'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll' -r:'C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Data/Managed/UnityEngine/UnityEngine.JSONSerializeModule.dll' Proyecto-Final/Assets/Scripts/Upgrades/Core/UpgradeCore.cs Proyecto-Final/Assets/Tests/Editor/Upgrades/UpgradeCoreTests.cs Proyecto-Final/Assets/Tests/Editor/Upgrades/UpgradeIntegrationTests.cs
```
Required Unity EditMode command: **not run**, existing user Editor/project lock; zero Unity tests observed. Expected suite contains 32 NUnit cases when discovered:
```bash
'/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe' -batchmode -nographics -projectPath 'C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final' -runTests -testPlatform EditMode -testResults 'C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final/Temp/magic-garden-editmode-results.xml' -logFile 'C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final/Temp/magic-garden-editmode.log'
```
Required Unity import/compile command: **not run**, existing user Editor/project lock; Unity diagnostics unknown:
```bash
'/c/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe' -batchmode -nographics -projectPath 'C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final' -quit -logFile 'C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final/Temp/magic-garden-compile.log'
```
TDD evidence: RED not active — strict TDD was not activated; GREEN not active — ordinary validation reported above. No pre-implementation RED claim.

## Exact implementation file inventory

Modified tracked task files (baseline .gitignore/Packages changes excluded):
```text
Proyecto-Final/Assets/Scriptables/Enemies/GardenGnome.asset
Proyecto-Final/Assets/Scriptables/Enemies/Infernum.asset
Proyecto-Final/Assets/Scriptables/Enemies/Skeleton.asset
Proyecto-Final/Assets/Scriptables/Progression/Experience Progression.asset
Proyecto-Final/Assets/Scripts/CraftingCauldron.cs
Proyecto-Final/Assets/Scripts/Enemies/Data/EnemyDataSO.cs
Proyecto-Final/Assets/Scripts/Enemies/EnemyBase.cs
Proyecto-Final/Assets/Scripts/GameInput/InputReader.cs
Proyecto-Final/Assets/Scripts/Gameplay Systems/Crafting System/CraftingSystem.cs
Proyecto-Final/Assets/Scripts/Gameplay Systems/Crafting System/CraftingUIManager.cs
Proyecto-Final/Assets/Scripts/Gameplay Systems/GameFlowController.cs
Proyecto-Final/Assets/Scripts/Gameplay Systems/InventoryManager.cs
Proyecto-Final/Assets/Scripts/Gameplay Systems/LevelManager.cs
Proyecto-Final/Assets/Scripts/Gameplay Systems/LifeController.cs
Proyecto-Final/Assets/Scripts/Gameplay Systems/PauseController.cs
Proyecto-Final/Assets/Scripts/Gameplay Systems/PlantDataSO.cs
Proyecto-Final/Assets/Scripts/Gameplay Systems/SeedInventory.cs
Proyecto-Final/Assets/Scripts/Plants/AttackPlant.cs
Proyecto-Final/Assets/Scripts/Plants/DefensePlant.cs
Proyecto-Final/Assets/Scripts/Plants/ElectricPlantAura.cs
Proyecto-Final/Assets/Scripts/Plants/IceRangeSeed.cs
Proyecto-Final/Assets/Scripts/Plants/IceSlowArea.cs
Proyecto-Final/Assets/Scripts/Plants/Plant.cs
Proyecto-Final/Assets/Scripts/Plants/ResourcePlant.cs
Proyecto-Final/Assets/Scripts/Plants/SierraPlant.cs
Proyecto-Final/Assets/Scripts/Plants/StormRoseReactiveAura.cs
Proyecto-Final/Assets/Scripts/Player/ExperienceProgressionDataSO.cs
Proyecto-Final/Assets/Scripts/Player/PlayerAbilitySystem.cs
Proyecto-Final/Assets/Scripts/Player/PlayerContentUnlockSystem.cs
Proyecto-Final/Assets/Scripts/Player/PlayerController.cs
Proyecto-Final/Assets/Scripts/Player/PlayerExperienceSystem.cs
Proyecto-Final/Assets/Scripts/Player/PlayerMovementController.cs
Proyecto-Final/Assets/Scripts/Player/PlayerSpellController.cs
Proyecto-Final/Assets/Scripts/Player/PlayerTeleportController.cs
Proyecto-Final/Assets/Scripts/Spells/BasicAreaSpell.cs
Proyecto-Final/Assets/Scripts/Spells/BasicMeleeSpell.cs
Proyecto-Final/Assets/Scripts/Spells/BasicRangeSpell.cs
Proyecto-Final/Assets/Scripts/Spells/Data/SpellDataSO.cs
Proyecto-Final/Assets/Scripts/Spells/FireSpell.cs
Proyecto-Final/Assets/Scripts/Spells/HammerSpell.cs
Proyecto-Final/Assets/Scripts/Spells/PiercingSpell.cs
Proyecto-Final/Assets/Scripts/Spells/Spell.cs
Proyecto-Final/Assets/Scripts/Spells/TeleportSpell.cs
Proyecto-Final/Assets/Scripts/UI/ExperienceUIController.cs
Proyecto-Final/Assets/Scripts/UI/SpellInventory.cs
Proyecto-Final/Assets/Scripts/UI/UIFlowController.cs
```
New task sources/config/docs (each also has its adjacent Unity .meta, generated by the already-running Editor):
```text
Proyecto-Final/Assets/Scripts/Upgrades/Core/MagicGarden.Core.asmdef
Proyecto-Final/Assets/Scripts/Upgrades/Core/UpgradeCore.cs
Proyecto-Final/Assets/Scripts/Upgrades/ExperiencePickup.cs
Proyecto-Final/Assets/Scripts/Upgrades/UpgradeCasting.cs
Proyecto-Final/Assets/Scripts/Upgrades/UpgradePanel.cs
Proyecto-Final/Assets/Scripts/Upgrades/UpgradeRuntime.cs
Proyecto-Final/Assets/Scripts/Upgrades/README.md
Proyecto-Final/Assets/Scriptables/Upgrades/Resources/MagicGardenBalance.json
Proyecto-Final/Assets/Tests/Editor/Upgrades/MagicGarden.Tests.asmdef
Proyecto-Final/Assets/Tests/Editor/Upgrades/StandaloneCoreChecks.cs
Proyecto-Final/Assets/Tests/Editor/Upgrades/UpgradeCoreTests.cs
Proyecto-Final/Assets/Tests/Editor/Upgrades/UpgradeIntegrationTests.cs
```
New directory metadata, all within allowed surfaces:
```text
Proyecto-Final/Assets/Scripts/Upgrades.meta
Proyecto-Final/Assets/Scripts/Upgrades/Core.meta
Proyecto-Final/Assets/Scriptables/Upgrades.meta
Proyecto-Final/Assets/Scriptables/Upgrades/Resources.meta
Proyecto-Final/Assets/Tests.meta
Proyecto-Final/Assets/Tests/Editor.meta
Proyecto-Final/Assets/Tests/Editor/Upgrades.meta
```
The already-untracked `odd/tasks/magic-garden-upgrades.md` was updated in place, preserving T1 and all parent task IDs. No scenes/prefabs or other files were authored. Initial ignored verification outputs: project Temp/magic-garden-core-checks.exe, magic-garden-core-checks.log and magic-garden-test-syntax.dll. Native correction follow-up outputs: Proyecto-Final/Library/MagicGardenValidation/compile.log, editmode.log and editmode-results.xml. ProjectSettings cleanup has no normalized content/mode diff, with the residual status marker detailed above.
