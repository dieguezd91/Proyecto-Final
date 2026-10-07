# Compact centered upgrade modals

## Objective
On Upgrades, adjust only four existing upgrade UI prefabs so windows look compact/centered, not full-screen artwork. Reference1920x1080: LevelUp window~1100x680, cards~300x420, CraftButton~165x32 exact47:9; shop window~1150x750, rows~950x170. Dimensions flexible for readability. Maintain TutorialBox Sliced, CraftButton Simple/aspect, m5x7, three horizontal cards, existing View/button/runtime references and functionality. No global localScale shrink, core/gameplay/balance/generator/art/importer/Canvas/scene changes, commits or PRs.

## Baseline and approach
Prior tutorial art modifications/tests are uncommitted and must be preserved/refined, not restored to HEAD. Dirty .gitignore, Packages, Settings and fallback font are unrelated baseline. Snapshot exact current settings/font hashes before native runs. Unity2022.3.62f3. Parent checked live project Editor PID14576 and import workers, user chose save/close then continue. Recheck ownership safely; never kill processes/remove locks.
Canvas Game UI overrides both panel roots full-stretch. Keep roots for full-screen background/modal raycast blocker; introduce authored centered TutorialBox Window inside each root and move/reanchor existing visual content beneath it. Preserve View serialized fields/component identities; no changes needed to Canvas overrides. Card source and LevelUpPanel nested root anchor/size overrides both need adjustment. Shop existing ScrollRect/Content/View refs retained; LayoutElement rows compacted. New authored RectTransform/Image children only in prefab assets, never runtime visual generation.

## Tasks
- [x] C1 Map current transforms/layout/nested overrides and process ownership.
- [x] C2 Compact windows, cards/buttons/rows and text/padding directly in four prefab assets.
- [x] C3 Update layout assertions and validate compilation/tests/rendered PlayMode at1920x1080 and1280x720 (retain5:4 regression where useful).
- [x] C4 Independently inspect screenshots/references/scope and deliver final results/limitations.

## Checks and route
Mapping and multi-file implementation delegated, one writer. Explicit TDD not enabled; ordinary tests required, no fabricated RED. Existing50 cases include real EnterPlayMode Game fixture and isolated render fixture; preserve test semantics. Add bounds/center/size/fit assertions, adapt existing full-screen frame expectations. Use fresh ignored Library/MagicGardenCompactUIValidation logs/XML/screenshots; do not overwrite prior evidence. Reuse graphics-enabled2022.3.62f3 commands -runTests -testPlatform EditMode (EnterPlayMode fixtures); no -quit asynchronous runner. Native compile -quit. Generator MUST NOT run. Automated captures read by parent provide visual evidence; do not claim physical input/manual Editor validation.
Native side effects cleanup authorized: only ten default platform additions verifiably absent new task baseline and runInBackground changed by Test Runner (restore exact task baseline value). Surgical edits, no whole-file restore. Any other mutation requires stop/report. Preserve all baseline dirty data; global whitespace may already fail fallback font, scoped task checks required.

## Observed implementation and verification
C1: sanitized process recheck found only unrelated Unity PID4304 (target=false, import-worker=false); no target Editor ownership remained. No processes killed or locks removed.

C2: direct YAML edits only. Both existing root Images/View bindings stay on full-screen raycast-blocking dimmers (black alpha0.65, no sprite). Each prefab adds exactly one authored Window (GameObject/RectTransform/CanvasRenderer/Image); existing visual children reparented without changing their IDs. Window TutorialBox remains white/Sliced. LevelUp source and nested override sizes are300x420, with20px gaps. Shop LayoutElement rows are950x200, spacing14, viewport950x540. The flexible row height is30px above the170px goal: actual long description/cost/prerequisite glyph measurements require the room without uniformly shrinking fonts. All actions are164.5x31.5, exact47:9, CraftButton Simple/preserveAspect, existing child targetGraphics retained. Only heading maxima changed to40/36; other label sizing remains individually fitted24–36 (actions max32). All hierarchy localScale values remain1. Compact card/item 9-slice borders use integer4x pixels (Image multiplier1.5625); windows retain8x pixels (0.78125). No sprite, importer, font or material identity changes.

C3: matching Unity2022.3.62f3 graphics-enabled compile exited0 (Direct3D11). Final EditMode XML reports54 passed,0 failed/skipped, including both real EnterPlayMode fixtures. Intermediate checks caught shop padding and unloaded-prefab anchor measurement problems; final checks use read-only resolved clones plus actual Game scene measurements. Final assertions cover exact centered bounds, dimension caps, full root dimmer, card gaps, window/viewport containment, child overlap, label overflow, localScale and actual glyph ink clear of sliced borders. Shop scrolling rows are clipped by the original viewport, not incorrectly required to all fit simultaneously.

Actual Game frames (screen pixels; isolated frames match):

| Capture | LevelUp frame (x,y,width,height) | Shop frame (x,y,width,height) |
| --- | --- | --- |
| 1920x1080 | 410,200,1100,680 | 385,165,1150,750 |
| 1280x720 | 273.33,133.33,733.33,453.33 | 256.67,110,766.67,500 |
| 1280x1024 | 202.72,241.68,874.55,540.63 | 182.85,213.86,914.30,596.28 |

Measured local card/item/action/viewport sizes are300x420 /950x200 /164.5x31.5 /950x540; receipts enumerate actual transformed bounds. At1280x720 these render as200x280 /633.33x133.33 /109.67x21 /633.33x360. No TutorialBox frame reaches screen edges.

Fresh evidence: `Proyecto-Final/Library/MagicGardenCompactUIValidation/compile.log`, `editmode.log`, `editmode-results.xml`, `task-baseline.json`, and thirteen PNGs with matching `-bounds.txt` / `-text-fit.txt` receipts:
- `runtime-levelup-{1920x1080,1280x720,1280x1024}.png`
- `runtime-shop-{1920x1080,1280x720,1280x1024}.png`
- `isolated-levelup-{1920x1080,1280x720,1280x1024}.png`
- `isolated-shop-{1920x1080,1280x720,1280x1024}.png`
- `isolated-empty-1920x1080.png`
PNG headers confirm those exact dimensions. Old art evidence directory was not written.

Production Game fixture preserved choice consumption, modal exclusion, timeScale pause/release, suspended input consumers, disabled purchase rejection, production resource spending/stack increments and close semantics. Isolated fixture preserved empty Continue, fixed-card selection, disabled purchase, callback deduplication and all five button states/tints. Tutorial skip and deliberate material grants remain test-only controls, not production gate changes. Captures temporarily simulate the existing CanvasScaler1920x1080/match0.5 against RenderTextures, then restore it; ExecuteEvents pointer callbacks are automated, not physical input. Capture camera renders the actual Game UI/HUD layer, not a full gameplay-camera screenshot. Manual Editor/input/scroll-wheel inspection remains unperformed.

Preservation: task-baseline settings SHA256 `561bdb51462a88b913f961094eb3b1d6ed28acd73c223ae9e85379b2a25ae025`; after native runs only ten verified absent default settings and runInBackground were surgically returned to this exact baseline. Both fonts, Canvas, Game scene, Packages, .gitignore and four prefab metadata hashes match the task snapshot. Every original prefab object/component ID remains; complete View and Button blocks are byte-identical to task baseline; panels add four IDs apiece, card/item none. Existing uncommitted tutorial-art/resource/test changes remain. Scoped whitespace validation passes; global check still reports only the three pre-existing fallback-font whitespace lines.

C4: parent read actual runtime level-up/shop1920x1080 and level-up1280x720 PNGs and confirmed compact centered windows; spot-checked final54/54 XML and scoped diff--check. Independent read-only verifier muyoz3jr-j-dkpe inspected runtime panel screenshots at1920x1080/1280x720, exact bounds and text-fit receipts, prefab/test diffs, preservation receipts and native logs; no blocker found. Last shop row clipping is intentional ScrollRect masking; Close remains contained outside viewport. No native reruns or changes by verifier. Physical input/manual scroll and human in-Editor operation remain unverified; PlayMode fixture pointer assertions and actual UI/HUD render inspection completed. No commits/PRs/dependency/runtime/generator/Canvas/scene edits. Strict TDD not activated; ordinary observed checks only. Engram mirror pending because tool unavailable.
