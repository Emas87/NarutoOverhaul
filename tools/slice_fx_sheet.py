#!/usr/bin/env python3
"""Slices a nano-banana-generated animation sheet into this mod's vertical-strip VFX convention
(frame_W x (frame_H * N), one frame per row, center-grounded) - the format every animated
Projectile/Burst texture in Content/Projectiles/ already uses (see ANIMATION_PIPELINE.md).

Generalizes generate_armor_sheet.py's per-cell key/trim/resize logic (same fuzzy background-color
keying, same Pillow-based trim-to-bbox) from "N columns in one row" (armor equip sheets) to "N rows
in one column" (VFX animation strips) - this mod's own committed `process_fx.py`-equivalent never
made it into the repo (see CLAUDE.md/ANIMATION_PIPELINE.md), so this reconstructs the minimum of it
needed to land new VFX art.

Usage:
  tools/slice_fx_sheet.py --sheet /tmp/iron_leg_blast_sheet.png --rows 6 --bg green \
      --frame-w 150 --frame-h 150 \
      --out Content/Projectiles/Bursts/IronLegBlastProjectile.png \
      --review-dir /tmp/iron_leg_blast_review

Each row of the raw sheet is independently fuzzy-color-keyed to transparency, trimmed to its
content's bounding box, and re-centered (not foot-grounded - these are floating VFX, not
character art) into a fixed --frame-w x --frame-h cell; all N cells are then stacked into one
frame_w x (frame_h * rows) output PNG.
"""
import argparse
import sys
from pathlib import Path

from PIL import Image

BG_PRESETS = {
    "magenta": (255, 0, 255),
    "green": (0, 255, 0),
}


def _is_background(px, bg_name, bg_rgb, fuzz=40):
    r, g, b = px[0], px[1], px[2]
    # Same hue-dominance keying as generate_armor_sheet.py._is_background - nano-banana's "flat"
    # background is rarely one exact RGB value edge to edge (JPEG-style noise, shading, faint
    # dividers), so keying on hue dominance catches that better than a flat color-distance check.
    if bg_name == "magenta":
        return r > g * 1.3 and b > g * 1.3
    if bg_name == "green":
        return g > r * 1.3 and g > b * 1.3
    return all(abs(px[i] - bg_rgb[i]) <= fuzz for i in range(3))


def _key_and_trim(cell, bg_name, bg_rgb):
    cell = cell.convert("RGBA")
    px = cell.load()
    w, h = cell.size
    for y in range(h):
        for x in range(w):
            if _is_background(px[x, y], bg_name, bg_rgb):
                px[x, y] = (0, 0, 0, 0)
    bbox = cell.getbbox()
    return cell.crop(bbox) if bbox else cell


def _center_on_canvas(frame, canvas_w, canvas_h):
    # Center-grounded, not foot-grounded - these are floating energy-blast VFX with no "feet"
    # concept, same convention as the mod's other Projectile/Burst textures.
    canvas = Image.new("RGBA", (canvas_w, canvas_h), (0, 0, 0, 0))
    scale = min(canvas_w / frame.width, canvas_h / frame.height, 1.0)
    if scale < 1.0:
        frame = frame.resize((max(1, int(frame.width * scale)), max(1, int(frame.height * scale))), Image.NEAREST)
    x = (canvas_w - frame.width) // 2
    y = (canvas_h - frame.height) // 2
    canvas.paste(frame, (x, y), frame)
    return canvas


# Row-boundary inset applied before keying, in pixels of the raw sheet. nano-banana's row-divider
# lines (drawn to satisfy the "thin visible gap between frames" prompt instruction) sometimes
# render as a washed-out, JPEG-blended light green rather than pure background green - e.g.
# (204, 255, 205), which the hue-dominance key in _is_background (g > r*1.3) doesn't catch, since
# 255 is not > 204*1.3. That sliver then survives _key_and_trim and lands baked into the exported
# frame's edge (seen as a stray near-white line at the very top/bottom of a frame - Front Lotus
# 2026-08-18). Trimming a small margin off each row before keying removes the divider band outright
# instead of trying to widen the color key (which risks eating real white-hot VFX core content).
ROW_MARGIN = 10


def slice_sheet(sheet_path, rows, frame_w, frame_h, bg_name, bg_rgb, row_margin=ROW_MARGIN):
    sheet = Image.open(sheet_path).convert("RGBA")
    src_row_h = sheet.height // rows
    if row_margin * 2 >= src_row_h:
        raise ValueError(
            f"--row-margin {row_margin} is too large for this sheet: each of the {rows} rows "
            f"is only {src_row_h}px tall ({sheet.height}px / {rows} rows), so trimming "
            f"{row_margin}px off both the top and bottom would leave nothing (or a negative "
            f"crop). Pass a smaller --row-margin, or check --rows matches the actual sheet."
        )
    frames = []
    for i in range(rows):
        y0 = i * src_row_h + row_margin
        y1 = (i + 1) * src_row_h - row_margin
        raw_row = sheet.crop((0, y0, sheet.width, y1))
        content = _key_and_trim(raw_row, bg_name, bg_rgb)
        frames.append(_center_on_canvas(content, frame_w, frame_h))
    return frames


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--sheet", required=True)
    ap.add_argument("--rows", type=int, required=True, help="Number of equal-height frame rows in the raw sheet")
    ap.add_argument("--frame-w", type=int, required=True)
    ap.add_argument("--frame-h", type=int, required=True)
    ap.add_argument("--bg", default="green", help="'magenta', 'green', or 'r,g,b'")
    ap.add_argument("--out", required=True)
    ap.add_argument("--review-dir", default=None, help="Optional dir to dump per-frame crops for manual review")
    ap.add_argument("--row-margin", type=int, default=ROW_MARGIN, help="Pixels trimmed off each raw row's top/bottom before keying, to cut out divider-line JPEG-blend artifacts (see ROW_MARGIN comment)")
    args = ap.parse_args()

    bg_rgb = BG_PRESETS[args.bg] if args.bg in BG_PRESETS else tuple(int(c) for c in args.bg.split(","))
    try:
        frames = slice_sheet(args.sheet, args.rows, args.frame_w, args.frame_h, args.bg, bg_rgb, args.row_margin)
    except ValueError as e:
        print(f"error: {e}", file=sys.stderr)
        sys.exit(1)

    if args.review_dir:
        review_dir = Path(args.review_dir)
        review_dir.mkdir(parents=True, exist_ok=True)
        for i, frame in enumerate(frames):
            frame.save(review_dir / f"frame_{i:02d}.png")
        print(f"Wrote per-frame review crops to {review_dir} - check these before trusting the final sheet.")

    final = Image.new("RGBA", (args.frame_w, args.frame_h * args.rows), (0, 0, 0, 0))
    for i, frame in enumerate(frames):
        final.paste(frame, (0, i * args.frame_h), frame)

    Path(args.out).parent.mkdir(parents=True, exist_ok=True)
    final.save(args.out)
    print(f"Wrote {args.out} ({final.width}x{final.height})")


if __name__ == "__main__":
    main()
