# NarutoOverhaul notes for Claude

## When fuzz sweeping still can't trim cleanly: scattered JPEG noise across the WHOLE canvas (found 2026-08-18)

Occasionally a Gemini source JPEG has noise that isn't just at the fuzz=18
elbow (see below) - `-trim` bbox stays stuck near the full 1024x1024 canvas
even up to fuzz=50%, because scattered near-white (not green-tinted) pixels
survive color-keying anywhere on the canvas, not just near the subject edge.
Symptom: the landed icon's `getbbox()` is the full canvas AND a corner pixel
sample shows fully opaque (alpha=255) noise, e.g.
`convert file.png -crop 3x3+0+0 txt:-` showing near-white opaque pixels
instead of alpha=0. A `-fuzz` sweep (10/14/18/22/25/30/40/50) does not
converge - this is a different failure mode from the original fuzz=12 bug,
not fixable by raising fuzz further (the noise pixels are genuinely far in
color-distance from the green key, so no fuzz threshold catches them without
also eating the subject).

Fix: skip auto-trim, pass an explicit `crop=WxH+X+Y` to
`convert_gemini_sprite.sh` (its 7th positional arg; `land_icon.sh` doesn't
expose this, call `convert_gemini_sprite.sh` directly on the latest download
instead). Get the crop rectangle by zooming into the Gemini in-page preview
(`mcp__claude-in-chrome__computer` `zoom` action) around just the image
card, reading off the subject's bounding box in screenshot pixels, then
scaling proportionally to the raw image's actual dimensions (usually 1024x1024
even though the on-page preview renders smaller/differently-cropped). This
is the same manual-crop escape hatch already documented in
`convert_gemini_sprite.sh`'s own header comment for hue-colliding decorative
elements - it applies here too, just for a different root cause.

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

## Icon generation pipeline: why fuzz=18 + point filter (fixed 2026-08-17)

`tools/land_icon.sh` + `tools/convert_gemini_sprite.sh` is the standing
pipeline for turning a Gemini-generated icon image into a game-ready PNG:
generate in the Gemini web UI (browser automation), download, run
`tools/land_icon.sh <dest_relpath_under_Content_Items> <w> <h> [bg_color] [fuzz]`.

**Root cause of icon blur/muddiness, fully diagnosed 2026-08-17**: it was
*not* the resize filter. It was `-fuzz 12%` (the old default) failing to
key out the flat background color cleanly - Gemini's "flat" background is
actually full of JPEG compression noise, which at 12% fuzz leaves speckled
opaque pixels scattered across the *entire* canvas (confirmed via
`convert file.jpeg -fuzz 12% -transparent "#00FF00" -alpha extract` - the
mask showed noise everywhere, not just subject edges). That noise then
defeats `-trim`'s bounding-box detection (it can't shrink past the noise),
so the crop silently returns the full original canvas and squishes it into
the tiny target size instead of a tight crop around the subject.

Fix: raised the default fuzz `12 -> 18` in both scripts, and standardized
the fuzz argument as a **plain number, no `%`** (the scripts append it
themselves - passing `"18%"` used to silently double up to `"18%%"`,
which ImageMagick tolerated but which masked how low the effective value
really was).

Also tried switching the downscale from `-filter point` to `-filter box`
(theory: nearest-neighbor picks one random source pixel per output pixel
when shrinking a smooth ~1400px AI image ~15-20x down to a ~30px canvas,
so it should alias). A/B tested side by side once the fuzz bug was fixed:
box's averaging blends neighboring colors into soft, semi-blended edges,
which reads as *worse* for this flat-color pixel-art style even though it
scores lower on a naive "unique edge color count" metric. Reverted to
`-filter point` - keep it that way; it's correct for this art style.

**Per-image tuning still needed sometimes**: even at fuzz=18, some source
images (heavier JPEG noise, or busy detail near the frame edges) still
fail to trim cleanly. Symptom: the landed PNG's `Image.getbbox()` (PIL)
comes back suspiciously small/off-center relative to the canvas, or a
`-filter point -resize 1000%` zoom (see below) shows a fragmented/cropped
result that doesn't match the Gemini preview. Fix: sweep fuzz manually,
e.g.:
```
for f in 10 14 18 22 25; do
  convert "$SRC" -fuzz "${f}%" -transparent "#00FF00" -trim +repage -format "%wx%h%O\n" info:
done
```
and re-run `land_icon.sh` with whichever fuzz value gives a *stable*
crop size across a couple of neighboring values (the failure mode is a
crop that's either the full 1408x768 canvas or a tiny fragment; a good
fuzz gives a plausible subject-sized crop, consistent across at least 2
adjacent fuzz values).

**Background color choice**: default to green (`#00FF00`); switch to
magenta (`#FF00FF`) only when the subject itself is green-dominant.
Conversely, if the subject is red/warm-dominant, magenta can eat into it
(magenta and red share hue characteristics) - use green instead even for
red subjects. When in doubt, regenerate with the other key color rather
than fighting the spill-cleanup pass.

**Verification habit that catches both bugs above**: after every
`land_icon.sh` call, immediately run
`python3 -c "from PIL import Image; im=Image.open('<path>'); print(im.size, im.getbbox())"`
and eyeball a `convert <path> -filter point -resize 1000% <tmp>.png`
zoom via the Read tool. A bbox that's suspiciously small, off-center, or
touches only one side of the canvas is the signal to fuzz-sweep rather
than trust the default.

## Gemini web UI browser-automation gotchas (2026-08-17)

Driving Gemini's image generation through `mcp__claude-in-chrome__*`:

- **First click-after-navigate frequently drops.** The very first
  `left_click` on the composer right after a page transition (new tab,
  after downloading, after a long response) often lands but the
  following `type` or the send click doesn't register - the message
  shows "Has parado esta respuesta" (stopped) or the text just sits
  unsent in the composer. Recovery: screenshot, check if the composer
  still holds your typed text, and if so just click the send arrow
  again (no need to retype). This happens often enough to just expect
  it as a normal step, not an error.
- **Clicking the image to download can open a full-screen edit overlay**
  instead of downloading directly, depending on exact pixel position and
  viewport size (which drifts across the session as the conversation
  grows taller). If the "latest download" check comes back stale
  (same file as before), screenshot first - if you see the edit overlay
  (color swatches, "Bocoto"/"Texto" tools, a "Guardar" button top
  right), click the download icon in *that* overlay's top-right toolbar
  instead of retrying the original click coordinates.
- **Viewport dimensions shift** (seen both 1549x784, 1558x789, 1512x794
  in one session) as the page layout changes - don't hardcode composer/
  send-button coordinates across turns; re-screenshot and re-locate them
  when a click doesn't land where expected.
- Tabs occasionally freeze (`CDP sendCommand "Page.captureScreenshot"
  timed out`) - recovery is `tabs_context_mcp` -> `tabs_create_mcp` a
  fresh tab -> `navigate` back to the same Gemini conversation URL ->
  `tabs_close_mcp` the frozen one.

## AnimLib (considered, not adopted)

Real tModLoader library mod (by TwiliChaos, https://github.com/Ilemni/AnimLib)
for building more complex custom player-layer animations via
`AnimationSource`/`AnimationController` + tML's `PlayerLayers`. It's a code
framework, not an art source - only relevant once we already have good art and
want fancier animation logic than the simple equip-layer template currently
used in `tools/generate_armor_sheet.py`.
