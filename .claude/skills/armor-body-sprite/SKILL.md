---
name: armor-body-sprite
description: "Use when creating or reworking a NarutoOverhaul armor set's Body equip-layer sprite sheet (Content/Items/Armor/<Name>BodyItem_Body.png). Builds a real 9x4 vanilla-shaped Body sheet from a vanilla reference plus custom art, instead of the broken 'one static frame tiled into all 36 cells' approach."
---

# Armor Body sprite sheet workflow

Builds `Content/Items/Armor/<Name>BodyItem_Body.png` (360x224, 9 cols x 4 rows
of 40x56 cells) the right way: real per-frame vanilla animation shapes plus
custom idle art, not one frame duplicated into all 36 cells (that's what was
broken before and read as "the player turns into a solid vest").

## 0. Read the frame map first

`/home/ema/Downloads/Armor_Mapping.png` (360x224, same grid) labels what each
row/column means:

- **Cols 0-1**: Jump pose
- **Cols 2-6**: Front (rows 0-1) / Back (rows 2-3) walk-cycle frames
- **Cols 7-8**: Extend
- **Rows 0, 2**: main torso frames ("MALE"/"FEM.")
- **Rows 1, 3**: "SHOULDER" — a separate arm/shoulder sub-animation, not torso

Cross-check any "this frame looks wrong" report against this map before
guessing which cell to fix.

## 1. Pick a real vanilla Body reference

Prefer `tools/reference/armor_animations/Body/*.png` — these are labeled by
real vanilla armor name (e.g. `NinjaShirt_14.png`) and already ground-truthed.
An unlabeled source (e.g. a `Downloads/Terraria/Armor/Armor_<N>.png` dump) can
work too, but verify it's actually torso art before using it: scan per-cell
opaque-pixel counts across the 9x4 grid — cells (0,0)/(1,0)/(0,2)/(1,2) should
be the bulkiest (idle/jump pose), and the shape should look like a garment,
not a chain/accessory, when you render it at 3x with NEAREST scaling.

```python
from PIL import Image
im = Image.open(path).convert('RGBA')
CELL_W, CELL_H = 40, 56
for row in range(4):
    print(row, [sum(1 for y in range(CELL_H) for x in range(CELL_W)
                     if im.crop((c*CELL_W, row*CELL_H, (c+1)*CELL_W, (row+1)*CELL_H)).load()[x,y][3] > 0)
                for c in range(9)])
```

Don't trust matching pixel dimensions alone (360x224 is reused by several
unrelated equip-layer types) — always render and eyeball the grid.

## 2. Get the idle-frame bbox from the reference

Read the exact bbox of cells (0,0)/(1,0)/(0,2)/(1,2) in the chosen base — this
tells you where real vanilla art sits (usually a small band around the
shoulders/chest, e.g. x:12-32, y:26-42 — NOT bottom-anchored down to the feet).
Any custom art you paste in must match this position and size, or it'll be
oversized and block the whole player (the original bug).

## 3. Build custom idle art (optional)

If working from a hand/AI-drawn reference image (not a vanilla sheet) for the
idle pose specifically:

1. Auto-crop the reference to its non-white bbox.
2. Make near-white background pixels transparent.
3. Downscale with `Image.LANCZOS` to the target size found in step 2 (not an
   arbitrary size — match the vanilla base's own idle-frame dimensions).
4. Snap alpha to a hard 0/255 edge (threshold ~140) to avoid soft/blurry
   antialiased fringes in the final pixel-art sheet.
5. Paste at the exact offset found in step 2, not centered/bottom-anchored.

## 4. Recolor the vanilla base's real per-frame shapes

For every other cell (walk/turn/jump frames), reuse the vanilla base's own
differentiated per-frame silhouettes — recolored to the target palette by
luminance, via `tools/recolor_armor_sheet.py` (edit its `GRADIENT` list to the
new armor's palette; sample target colors from that armor's existing
Head/Legs textures so everything matches). This preserves a real walk
animation instead of a static duplicate.

```
python3 tools/recolor_armor_sheet.py <base.png> <recolored.png>
```

Then overlay the step-3 custom idle art onto cells (0,0)/(1,0)/(0,2)/(1,2) of
the recolored sheet (paste, don't blend).

## 5. Apply and verify

1. `identify` the new file — must stay exactly 360x224, no shape regression.
2. Copy to `Content/Items/Armor/<Name>BodyItem_Body.png`.
3. `bash tools/build.sh` then `bash tools/smoke_test_server.sh` — both must
   pass before calling it done.
4. Render the sheet at 3x with NEAREST scaling (and optionally an annotated
   grid with cell borders) and check it before asking the user to launch the
   game — cheaper than a round trip through the client.

## Not part of this skill

Clearing/emptying frames (e.g. the "SHOULDER" rows, or the walk/turn columns)
to fully transparent is a **vest-specific** fix, not a general Body-sprite
step — a sleeveless vest has no real content for those poses, but a
full-sleeve robe/jacket armor piece would. Decide per-armor whether any cells
should be left empty instead of painted.
