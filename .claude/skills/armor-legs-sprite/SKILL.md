---
name: armor-legs-sprite
description: "Use when creating or reworking a NarutoOverhaul armor set's Legs equip-layer sprite sheet (Content/Items/Armor/<Name>LegsItem_Legs.png). Builds a real 20-frame boot/ankle animation from another installed mod's asset instead of the broken 'one static frame tiled into all 20 cells' approach."
---

# Armor Legs sprite sheet workflow

Builds `Content/Items/Armor/<Name>LegsItem_Legs.png` (40x1120, a single
40px-wide column of 20 stacked 40x56 frames — same strip layout as Head, no
row/column grid like Body has).

## 1. Diagnose: is the current asset a static duplicate?

Same check as Head — scan opaque-pixel count per frame:

```python
from PIL import Image
im = Image.open(path).convert('RGBA')
CELL_H = 56
for i in range(20):
    cell = im.crop((0, i*CELL_H, 40, (i+1)*CELL_H))
    px = cell.load()
    opaque = sum(1 for y in range(CELL_H) for x in range(40) if px[x,y][3] > 0)
    print(i, opaque)
```

If every one of the 20 values is identical, it's the same "one frame tiled
everywhere" bug that broke Body — the Legs equip texture only needs to draw
the boot/ankle-cuff area (the rest of the leg is the player's own leg
sprite), but it still needs a real walk-cycle: frames 0-4 are typically the
idle pose (identical to each other, that's correct), then frames 5+ show
genuine per-frame variation as the leg strides.

## 2. Find a real reference from another installed mod

`tools/reference/armor_animations/Legs/NinjaPants_14.png` is a good on-theme
starting point, but for a specific requested art style (e.g. "these vanilla
pants look ugly, find something better"), pull additional candidates from an
installed Workshop mod via `tools/extract_tmod.py`:

```
python3 tools/extract_tmod.py list <Mod.tmod> "_Legs" | grep -iE "ninja|shadow|assassin|rogue|stealth"
python3 tools/extract_tmod.py extract <Mod.tmod> "Items/.../Name_Legs.rawimg" out.png
```

Render as a 5x4 grid (reshape the 20-frame vertical strip for easier
viewing) and confirm real per-frame variation before picking one — present
multiple candidates to the user side by side rather than guessing which
"feels" right.

## 3. Recolor to match a reference image

Two approaches, both keeping the chosen base's real per-frame silhouette
untouched (only recoloring pixels, never touching alpha/shape):

- **Whole-sheet luminance gradient** (`tools/recolor_armor_sheet.py`) when
  you just want "the same design, different color" — preserves all of the
  base's own shading detail.
- **Band-by-local-Y recoloring** when matching a specific multi-material
  reference (e.g. navy pant cuff / white bandage wrap / dark sandal sole,
  sampled from a user-provided character reference image). Get the real
  per-frame bbox first (`min(ys)`/`max(ys)` across opaque pixels) so the
  bands line up with where the art actually sits in the cell — don't guess
  fixed y-thresholds.

```python
if local_y < CUFF_BOUNDARY: color = cuff_color
elif local_y < WRAP_BOUNDARY: color = wrap_color
else: color = sole_color
```

Sample target colors directly from the user's reference image at a few
representative points (ankle cuff, wrap, sole) rather than guessing hex
values.

## 4. Apply and verify

`identify` the output — must stay 40x1120. Copy to
`Content/Items/Armor/<Name>LegsItem_Legs.png`, run `tools/build.sh` +
`tools/smoke_test_server.sh`.

## Not part of this skill

Unlike Head, Legs items generally don't need companion C# flag changes
(there's no "hide the player's own legs" flag being toggled here) — the
fix is purely the sprite sheet itself. If a future Legs rework does need a
matching C# change, treat it as armor-specific, not a general step.
