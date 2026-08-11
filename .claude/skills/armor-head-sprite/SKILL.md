---
name: armor-head-sprite
description: "Use when creating or reworking a NarutoOverhaul armor set's Head equip-layer sprite sheet (Content/Items/Armor/<Name>HelmetItem_Head.png) and its ModItem C# flags. Builds a small forehead-band sheet from another installed mod's asset instead of a full-head-covering block, and fixes the face/hair visibility flags that go with it."
---

# Armor Head sprite sheet workflow

Builds `Content/Items/Armor/<Name>HelmetItem_Head.png` (40x1120, a single
40px-wide column of 20 stacked 40x56 frames) as a small forehead band, not a
full-head-covering block — the latter is what caused "the headband covers the
whole head" and, after a naive fix, "the face/hair goes invisible."

## 1. Diagnose: is the current asset oversized?

Scan per-frame opaque-pixel count and bbox across all 20 frames:

```python
from PIL import Image
im = Image.open(path).convert('RGBA')
CELL_H = 56
for i in range(20):
    cell = im.crop((0, i*CELL_H, 40, (i+1)*CELL_H))
    px = cell.load()
    pts = [(x,y) for y in range(CELL_H) for x in range(40) if px[x,y][3] > 0]
    xs=[p[0] for p in pts]; ys=[p[1] for p in pts]
    print(i, len(pts), (min(xs),min(ys),max(xs)+1,max(ys)+1) if pts else None)
```

A real forehead band is small (~150-200 opaque px, ~18px wide x ~16-18px
tall) and shifts slightly frame-to-frame (a subtle head-bob, not a static
duplicate — same shape, small vertical offset). A broken "full head" asset is
much bigger (600+ px, 25-35px tall) and usually identical in every frame.

## 2. Find a real reference from another installed mod

This repo's own `tools/reference/armor_animations/` doesn't include Head
pieces shaped like a plain band. Instead, pull from an installed Workshop mod
via `tools/extract_tmod.py` (already used successfully for
ThoriumMod's `RogueRoninHeadband_Head` — small scale, real subtle bob
animation, a distinguishable metal-plate patch vs. cloth band by luminance).

```
# find installed mods
find ~/.steam/*/steamapps/workshop/content/1281930/ -iname "*.tmod"

# list candidates by name (bandana/headband/ninja/etc.)
python3 tools/extract_tmod.py list <Mod.tmod> "_Head" | grep -iE "headband|bandana|ninja|shinobi|cloth"

# extract + decode to png
python3 tools/extract_tmod.py extract <Mod.tmod> "Items/.../Name_Head.rawimg" out.png
```

Render at 3x/NEAREST in a 5x4 grid (or similar) and eyeball it before
committing — same principle as the Body workflow: don't trust the name or
dimensions alone.

## 3. Recolor by discrete luminance levels, not a gradient

Small pixel-art assets like this typically use only a handful of distinct
luminance values (e.g. 7 exact levels), representing outline / cloth-shadow /
cloth / plate-shadow / plate / plate-highlight. Print the actual histogram
and map each exact level explicitly rather than using a continuous gradient
(a continuous gradient blurs the plate/cloth distinction that makes the
design read clearly at this size):

```python
LUMA_MAP = {
    32:  (10, 10, 15),    # outline
    54:  (18, 24, 48),    # cloth dark
    75:  (30, 35, 62),    # cloth mid
    97:  (43, 48, 82),    # cloth light
    129: (100, 100, 120), # plate shadow
    162: (132, 130, 149), # plate mid
    203: (172, 180, 202), # plate highlight
}
```

Sample the target palette from the user's reference image (crop-and-sample a
grid of points over the cloth region and the plate region separately, take
the most common colors in each) rather than guessing hex values.

## 4. Apply and verify shape

`identify` the output — must stay 40x1120. Copy to
`Content/Items/Armor/<Name>HelmetItem_Head.png`.

## 5. Fix the ModItem C# flags — this is not optional

A full-head asset and a forehead-band asset need opposite settings on the
`<Name>HelmetItem.cs`'s `SetStaticDefaults()`:

- **`ArmorIDs.Head.Sets.DrawHead[Item.headSlot]`**: `false` for a full-head
  asset (hide the player's own face, it's fully covered anyway); must be
  **removed / left default-true** for a band asset, or the player's face
  renders invisible (the band is drawn but nothing is underneath it).
- **`ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true`**: required for a
  band asset once `DrawHead` is no longer suppressed — without it the face
  shows but hair stays hidden (tModLoader defaults to "helmet covers hair"
  unless a hat/band explicitly opts into showing it, same as vanilla bandana
  items).

Both bugs are silent — no build error, no crash, just wrong rendering in
front of the user. Always check both flags together when switching a Head
asset from full-coverage to a partial band, and re-run the smoke test.
