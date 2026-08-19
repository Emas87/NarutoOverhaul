---
name: vfx-blast-sprite
description: "Use when creating a new large, player-tracking VFX 'blast' effect for a NarutoOverhaul weapon/ability - a multi-frame energy effect that covers most of the player's body during a swing/cast, generated via Gemini (nano-banana) and wired up as a PlayerAnchoredBurstEffectProjectile. Landed 2026-08-18 for the Taijutsu kick/punch blast rework (Iron Leg/Front Lotus/Leaf Hurricane/Gentle Fist)."
---

# Large player-anchored VFX blast pipeline

End-to-end recipe for a body-covering energy VFX that tracks the player
through a swing (not a small fixed-position flash like the existing
`Content/Projectiles/Bursts/*BurstProjectile` family - see
`Common/VFX/BurstEffectProjectile.cs` for that older, smaller pattern).

## 0. Design the concept before generating anything

Pick one clear visual concept per weapon/ability, tied to its own stats:

- **Canvas size**: large enough to read as covering most of the player
  (~120-180px vs. the player's ~56px collision box). Scale roughly with the
  weapon's reach/power.
- **Frame count**: 5-8 is plenty for a one-shot swing effect.
- **`LifetimeTicks`**: match the weapon's own `useAnimation` duration exactly,
  so the blast plays out over precisely the swing, not longer/shorter.
- **Color**: pick a hue that reads clearly against whichever key color you'll
  use (see step 2) - don't make the effect's dominant color the same hue
  family as the background you plan to key out, or the slicer's hue-based key
  will eat the effect along with the background.

## 1. Generate the raw sheet with Gemini (nano-banana)

Two ways to reach nano-banana; try the first, it's faster and has no browser
flakiness - fall back to the second only if it errors:

**A. `tools/generate_armor_sheet.py request`** (direct API via OpenRouter, no
browser needed) - reuse its `request` subcommand as-is, it isn't armor-specific:
```
python3 tools/generate_armor_sheet.py request --ref <any reference png> \
  --prompt "<see prompt template below>" --out /tmp/raw_sheet.png
```
This fails with `HTTPError 402 Payment Required` if the `OPENROUTER_API_KEY`
in this environment is out of credits (happened 2026-08-18) - if so, use B.

**B. Gemini web UI via `mcp__claude-in-chrome__*` browser automation.** Load
the core tool set first (`ToolSearch` with
`select:mcp__claude-in-chrome__tabs_context_mcp,...navigate,...computer,...tabs_create_mcp,...tabs_close_mcp`),
navigate to `https://gemini.google.com/app`, click the model dropdown and
pick a real model (not "Flash-Lite" - it didn't reliably emit images in
testing; "Flash"/"3.6 Flash" worked), click the composer, type the prompt,
click send. **First click after a page transition often drops** (see the
repo's `CLAUDE.md` browser-automation notes) - if the message doesn't send,
just click send again, no need to retype. Wait ~15-25s per image (longer for
8-frame sheets), screenshot to check for the "Creating your image" spinner
vs. the finished render, then hover the image to reveal the download icon
(top-right of the image, 3-icon toolbar) and click it. Downloaded files land
in `~/Downloads/Gemini_Generated_Image_*.jpeg` - copy the newest one out
before it gets buried by the next download.

**Prompt template** (fill in the bracketed parts):
```
Generate one image: a game VFX sprite sheet, a single tall vertical strip
containing exactly [N] animation frames stacked directly on top of each other
(frame 1 at the very top, frame [N] at the very bottom), each frame
equal-sized and roughly square, with a thin visible gap between frames. Do
NOT draw any character, person, or body anywhere in the image - pure floating
special-effect art only. Background: flat solid pure [green (#00FF00) /
magenta (#FF00FF)], completely flat, no gradient, no shading, no
checkerboard, no texture.

The effect across the [N] frames: [concept description - shape, motion,
color]. If your background is green and the effect itself is green-toned,
explicitly call for a distinctly different/more saturated shade plus a
white/cyan glow core so it doesn't collide with the key color (see step 2).

Frame 1: [starting state, small/forming].
Frame 2-[N-1]: [expansion/peak progression, describe each beat explicitly -
vague phrasing like "slightly different" produces near-duplicate frames].
Frame [N]: [dissipation/fade].

Style: flat cel-shaded 2D game effect art, sharp clean edges, vivid saturated
colors, no photorealism, no 3D render look.
```

## 2. Pick the key color: green vs. magenta

Same rule as the icon pipeline (`CLAUDE.md`): default to **green**
(`#00FF00`); switch to **magenta** (`#FF00FF`) only when the effect itself is
green-dominant (e.g. a leaf/wind vortex) - confirmed working for
`LeafHurricaneBlastProjectile`. Don't use magenta for a red/warm-dominant
effect (magenta shares hue with red and will eat into it).

## 3. Slice with `tools/slice_fx_sheet.py`

```
python3 tools/slice_fx_sheet.py --sheet /tmp/raw_sheet.jpeg --rows [N] \
  --bg [green|magenta] --frame-w [W] --frame-h [H] \
  --out Content/Projectiles/Bursts/<Name>BlastProjectile.png \
  --review-dir /tmp/<name>_review
```

This generalizes `generate_armor_sheet.py`'s per-cell key/trim/center logic
from "N columns in one row" to "N rows in one column" (the vertical-strip
convention every animated Projectile/Burst texture in this mod uses -
`frame_W x (frame_H * N)`, center-grounded, not foot-grounded).

**Known gotcha, already fixed in the tool but worth knowing**: nano-banana's
row-divider lines (from the "thin visible gap between frames" prompt
instruction) sometimes render as a washed-out, JPEG-blended near-white-green
rather than pure background color - e.g. `(204, 255, 205)`, which fails the
hue-dominance key (`g > r*1.3` needs `255 > 265`, false) and survives as a
stray line baked into a frame's top/bottom edge (hit on
`FrontLotusBlastProjectile`, 2026-08-18). The slicer now insets `ROW_MARGIN`
(10px default, `--row-margin` to override) off each row's top/bottom *before*
keying, which removes the divider band outright. If a similar artifact shows
up somewhere else (not at a row edge), don't widen the color-key fuzz to
compensate - that risks eating real white-hot VFX core content instead.

**Always verify visually before wiring into code**:
```
python3 -c "from PIL import Image; im=Image.open('<out.png>'); print(im.size, im.getbbox())"
convert <out.png> -background '#333' -flatten /tmp/preview.png
```
Read `/tmp/preview.png` back and eyeball every frame for stray background
fringe, clipped content, or leftover divider lines before committing.

## 4. Wire it into code

Subclass `Common/VFX/PlayerAnchoredBurstEffectProjectile.cs` (not the older
`BurstEffectProjectile.cs`, which spawns at a *fixed* position and doesn't
track the player):

```csharp
public class <Name>BlastProjectile : PlayerAnchoredBurstEffectProjectile
{
    protected override int FrameCount => N;
    protected override int LifetimeTicks => <matches the weapon's useAnimation>;
    protected override int CanvasSize => <W, must equal the PNG's width>;
}
```

Spawn it from the weapon's `UseAnimation(Player player)`:
```csharp
ChakraVFX.SpawnPlayerAnchoredBurst<<Name>BlastProjectile>(player, new Vector2(offsetX, offsetY));
```
`offset` is in the player's un-flipped facing direction (+X = in front of
them, flipped automatically per-tick off the player's real direction) -
positive Y offsets push the effect toward the lower body, useful for kicks;
near-zero Y keeps it centered, useful for punches/torso effects.

## 5. Verify

`tools/build.sh` (must be 0 errors/warnings) then
`tools/smoke_test_server.sh` (headless boot, catches bad texture
paths/frame-count mismatches that a plain build can't). Both must pass clean
before considering the asset done - see the repo's build/verify convention.
