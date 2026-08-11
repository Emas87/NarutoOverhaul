# NarutoOverhaul notes for Claude

## Extracting reference art from installed Workshop mods (implemented 2026-08-04)

`tools/extract_tmod.py` unpacks any `.tmod` (ours or another installed mod's,
e.g. ThoriumMod/CalamityMod) and decodes `.rawimg` textures to real `.png`.
Written from scratch against tModLoader's own `TmodFile.cs`/`ImageIO.cs`
format (no external unpacker tool needed - `.tmod` is a custom binary
container, not a zip). This is what revealed the template-shape bug below -
see `ARMOR_REFERENCE_IMAGES.md` "Template shape correction" for the full
story and exact commands used.

`.tmod` files live under `~/.local/share/Terraria/tModLoader/Mods/*.tmod`
(locally installed) and
`<tModLoader install>/steamapps/workshop/content/1281930/<id>/<tml-version>/`
(Workshop mods).

## CRITICAL: real armor equip-layer template shapes (found 2026-08-04)

`tools/generate_armor_sheet.py` previously used an invented 800x448 (20 cols
x 8 rows) canvas for every equip type. Ground-truthed against a real shipped
armor piece (ThoriumMod's `YewWoodBreastguard`, extracted via
`extract_tmod.py`) that the REAL vanilla-compatible shapes are:

- **Body / Arms**: 360x224 = 9 columns x 4 rows of 40x56 cells
- **Head / Legs**: 40x1120 = 1 column x 20 rows of 40x56 cells - a TALL
  VERTICAL STRIP, not a wide grid

This was likely the real mechanical cause of the "weird full-face head" bug,
independent of art quality - tModLoader slices a Head texture expecting a
40px-wide column, so a Head file built on the old wide-grid canvas gets
sliced into completely wrong frame regions no matter how good the art is.
The script now takes `--equip-type {body,arms,head,legs}` and picks the
correct shape automatically. Any armor art generated before this fix should
be considered suspect and re-tiled/regenerated against the corrected shapes.

## Player Renderer (Workshop tool, installed 2026-08-04)

Steam Workshop mod by Auxves (id `2830137447`) - lets you render your
currently-equipped player character to a real spritesheet from inside the
game: type `/render <name>` in chat while playing, output saves to
`~/.local/share/Terraria/tModLoader/Sprites/`. Used to get a genuine
high-res reference of a character/armor instead of decoding vanilla `.xnb`
assets (which would require writing/finding a proper XNB decoder - a PyPI
package literally named `xnb` was tried and is an unrelated ML library, not
useful here).

## AnimLib (considered, not adopted)

Real tModLoader library mod (by TwiliChaos, https://github.com/Ilemni/AnimLib)
for building more complex custom player-layer animations via
`AnimationSource`/`AnimationController` + tML's `PlayerLayers`. It's a code
framework, not an art source - only relevant once we already have good art and
want fancier animation logic than the simple equip-layer template currently
used in `tools/generate_armor_sheet.py`.
