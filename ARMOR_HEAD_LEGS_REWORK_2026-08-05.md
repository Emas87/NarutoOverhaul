# Ninjutsu armor rework session — 2026-08-05

## Context

Started from a bug report: the Ninjutsu Body armor made the player look like
they'd "turned into a solid vest" — the equip-layer art blocked the whole
torso instead of leaving room for the player's own arms/neck to show. This
snowballed into a full rework of Ninjutsu's Body, Legs, and Head equip
textures, plus fixing the same underlying bugs in the game's separate
Village Headband vanity items.

Two new skills capture the reusable parts of this work:
`.claude/skills/armor-body-sprite/SKILL.md`,
`.claude/skills/armor-head-sprite/SKILL.md`,
`.claude/skills/armor-legs-sprite/SKILL.md`. Read those before repeating
this on Taijutsu/Genjutsu.

## What was actually wrong (root causes)

1. **Body**: `NinjutsuBodyItem_Body.png` frame art filled its 40x56 cell
   almost edge-to-edge (73% pixel coverage, no negative space) — plus the
   whole 9x4 sheet was one static frame duplicated into all 36 cells instead
   of a real per-pose walk-cycle.
2. **Legs**: `NinjutsuLegsItem_Legs.png` was the same "one frame duplicated
   into all 20 cells" bug (every frame had identical opaque-pixel count).
3. **Head**: `NinjutsuHelmetItem_Head.png` drew a full head/face block
   (656 opaque px, ~26x35) instead of a small forehead band, and the
   `ArmorIDs.Head.Sets.DrawHead[...] = false` override (needed for a
   full-face asset) hid the player's real face once removed — needed a
   companion `DrawFullHair[...] = true` to bring hair back too.
4. **Village Headbands** (`LeafVillageHeadbandItem_Head.png` etc., separate
   pure-vanity items from the Ninjutsu class item): were a single static
   **40x40** icon — not even the right 40x1120 20-frame strip shape tML
   expects. Never could have rendered correctly.
5. **Village Headband source asset gotcha**: the ThoriumMod base used to
   rebuild these (`RogueRoninHeadband_Head`) actually contains **two
   separate connected shapes** per frame — a top forehead-band strand and a
   lower arc that reads as a mouth mask. A naive row-based cut doesn't
   separate them (they interleave: same rows, different x-ranges). Had to
   use `scipy.ndimage.label` (8-connectivity) to properly isolate and keep
   only the topmost connected component.

## How each was fixed

- **Body**: rebuilt from a real vanilla Body sheet
  (`/home/ema/Downloads/Terraria/Armor/Armor_28.png`, user-picked) for the
  real per-frame walk/turn animation shapes (recolored by luminance gradient
  via `tools/recolor_armor_sheet.py`), with custom art for the idle/jump
  frames only, built from a user-supplied Gemini reference image (downscaled,
  armhole + neck negative-space carved in). Per the `Armor_Mapping.png`
  legend, rows 1 & 3 are a separate "SHOULDER" sub-animation — cleared to
  fully transparent since this is a sleeveless vest (a robed/sleeved armor
  piece would want real content there instead).
- **Legs**: rebuilt from ThoriumMod's `StealthPants_Legs` (extracted via
  `tools/extract_tmod.py` from the installed Workshop mod), recolored by
  luminance gradient to blue — kept exactly as the user's final pick (an
  earlier band-by-Y variant styled after a Kakashi reference photo was tried
  and rejected in favor of the simpler whole-sheet gradient recolor).
- **Head (Ninjutsu)**: rebuilt from ThoriumMod's `RogueRoninHeadband_Head`,
  recolored by mapping each of its 7 discrete luminance levels to a
  cloth/plate palette sampled from the user's `leaf.png` reference. Then
  needed the `DrawHead`/`DrawFullHair` C# flag fix (see root cause #3), and
  later the two-arc mouth-mask strip (root cause #5).
- **Village Headbands**: same ThoriumMod base, recolored per-village from 5
  user-provided reference images (`leaf.png`, `cloud.png`, `sand.png`,
  `mist.png`, `stone.png` in `~/Downloads`), same `DrawFullHair` C# fix
  added to all 5 `*VillageHeadbandItem.cs` files, same two-arc strip.

## Assets preserved for later (not deleted)

The old 40x40 village-headband icons had a distinct "face + mask" look the
user liked for a future dedicated character item. Before overwriting, they
were moved (not deleted) to `tools/reference/mouth_mask_designs/`:
- `LeafMouthMaskDesign.png`, `CloudMouthMaskDesign.png`,
  `SandMouthMaskDesign.png`, `MistMouthMaskDesign.png`,
  `StoneMouthMaskDesign.png` — trimmed to hair+headband only (mouth/face
  region erased), since only the Kakashi-flagged one should keep a face.
- `KakashiHeadbandDesign.png` — copy of the original Leaf design, face kept
  intact (old 40x40 icon format — NOT wired to any live item yet).
- `KakashiHeadbandItem_Head_reference.png` — the **new-format** (40x1120,
  properly animated) two-arc ThoriumMod-based sprite, navy palette, full
  band+mask kept intact. This is the one to actually use if/when a live
  `KakashiHeadbandItem` gets built — it's already in the correct equip
  texture format, just needs a `ModItem` class + `.cs` wiring to go live.

## Cleaned up

Deleted `tools/armor_wip/` (74 untracked scratch/preview files generated
during this session, ~612K) — none were tracked in git, and all final chosen
sprites are already committed to `Content/Items/Armor/`.

## Verification workflow used throughout

Every applied change went through: `identify` (confirm exact pixel
dimensions unchanged — 360x224 for Body, 40x1120 for Head/Legs) →
`bash tools/build.sh` → `bash tools/smoke_test_server.sh` → both must pass
before calling anything done. Screenshots/renders were sent to the user via
`SendUserFile` for visual approval *before* copying a candidate into the
live `Content/Items/Armor/` path, every time.

## Still pending (not started)

- **Taijutsu and Genjutsu Body**: still use `generate_armor_sheet.py`'s
  "duplicate frame 0 into all 36 cells" approach — not yet reworked with the
  Body skill's per-frame-animation technique.
- **Taijutsu and Genjutsu Legs**: confirmed to have the same static-duplicate
  bug as Ninjutsu's Legs had (`TaijutsuLegsItem_Legs.png` and
  `GenjutsuLegsItem_Legs.png` both have one identical opaque-pixel count
  across all 20 frames). Not yet reworked.
- **Taijutsu and Genjutsu Head**: still use the old oversized full-head
  block (574 and 461 opaque px respectively, vs. the ~88px fixed band size).
  `DrawHead = false` was already removed from both `.cs` files earlier in
  this session (per user request), but their `_Head.png` art itself hasn't
  been rebuilt yet — so right now those two will likely show a doubled-up/
  oversized face until the same Head rework is done. This should probably be
  the very next thing tackled, since it's a live visual regression, not just
  unfinished scope.
- **Kakashi headband item**: reference art exists and is ready
  (`KakashiHeadbandItem_Head_reference.png`), but no `ModItem` class/wiring
  exists yet to make it a real obtainable item.
- Consider whether Taijutsu/Genjutsu Body/Legs should reuse ThoriumMod/other
  installed-mod sources (as Ninjutsu's Legs and Head ended up doing) instead
  of the original vanilla-reference-only approach, given how much better
  those turned out.
