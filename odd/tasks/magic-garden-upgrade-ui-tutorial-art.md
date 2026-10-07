# Tutorial art for upgrade UI

## Objective and scope
On Upgrades (HEAD e6c16e6d), style four existing upgrade UI prefabs using the exact Panel Tutorial artwork. TutorialBox from UISprites.png for frames/backgrounds, valid 9-slice; CraftButton for button visuals Simple with preserved 47:9 pixel aspect; m5x7 TMP font/material. Adapt spacing/dimensions/text for coherent readable pixel art. No new art/importer/core/gameplay/balance changes, generator execution, commits or PRs.

## Baseline
Dirty .gitignore, Packages manifest/lock and ProjectSettings.asset before task must be preserved. Worker also found a pre-existing dirty LiberationSans SDF - Fallback.asset; it was preserved. Unity 2022.3.62f3. Parent confirmed live project Editor and user chose save/close and continue. Check ownership safely before native runs. Do not kill processes/remove locks. Prior art-less UI passed 43 EditMode cases; not evidence for new rendered style.

## Reference evidence
UISprites.png GUID 6b876b38e4597704b84b5e76e54d27a1. TutorialBox local fileID 2111154574, 98x35, border 3/4/3/3, PPU16, Point. CraftButton fileID 2122693667, 47x9, zero borders. m5x7.asset GUID 52be7e01f8fbce54186eac8da2da70cc, fileID11400000, shared material7763458193861376953. Verified against actual YAML and native AssetDatabase tests before applying. All four existing prefab GUIDs/metas, original component fileIDs, serialized View/button references, three nested LevelUpCard sources and the SpecialUpgradeItem source reference remain intact.

## Tasks
- [x] A1 Inspect tutorial/reference assets and four prefab relationships; confirm Upgrades and baseline.
- [x] A2 Apply art/font/layout changes to existing four prefabs without generator, preserving interactions and refs.
- [x] A3 Update UI art assertions and validate compilation/EditMode plus actual PlayMode interactions and captured rendered images if supported.
- [x] A4 Independently verify diff/reference/layout evidence; report visual/PlayMode results or exact limitations.

## Route and checks
Mapping and multifile implementation delegated, one writer. TDD not explicitly enabled, ordinary functional tests required, never fabricate RED or screenshots. Tests-only/editor-only validation helper may inspect/modify existing prefab assets and capture rendered UI, but MUST NOT invoke UpgradeUIPrefabAuthoring.Generate or create replacement prefab assets/new art. Prefer exact existing asset edits. No changes to shipped logic. Authoring helper MUST NOT auto-run on import/play or overwrite future art automatically.
Use persistent ignored Library/MagicGardenUIArtValidation logs/XML/screenshots. Check Windows process ownership without exposing raw command lines with credentials; emit only PID/project/name status. Capture settings baseline bytes/hash before native runs. Never erase baseline diffs. Native test runs can add ten default platform settings: parent authorizes surgical removal ONLY fields verifiably added relative to this task snapshot, preserving baseline. Report ambiguity rather than restoring whole file. After the writer identified an additional runInBackground change, the human explicitly approved option 1: remove those ten verified additions and restore ONLY runInBackground to baseline 0. Use graphics-enabled Unity for rendering tests where feasible; headless -nographics cannot establish visual acceptance. A PlayMode test run and parent reading real screenshots can establish automated evidence, but do not claim human/manual Editor validation. Tests that only load prefab without actual runtime flow must be described as isolated UI smoke, not full-game flow.

## Implementation evidence
- Modified the four EXISTING prefabs, not replacement assets. A temporary explicitly selected Editor test loaded/modified them through PrefabUtility and was removed after the single successful authoring pass. No UpgradeUIPrefabAuthoring.Generate invocation, automatic import/play authoring, retained duplicate generator, shipped UI logic edits, scene YAML edits, dependency changes or new art.
- All panel/card/item frames use white-tinted TutorialBox, Sliced, with pixelsPerUnitMultiplier 0.78125 (8 canvas units per sprite pixel at the existing Canvas reference PPU100). Full-screen panel frames replace the former dark solid backgrounds; no dimmer hides the artwork.
- Cards/items keep their original root Button and background Image references. Authored Action visual children provide 235x45 Simple/preserveAspect CraftButton graphics and readable CHOOSE/PURCHASE labels; Button.targetGraphic points to that child, while the frame stays separate. Continue/Close are fixed 282x54, also exact 47:9 aspect. Original five state colors, serialized Button references and delegates remain unchanged.
- Three cards remain horizontal. Re-spaced target/icon/name/stat/bonus/rarity/current-to-result labels leave room above the bottom action. Shop rows are 280 reference units high, separated by 20 units, with cost/prerequisite padding raised after inspecting real captures. Icons and actions do not overlap checked text regions.
- Every relevant TMP label uses m5x7 and its exact shared material. Authored constant 0.2 vertex gradients maintain dark readable text despite unchanged runtime rarity/eligibility color assignments. Font maxima: headings48, ordinary labels36, help/actions32; autosizing minimum24. No font/material/importer changes.
- Existing UpgradeUIIntegrationTests changed only the intended art assertion (Simple/preserveAspect CraftButton plus separate sliced frame); its inactive-parent lookup was corrected. Original core/gameplay integration tests were not edited.

## Native verification of record
Strict TDD was not activated; no RED/GREEN lifecycle evidence is claimed. All native commands ran synchronously, graphics enabled, on Unity 2022.3.62f3. Sanitized ownership checks found no Editor owning this project; an unrelated Editor was left untouched.

Final compilation command (exit0; compile.log confirms return code0):
```text
"C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" -batchmode -projectPath "C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final" -quit -logFile "C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final/Library/MagicGardenUIArtValidation/compile.log"
```
Final full test command (exit0; 50 passed, 0 failed/skipped):
```text
"C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" -batchmode -projectPath "C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final" -runTests -testPlatform EditMode -testResults "C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final/Library/MagicGardenUIArtValidation/editmode-final-results.xml" -logFile "C:/Users/Dani/OneDrive/Documentos/GitHub/Proyecto-Final/Proyecto-Final/Library/MagicGardenUIArtValidation/editmode-final.log"
```
Counts: 24 UpgradeCoreTests + 8 UpgradeIntegrationTests + 11 UpgradeUIIntegrationTests (the existing43), 5 UpgradeUIArtTests, 2 UpgradeUIArtPlayModeChecks. The runner platform is EditMode, but BOTH new UnityTest cases explicitly EnterPlayMode/ExitPlayMode; these are actual native Play Mode executions, not asset-only substitutes. Direct3D11 rendered captures were produced on the native graphics device.

### Real runtime and isolated fixture scope
- GameSceneRuntimeChoiceShopPurchaseAndModalRelease loads the actual Assets/Scenes/Game.unity runtime and existing Canvas presenter. Test-only fixture skips tutorial, sets Day, clears any existing modal, resets progression, sets timeScale1 and grants XP150. No shipped gates or progression rules are modified.
- ExecuteEvents pointer enter/down/up/click dispatch to the actual authored card verifies pending-choice consumption, timeScale0 while choosing then1 on release, LevelUp modal ownership then None, InputReader.MoveInput zero, rejection of Inventory while choosing, and disabled/restored legacy raw-input consumers.
- The fixture opens the real shop, removes resources for the first production definition, clicks its disabled purchase and verifies unchanged stacks, grants exactly its aggregated production costs, clicks Purchase and verifies stack increment plus zero spent-resource balances. It clicks Close and verifies the shop hidden, Crafting modal released and timeScale1. Resources/tutorial control exist only in tests; this is Game-scene upgrade-flow coverage, not complete campaign gameplay or physical keyboard/mouse validation.
- IsolatedPrefabPointerSmokeAndRenderedLayouts is separately identified as isolated UI smoke: three populated cards, explicit empty-pool Continue, two shop rows including disabled eligibility, repeated Show calls without duplicate callbacks, Purchase/Close delegates. It checks all five Button transition states and CanvasRenderer state tints. Isolated fixture icons use an existing sheet sprite; runtime screenshots use actual owned-target icons.
- Capture helpers temporarily route the in-memory Canvas to a UI-layer camera/RenderTexture and disable CanvasScaler only during capture, applying the existing 1920x1080 / matchWidthOrHeight0.5 scaling formula. They restore those in-memory settings and never save the scene. TMP overflow/font-size and icon/action collision assertions pass at each capture dimension. These are rendered automated checks, not manual human acceptance.

### Screenshot and text-fit artifacts
All paths below are relative to `Proyecto-Final/Library/MagicGardenUIArtValidation/`:
- runtime-levelup-1920x1080.png
- runtime-levelup-1280x720.png
- runtime-levelup-1280x1024.png
- runtime-shop-1920x1080.png
- runtime-shop-1280x720.png
- runtime-shop-1280x1024.png
- isolated-levelup-1920x1080.png
- isolated-levelup-1280x720.png
- isolated-levelup-1280x1024.png
- isolated-shop-1920x1080.png
- isolated-shop-1280x720.png
- isolated-shop-1280x1024.png
- isolated-empty-1920x1080.png
Thirteen real PNGs: reference1920x1080, smaller16:9 1280x720, narrower5:4 1280x1024. Each has a matching `-text-fit.txt` report. Images were read by the writer; parent inspected runtime level-up at1920x1080 and shop at1920x1080/1280x1024. Independent verifier inspected runtime level-up1280x720 and shop at all three resolutions and found no visual blocker. Passing asset tests alone is not visual acceptance.

### Earlier failures and warnings
- authoring-results.xml: first one-time authoring test failed before edits because TMP font material is a field, not a reflected property. Fixed field lookup; authoring-retry-results.xml passed1/1, then the authoring method was removed.
- isolated-results.xml: EnterPlayMode domain reload invalidated an iterator's captured closure. Moved smoke body into a separately created coroutine after entering Play Mode.
- isolated-retry-results.xml / isolated-render-results.xml: uniform-color RenderTexture capture failed the nonblank-content assertion. Allowed normal graphics frames to render Canvas meshes and placed the isolated root Canvas on the camera-visible UI layer. isolated-layer-results.xml passed1/1 with real captures; no blank image was claimed as visual evidence.
- art-results.xml passed5/5; runtime-results.xml passed1/1. editmode-results.xml initially passed49/50 because the updated art assertion's parent lookup excluded inactive prefab parents; fixed includeInactive=true. editmode-final-results.xml passes50/50, including all fixes and final shop padding/state checks.
- Native Game-scene output contains warnings about Skeleton Animator transitions/missing states, Bird Sprite SoundAmbientRandomizedTrigger required components and DontDestroyOnLoad on non-root objects. They were reported, not suppressed or edited. Native startup also reports licensing token/signature messages while successfully resolving entitlement and exiting0. No failing test remains in the final required run.

## Baseline preservation and approved cleanup
Before cleanup, Compare-Object against the exact task snapshot showed only these ten native additions: androidAutoRotationBehavior, androidPredictiveBackSupport, audioSpatialExperience, visionOSBundleVersion, tvOSBundleVersion, iOSSimulatorArchitecture, tvOSSimulatorArchitecture, metalCompileShaderBinary, switchDisableHTCSPlayerConnection, syncCapabilities; plus runInBackground0->1. Following explicit human approval, precise YAML edits removed only those ten lines and restored only runInBackground0. No whole-file restore was used.
After cleanup, baseline and current settings SHA256 both equal `561BDB51462A88B913F961094EB3B1D6ED28ACD73C223AE9E85379B2A25AE025`, and line comparison has no differences. Task-baseline ProjectSettings changes remain intact. No native rerun follows this settings-only cleanup, as authorized, because it would repeat the same side effects.
Preserved font SHA256 values:
- Pre-existing dirty LiberationSans SDF - Fallback.asset: `AAF76286BB419CEA8DF94133B7A534186E55F8ECF5AF6FB1C3B1F314DF5F3A4F`.
- m5x7.asset: `77F62AD60029ACA7636719B1B3CB97B9E9E84F1DD3C42BE23BEC100A99CEDE5D`.
.gitignore and Packages dirty changes were left untouched. Prefab metas/importers/sprites/fonts/gameplay/core/balance/generator/scene YAML were not edited by the writer; no commit or terminal Git mutation.

## Final follow-up checks
- `git diff --check`: exit2, solely trailing whitespace in the preserved pre-existing dirty LiberationSans SDF - Fallback.asset at lines22,361,378. Its baseline hash is unchanged; no out-of-scope whitespace edits were made.
- `git diff --check -- Proyecto-Final/Assets/Prefabs/UI/Upgrades Proyecto-Final/Assets/Tests/Editor/Upgrades Proyecto-Final/ProjectSettings/ProjectSettings.asset odd/tasks/magic-garden-upgrade-ui-tutorial-art.md`: exit0; no task-surface whitespace errors. Git emits LF-to-CRLF advisory warnings, not new content changes.
- `git diff --stat`, `git diff -- Proyecto-Final/ProjectSettings/ProjectSettings.asset`, `git status --short`: inspected; settings have no displayed content hunks after surgical cleanup, and unrelated baseline dirty/untracked files remain. New untracked tests/metas and this feature document are reported by status rather than ordinary tracked diff statistics.

## Progress and remaining disposition
A2/A3 implementation and automated verification complete. Parent spot-checked final XML50/50, scoped whitespace and exact settings snapshot comparison. Independent verifier muynx8pt-g-4d4u confirmed no art/layout/reference/interaction blocker, scope preservation, exact asset references, rendered readability and real automated Play Mode coverage. A4 complete. Global whitespace failure is only the pre-existing unchanged fallback font. Physical keyboard/mouse, shop-scroll input, manual in-Editor acceptance and full campaign gameplay remain unverified; lower shop rows are intentionally masked by the vertical ScrollRect. No manual human validation is claimed. No commits/PRs/generator execution. Engram mirror remains pending (tool unavailable).
