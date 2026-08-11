# Vanilla Terraria armor reference images needed

Goal: grab a handful of **vanilla** armor images from the official Terraria Wiki
(terraria.wiki.gg) to use as pose/layout reference images when generating our own
Taijutsu / Ninjutsu / Genjutsu armor animation sheets with nano-banana. These are
reference only (used as an image-conditioning input alongside our own asset + a
text prompt) — final art stays in our own color palette/style, we're just borrowing
proven pose composition and frame timing from Re-Logic's own equipped-animation art
so we don't have to describe every pose from scratch in text.

I can't fetch these myself (terraria.wiki.gg blocks automated/bot requests), so please
grab them with your tool and save them under `tools/reference/` in this repo, using
the filenames below. Whatever image format the wiki serves (png/gif/webp) is fine —
if it's an animated gif, either the gif itself or a couple of exported frames both work.

## What to look for on each page

For each armor set page below, there are usually two useful kinds of image:
1. An **"Equipped" preview** (often a small looping gif/animation near the top of the
   page or in the infobox) showing a character wearing the piece while idle/walking —
   this is the most useful one, it shows real in-game frame poses and timing.
2. The **inventory icon images** for the Head/Body/Legs pieces individually (usually
   named like `<Item Name>.png`, shown in the item infobox) — useful as a secondary/
   fallback reference if no equipped animation is available.

Grab whichever of these two you can get per set; the equipped animation is preferred.

## Template shape correction (2026-08-04) — READ BEFORE TOUCHING ANY ARMOR FILE

Every armor piece regenerated so far (Taijutsu, and the in-progress Ninjutsu/Genjutsu attempts)
was built on an **invented 800x448 (20 cols x 8 rows) canvas for every equip type**. That shape is
wrong. Extracting and decoding a real shipped armor piece straight out of `ThoriumMod.tmod`
(`Items/ArcaneArmor/YewWoodBreastguard_Body.rawimg` etc., via the new `tools/extract_tmod.py`)
shows the actual vanilla-compatible shapes are:

- **Body / Arms**: 360x224 = 9 columns x 4 rows of 40x56 cells
- **Head / Legs**: 40x1120 = 1 column x 20 rows of 40x56 cells — a **tall vertical strip**, not a
  wide grid

This matches the `tools/reference/armor_animations/` wiki images exactly (also 360x224) — those
were the correct real dimensions all along, not undersized thumbnails as previously assumed here.

This is likely the real *mechanical* cause of the "weird head" bug described below, independent of
art quality: tModLoader reads a Head equip texture expecting a 40px-wide column, so a Head file
built on the old wide-grid canvas gets sliced into entirely wrong frame regions no matter how good
the art in it is. `tools/generate_armor_sheet.py` now takes `--equip-type {body,arms,head,legs}`
and picks the correct shape automatically (see its module docstring) — **every armor piece below,
including the already-"DONE" Taijutsu set, needs to be re-tiled or regenerated against the
corrected shapes before it can be trusted.**

How this was found: rendered a real base-body reference in-game via the Player Renderer Workshop
mod (`/render`, see `CLAUDE.md`), which raised the question of what real experienced mods actually
ship. `tools/extract_tmod.py` was written against tModLoader's own `TmodFile.cs`/`ImageIO.cs`
binary format (no external unpacker needed) to pull a real armor texture out of ThoriumMod for
comparison, which is what surfaced the shape mismatch.

## Arms shape correction (2026-08-04) — the "Body / Arms: 9x4" claim above was half wrong

The "Body / Arms: 360x224 = 9x4" line above was only ever verified for **Body**. ThoriumMod's
`YewWoodBreastguard` — the one real armor piece used to ground-truth it — has **no `_Arms.rawimg`
file at all**, so the Arms shape was extrapolated from Body's shape, never actually checked against
a real file.

Extracting real `_Arms.rawimg` files out of `CalamityMod.tmod` (which has plenty:
`DemonshadeBreastplate_Arms`, `EmpyreanCloak_Arms`, `MeldTransformation_Arms`) shows all three are
**40x1120 — the same single-column vertical-strip shape as Head/Legs, not Body's 9x4 grid.**
Cross-checked the Body claim at the same time across every `_Body.rawimg` in both CalamityMod (42
files) and a second, unrelated mod (DBZMODPORT, 8 files) — all 50 are exactly 322572 bytes
(360x224), zero exceptions, so Body's shape stands confirmed.

Corrected shapes, now used by `tools/generate_armor_sheet.py`'s `EQUIP_TEMPLATES`:

- **Body**: 360x224 = 9 columns x 4 rows of 40x56 cells
- **Arms / Head / Legs**: 40x1120 = 1 column x 20 rows of 40x56 cells — vertical strip

`NinjutsuBodyItem_Arms.png` was rebuilt at the corrected 40x1120 shape (reusing the same PixelLab
sleeve art, just re-tiled). Genjutsu and Taijutsu `_Arms.png` files are still sitting on the old
(and now doubly-wrong) 800x448 shape — the "NEEDS RE-TILING" status below covers the general shape
fix, but their eventual re-tile must target 40x1120 for Arms, not 360x224.

## Status

- **Taijutsu (Stage C): NEEDS RE-TILING.** Previously marked DONE (all 4 pieces regenerated,
  deployed, and verified via build + headless server load test), but that verification predates
  the template-shape correction above — the underlying art may be fine, but it's sitting on the
  wrong-shaped 800x448 canvas and needs re-tiling onto the real 360x224 (Body/Arms) / 40x1120
  (Head/Legs) shapes before it can be considered actually correct.
- **Ninjutsu (Stage D): IN PROGRESS.** `NinjutsuBodyItem_Body.png` art is DONE (PixelLab
  `/inpaint` against a real Player-Renderer body reference, clipped to a clean single idle frame -
  see PixelLab pilot findings below) and correctly shaped. `_Arms.png`, `_Head.png`, `_Legs.png`
  are mechanically re-tiled to the correct shapes (their OLD art, just cropped to frame 0 and
  retiled - not yet regenerated with good art) to confirm/fix a second, more severe shape bug
  found via in-game `/render`: with Body fixed but Head still on the old 800x448 shape, certain
  animation frames rendered a giant floating disembodied head with no headband at all (confirms
  the wrong-shape Head texture doesn't just look bad, it gets sliced into completely wrong regions
  at specific frame indices - a correctness bug, not just an art-quality one). All 4 pieces are
  now on the correct shapes and `tools/build.sh` succeeds. In-game `/render` confirmed via
  Player Renderer: the giant-floating-head bug IS gone (shape fix worked). Found and fixed a
  separate code bug while checking: `NinjutsuHelmetItem.cs` never told the game to hide the
  player's real head/hair, so a big blob of the player's own hair rendered above our headband
  (our Head texture draws a full face, not a partial mask, so it needs
  `ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false` in `SetStaticDefaults()` - added, per the
  same pattern in `tactiq-free-transcript-7tcxaKhhVsc.txt`, a tModLoader armor-set tutorial
  transcript the user provided). `_Arms.png` art has since been redone too: the old art was a
  disconnected blob unrelated to an arm shape (not just "doesn't fully cover the arm"), replaced
  via a PixelLab edit against a real Player-Renderer base-pose reference, masked down to just the
  two sleeve regions, and rebuilt at the corrected 40x1120 shape (see "Arms shape correction"
  above — Arms was wrongly on the 360x224 Body grid shape until this pass). `_Legs.png` is still
  the old mechanically-retiled art, never independently re-verified in-game.

## Second shape bug: vertical anchor within the cell (2026-08-04)

After the col/row shape fix above, the user reported the head texture was rendering at
body-height instead of on the head. Ground-truthed the fix against the same real ThoriumMod
Head file: `YewWoodHelmet_Head.png` frame 0's content sits at y=10..30 within its 56px cell (a
small top margin), NOT foot-grounded at the bottom. `_center_on_canvas()` in
`generate_armor_sheet.py` was unconditionally foot-grounding every equip type (correct for
Body/Arms/Legs, wrong for Head) - `build` now anchors Head content to the top of the cell
(`HEAD_TOP_MARGIN = 10`) and everything else stays bottom-anchored.

Also reconsidered whether Head art needs to be a partial hood/headband mask at all: the already-
working `LeafVillageHeadbandItem_Head.png` (a Head-slot accessory, confirmed fine, no bugs) turns
out to use the exact same full-face style we'd been calling "the weird head bug" - full hair,
face, and headband baked into one texture, same as our Ninjutsu Head art. That style clearly works
fine as long as (a) the canvas shape and anchor are correct (this section's fix) and (b)
`ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false` is set so the player's real head doesn't show
through underneath it. So the original "redesign as a partial mask" plan was likely solving the
wrong problem - the shape/anchor bugs were the actual root cause, not the full-face art style
itself. Reverted to the original well-drawn full-face Ninjutsu head crop (not the partial-band
inpaint experiment, which came out messy/inconsistent - see below) rebuilt with the corrected
top-anchor.

## Partial-mask headband inpaint attempt (2026-08-04, abandoned)

Before the insight above, tried inpainting just a forehead-band-shaped mask (instead of the whole
head) via PixelLab `/inpaint`, hoping for a true partial mask. Two attempts, both inconsistent:
the model only partially repainted the masked region (most of the band stayed the original hair
color, with just a small localized navy patch), and a second attempt with more aggressive
guidance/lower init-image-strength came out worse (barely any headband at all). Not worth further
prompt-tuning right now given the full-face style turned out to be viable after all (see above).
Scratch files if this is revisited later: `head_band_mask.png`, `head_band_inpaint_out.png`,
`head_band_inpaint_out2.png`, `head_band_clipped.png` (all under the session scratchpad, not kept
in the repo).
- **Genjutsu (Stage E): NOT STARTED.** Same situation as Ninjutsu was — needs section 3 below,
  plus must target the corrected template shapes from the start this time.
- **Village headbands (Leaf/Cloud/Mist/Sand/Stone): NO FIX NEEDED.** Checked all 5 —
  they're already proper partial-mask single-frame textures (identical ~740/1600 opaque
  pixel shape across all 5, just recolored), so they don't have the opaque-face bug and
  don't need regenerating. (Note: these are 40x40 single-frame Head layers, not full equip-layer
  sheets, so the Head/Legs 40x1120 shape correction above doesn't apply to them.)

## Official Re-Logic frame semantics (found 2026-08-04)

The user pointed at `/home/ema/Downloads/Sprite Templates/Templates/` - Re-Logic's own official
sprite template pack (the one linked from
https://forums.terraria.org/index.php?threads/the-ultimate-guide-to-content-creation-and-use-for-the-terraria-workshop.100652/,
which is otherwise a Texture-Pack guide, not directly about `AutoloadEquip` modding, but ships
these templates as ground truth for vanilla frame layout). These are at half our working
resolution (20x28 per cell instead of 40x56) but the same grid shapes, which cross-validates
everything in the sections above AND gives us the actual semantic meaning of each frame index,
not just the shape:

**`PlayerTemplate_White_Head.png` / `_Arm.png` / `_Legs.png` / `_Full.png`** (all 20x560 = single
column, 20 rows of 20x28, i.e. our 40x56-scale Head/Legs shape) all share the identical labeled
frame breakdown (confirmed by reading the baked-in text labels row by row):
- **Row 0**: Idle
- **Rows 1-4**: Swing (4 frames)
- **Row 5**: Jump
- **Rows 6-19**: Walk (14 frames)

`_Full.png` is a whole-character (head+torso+arms combined) reference showing this same 20-frame
breakdown - useful as a pose-timing reference across all equip layers, not a literal equip file
shape itself.

**`Armor.png`** (180x112 = 9x4 grid of 20x28, i.e. our 40x56-scale Body/Arms shape) turns out to
be specifically the **Arm/sleeve** template (shoulder + arm shapes, not a torso), laid out as:
- **Rows 0-1**: Male (row 0 = main arm poses, row 1 = "Shoulder" layer)
- **Rows 2-3**: Female (same split)
- **Columns 0-1**: Jump (front/back variants)
- **Column 2**: Front (don/rest pose)
- **Columns 3-6**: swing/motion frames
- **Columns 7-8**: Extend

This means the real Body equip layer (confirmed 9x4/360x224 via the ThoriumMod
`YewWoodBreastguard_Body.rawimg` extraction earlier in this doc) likely shares this same
male/female x main/shoulder-row grid convention, not the single-column Idle/Swing/Jump/Walk
breakdown that Head/Legs use - Body and Arms are cut from the same torso drawing and share a
layout, distinct from Head/Legs' simpler single-pose-per-row strip.

**Practical takeaway for us**: our current pipeline only ever generates real art for frame/index 0
and tiles it into every other slot (a valid, simple convention plenty of real armor uses) - but
now we have the actual roadmap if we want proper per-pose animation later: Legs/Head's Jump pose
is row 5 specifically, and Swing is rows 1-4, so a future pass could generate those distinctly
instead of reusing the idle frame everywhere.

## PixelLab pilot findings (2026-08-04)

Tried PixelLab (`tools/pixellab_generate.py`, `/edit-image`) as an alternative to the
nano-banana/OpenRouter pipeline (OpenRouter free tier ran out of credits mid-session — see
`https://openrouter.ai/settings/credits`). Findings:

- `/edit-image` on a single cropped 40x56 frame with a naive prompt free-invented a whole
  miniature character including its own head/face — unusable as an equip layer (doubles up with
  the player's real head in-game). Explicit "no head/face/legs, keep transparent" prompting fixed
  that, but results were still blurry/anti-aliased (not crisp pixel art) with stray artifact pixels
  (a red dot, a gray dot) baked into the raw output itself — confirmed by inspecting the raw
  pre-tiling frame, not just the final tiled sheet.
- PixelLab's `/inpaint` endpoint (via the official `pixellab` PyPI SDK, not just the ad-hoc
  `/edit-image` wrapper) worked much better: fed it a real body frame rendered from the Player
  Renderer Workshop mod (`base_frame0.png`, cropped from `/render`'s output) plus a mask covering
  just the torso/arms region, asking it to paint armor only inside that mask. Because it's
  inpainting onto the *exact real body pose*, the result stays perfectly aligned — head and legs
  came back pixel-identical to the input, and the torso came back as a crisp, correctly-shaded
  navy jacket with no blur and no artifacts. The one gotcha: pixels inside the mask that PixelLab
  decided were "background" get filled with a flat gray instead of transparency, so the result
  needs to be clipped back to the original frame's alpha channel (intersected with the mask) to
  strip that gray fill before it's usable as an equip-layer texture.
- The `pixellab` PyPI SDK's response models are stale relative to the live API (expects
  `{"type":"usd","usd":...}` usage but the API now returns `{"type":"generations","generations":N}`
  for this key's plan) — raw `requests` calls against `https://api.pixellab.ai/v1/<endpoint>` work
  fine, just don't rely on the SDK's response parsing.

## Sets to fetch (pick ONE per row below, in priority order)

### 1. Taijutsu reference (green gi / martial-arts look) — already validated as a great match, DONE
- Page: https://terraria.wiki.gg/wiki/Ninja_armor
- Save as: `tools/reference/vanilla_ninja_armor_equipped.png` (or `.gif`)
- Why: green ninja gi, close color/style match to our Taijutsu set already, and this
  page's approach already produced our best result so far (used informally, not
  fetched from wiki - now we want the *equipped/walk-cycle* version specifically).

### 2. Ninjutsu reference (dark blue/navy stealth ninjutsu-caster look) — needed next
- Preferred page: https://terraria.wiki.gg/wiki/Shadow_armor
- Save as: `tools/reference/vanilla_shadow_armor_equipped.png` (or `.gif`)
- Fallback if that page has no good equipped animation: https://terraria.wiki.gg/wiki/Necro_armor
  → save as `tools/reference/vanilla_necro_armor_equipped.png`
- Why: both are dark, hooded/wrapped silhouettes closer to a stealthy jutsu-caster
  than a bright color set - good pose reference for a hood + wrapped body.
- Extra per-piece options if the main equipped gif isn't enough on its own:
  - Arms/gloves detail: https://terraria.wiki.gg/wiki/Shadow_armor (chest item page has
    the sleeve/glove closeup) → save as `tools/reference/vanilla_shadow_armor_chest.png`
  - Legs detail: https://terraria.wiki.gg/wiki/Shadow_greaves → save as
    `tools/reference/vanilla_shadow_greaves.png`
  - Head/hood detail (pose only - do NOT use as a base for redesigning our own head,
    per the lesson learned on Taijutsu): https://terraria.wiki.gg/wiki/Shadow_helmet →
    save as `tools/reference/vanilla_shadow_helmet.png`

### 3. Genjutsu reference (purple flowing robe / illusion-caster look) — needed next
- Preferred page: https://terraria.wiki.gg/wiki/Robe
- Save as: `tools/reference/vanilla_robe_equipped.png` (or `.gif`)
- Fallback if that page has no good equipped animation: https://terraria.wiki.gg/wiki/Ancient_armor
  → save as `tools/reference/vanilla_ancient_armor_equipped.png`
- Why: flowing robe silhouette closer to a mystical illusion-caster than a rigid
  plate/cloth armor set.
- Extra per-piece options if the main equipped gif isn't enough on its own:
  - Wizard-style robe alternative (more flowing sleeves): https://terraria.wiki.gg/wiki/Wizard_robe
    → save as `tools/reference/vanilla_wizard_robe.png`
  - Legs detail: https://terraria.wiki.gg/wiki/Ancient_Legs (or the Robe set's own legs
    piece if it has a separate page) → save as `tools/reference/vanilla_ancient_legs.png`
  - Head/hat detail (pose/silhouette only, NOT a base image for redesign):
    https://terraria.wiki.gg/wiki/Wizard_hat → save as `tools/reference/vanilla_wizard_hat.png`

### 4. (Optional, nice-to-have) A generic head/headband example — DONE, already fetched
- Page: https://terraria.wiki.gg/wiki/Ninja_Hood
- Save as: `tools/reference/vanilla_ninja_hood.png`
- Why: for the partial-mask/headband head redesign (already mostly solved using our
  own `LeafVillageHeadbandItem_Head.png`, but a second reference never hurts).

## After you fetch them

Drop the files in `tools/reference/` with the filenames above (create the folder if
it doesn't exist) and let me know - I'll fold them into the next generation prompts
as image-conditioning references alongside our own existing armor pngs.

**Reminder for Head pieces (Ninjutsu/Genjutsu):** per the lesson learned fixing the
Taijutsu head bug, when redesigning `NinjutsuHelmetItem_Head.png` / `GenjutsuHelmetItem_Head.png`,
do NOT use the current broken full-face texture as the nano-banana input reference -
use only a known-good partial-mask asset (e.g. our own `LeafVillageHeadbandItem_Head.png`
or the fetched `vanilla_ninja_hood.png`) as the base, and ask for a pure recolor/restyle
of the visible headband/hood cloth only.
