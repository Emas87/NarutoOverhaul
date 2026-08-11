#!/usr/bin/env python3
"""Generates a vanilla-template player-armor equip texture (40x56/frame) from a single
existing reference frame, using nano-banana (Gemini 2.5 Flash Image) for the actual art.

Why OpenRouter, not the Gemini API directly: this environment's GEMINI_API_KEY/NANOBANANA_API_KEY
has zero free-tier quota for every image-capable Gemini model (confirmed via direct API test -
"limit: 0", not just rate-limited). The exact same key works fine routed through OpenRouter's
`google/gemini-2.5-flash-image` model instead, so that's what this script uses (OPENROUTER_API_KEY).

REAL vanilla-compatible template shapes (ground-truthed 2026-08-04 by extracting and decoding a
real shipped armor piece - ThoriumMod's YewWoodBreastguard - directly out of ThoriumMod.tmod; see
ARMOR_REFERENCE_IMAGES.md "Template shape correction" section for the full extraction method).
This mod previously used an invented 20-col x 8-row (800x448) shape for every equip type, which is
WRONG and is the likely root cause of the "weird head" bug - tModLoader slices Head/Legs textures
as a single 40px-wide COLUMN, not a wide row, so a Head texture built on the wrong-shaped canvas
gets sliced into garbage frame regions no matter how good the art is:
  - Body            : 9 columns x 4 rows (360x224)
  - Arms / Head / Legs : 1 column x 20 rows (40x1120) - a TALL VERTICAL STRIP, not a grid
Only frame/row 0 needs unique art - modding convention (and what simpler real armors like
YewWoodBreastguard actually do) is to tile frame 0 into the remaining frames unless you want
per-frame pose variation, which this mod doesn't attempt yet.

CORRECTION 2026-08-04: Arms was originally assumed to share Body's (9, 4) grid shape, but that
was never actually verified - YewWoodBreastguard has no _Arms file at all. Extracting real
_Arms.rawimg files from CalamityMod (DemonshadeBreastplate, EmpyreanCloak, MeldTransformation -
all independently 40x1120) proved Arms actually matches Head/Legs' single-column shape instead.

Usage:
  tools/generate_armor_sheet.py request --ref Content/Items/Armor/TaijutsuBodyItem_Body.png \
      --prompt "..." --out /tmp/taijutsu_body_sheet.png

  tools/generate_armor_sheet.py build --sheet /tmp/taijutsu_body_sheet.png --cols 10 \
      --equip-type body --bg magenta \
      --columns 0,1,2,3,4,5,6,7,8,9 \
      --out Content/Items/Armor/TaijutsuBodyItem_Body.png \
      --review-dir /tmp/taijutsu_body_review

`request` calls nano-banana once and saves the raw labeled sheet - always inspect it manually
before slicing (nano-banana doesn't reliably hold a perfectly uniform grid; re-roll the prompt if
frames bleed into each other or drift in scale, per ANIMATION_PIPELINE.md's own notes on this).

`build` slices that sheet into individual frames (assumed to be one row of equal-width cells),
keys out the flat background color, trims + centers each frame into the 40x56 template cell, maps
them onto the requested --equip-type's frame indices (any frame not covered by --columns reuses
frame 0, i.e. the idle frame), and tiles frame 0 across the rest of that equip type's real shape
(grid for Body/Arms, vertical strip for Head/Legs) to produce the final ready-to-use equip
texture. Always eyeball the result in-game before committing - this is a mechanical assembly step,
not a substitute for reviewing the art.
"""
import argparse
import base64
import json
import os
import sys
import urllib.request
from pathlib import Path

from PIL import Image

REPO = Path(__file__).resolve().parent.parent

CELL_W = 40
CELL_H = 56

# (columns, rows) per equip type - see module docstring for how these were ground-truthed.
# NOTE: "arms" was corrected 2026-08-04 from an assumed (9, 4) (extrapolated from Body, never
# actually verified) to the real (1, 20) - ground-truthed by extracting real _Arms.rawimg files
# from CalamityMod (DemonshadeBreastplate_Arms, EmpyreanCloak_Arms, MeldTransformation_Arms - all
# three are 40x1120, matching Head/Legs' vertical-strip shape, not Body's grid).
EQUIP_TEMPLATES = {
    "body": (9, 4),
    "arms": (1, 20),
    "head": (1, 20),
    "legs": (1, 20),
}

BG_PRESETS = {
    "magenta": (255, 0, 255),
    "green": (0, 255, 0),
}


def request_sheet(ref_paths, prompt, out_path, model="google/gemini-2.5-flash-image"):
    api_key = os.environ.get("OPENROUTER_API_KEY")
    if not api_key:
        sys.exit("OPENROUTER_API_KEY is not set")

    content = [{"type": "text", "text": prompt}]
    for ref_path in ref_paths:
        data = base64.b64encode(Path(ref_path).read_bytes()).decode()
        content.append({"type": "image_url", "image_url": {"url": f"data:image/png;base64,{data}"}})

    payload = {
        "model": model,
        "messages": [{"role": "user", "content": content}],
        "modalities": ["image", "text"],
    }
    req = urllib.request.Request(
        "https://openrouter.ai/api/v1/chat/completions",
        data=json.dumps(payload).encode(),
        headers={"Authorization": f"Bearer {api_key}", "Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=120) as resp:
        data = json.load(resp)

    message = data["choices"][0]["message"]
    images = message.get("images")
    if not images:
        sys.exit(f"No image returned. Model said: {message.get('content')!r}")

    url = images[0]["image_url"]["url"]
    header, b64data = url.split(",", 1)
    raw = base64.b64decode(b64data)
    Path(out_path).write_bytes(raw)
    print(f"Wrote {out_path} ({len(raw)} bytes) - inspect it before running `build`.")


def _is_background(px, bg_name, bg_rgb, fuzz=40):
    r, g, b = px[0], px[1], px[2]
    # Nano-banana's "flat background" is rarely one exact RGB value edge to edge (shading/
    # dividers still show through) - keying on hue dominance (like convert_gemini_sprite.sh's
    # spill-cleanup pass) catches that; fall back to plain color distance for a custom --bg rgb.
    # Arms-style sheets also draw the "empty gap" between sleeves as a light gray checkerboard
    # placeholder (near-grayscale, ~180-240 per channel) instead of true transparency - key that
    # out too so it doesn't get baked in as a solid gray torso-shaped blob.
    is_checkerboard_gray = (
        max(r, g, b) - min(r, g, b) <= 12 and 150 <= r <= 250
    )
    if is_checkerboard_gray:
        return True
    if bg_name == "magenta":
        return r > g * 1.3 and b > g * 1.3
    if bg_name == "green":
        return g > r * 1.3 and g > b * 1.3
    return all(abs(px[i] - bg_rgb[i]) <= fuzz for i in range(3))


def _key_and_trim(cell, bg_name, bg_rgb):
    """Key out the flat background color -> transparency, then crop to the remaining content's
    bounding box (mirrors the fuzzy color-key + auto-trim used by tools/prepare_sprite.sh, done in
    Pillow here so it can run per-cell without shelling out per frame)."""
    cell = cell.convert("RGBA")
    px = cell.load()
    w, h = cell.size
    for y in range(h):
        for x in range(w):
            if _is_background(px[x, y], bg_name, bg_rgb):
                px[x, y] = (0, 0, 0, 0)

    bbox = cell.getbbox()
    return cell.crop(bbox) if bbox else cell



# How content is anchored vertically within its 40x56 cell. Ground-truthed 2026-08-04 against
# ThoriumMod's real YewWoodHelmet_Head.png: frame 0's content bbox was y=10..30 within the 56px
# cell (top-anchored with a small margin), NOT foot-grounded at the bottom. Foot-grounding is
# correct for Body/Arms/Legs (aligns with where the character's feet/waist sit in-frame), but
# applying it to Head put the headband art down at body-height in-game instead of on the head -
# a real, confirmed in-game bug, not just a style nitpick.
HEAD_TOP_MARGIN = 10


def _center_on_canvas(frame, canvas_w, canvas_h, anchor="bottom"):
    canvas = Image.new("RGBA", (canvas_w, canvas_h), (0, 0, 0, 0))
    scale = min(canvas_w / frame.width, canvas_h / frame.height, 1.0)
    if scale < 1.0:
        frame = frame.resize((max(1, int(frame.width * scale)), max(1, int(frame.height * scale))), Image.NEAREST)
    x = (canvas_w - frame.width) // 2
    if anchor == "top":
        y = HEAD_TOP_MARGIN
    else:
        y = canvas_h - frame.height  # foot-ground the sprite at the bottom of the cell
    canvas.paste(frame, (x, y), frame)
    return canvas


def slice_sheet(sheet_path, cols, cell_w, cell_h, bg_name, bg_rgb, anchor="bottom"):
    sheet = Image.open(sheet_path).convert("RGBA")
    src_cell_w = sheet.width // cols
    frames = []
    for i in range(cols):
        raw_cell = sheet.crop((i * src_cell_w, 0, (i + 1) * src_cell_w, sheet.height))
        content = _key_and_trim(raw_cell, bg_name, bg_rgb)
        frames.append(_center_on_canvas(content, cell_w, cell_h, anchor))
    return frames


def assemble_template(frames_by_index, template_cols, template_rows):
    """frames_by_index: dict[int, Image] for whichever of the template_cols*template_rows frame
    slots were generated (index = row * template_cols + col, row-major). Any index missing from
    the dict reuses index 0 (idle) - see module docstring. For Head/Legs, template_cols is 1, so
    this naturally produces the real vertical-strip shape instead of a grid."""
    if 0 not in frames_by_index:
        sys.exit("Frame 0 (idle) must always be provided - every other frame falls back to it.")

    sheet = Image.new("RGBA", (CELL_W * template_cols, CELL_H * template_rows), (0, 0, 0, 0))
    for index in range(template_cols * template_rows):
        frame = frames_by_index.get(index, frames_by_index[0])
        row, col = divmod(index, template_cols)
        sheet.paste(frame, (col * CELL_W, row * CELL_H), frame)
    return sheet


def build(args):
    if args.equip_type not in EQUIP_TEMPLATES:
        sys.exit(f"--equip-type must be one of {sorted(EQUIP_TEMPLATES)}")
    template_cols, template_rows = EQUIP_TEMPLATES[args.equip_type]

    bg_rgb = BG_PRESETS[args.bg] if args.bg in BG_PRESETS else tuple(int(c) for c in args.bg.split(","))
    columns = [int(c) for c in args.columns.split(",")]
    anchor = "top" if args.equip_type == "head" else "bottom"
    frames = slice_sheet(args.sheet, args.cols, args.cell_w, args.cell_h, args.bg, bg_rgb, anchor)
    if len(frames) != len(columns):
        sys.exit(f"--cols {args.cols} produced {len(frames)} frames but --columns lists {len(columns)} targets")

    max_index = template_cols * template_rows - 1
    if any(c > max_index for c in columns):
        sys.exit(f"--equip-type {args.equip_type} only has frame indices 0-{max_index} ({template_cols}x{template_rows}), got {columns}")

    if args.review_dir:
        review_dir = Path(args.review_dir)
        review_dir.mkdir(parents=True, exist_ok=True)
        for col, frame in zip(columns, frames):
            frame.save(review_dir / f"col_{col:02d}.png")
        print(f"Wrote per-frame review crops to {review_dir} - check these before trusting the final sheet.")

    frames_by_index = dict(zip(columns, frames))
    final = assemble_template(frames_by_index, template_cols, template_rows)
    Path(args.out).parent.mkdir(parents=True, exist_ok=True)
    final.save(args.out)
    print(f"Wrote {args.out} ({final.width}x{final.height})")


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="command", required=True)

    req = sub.add_parser("request", help="Call nano-banana once to generate a labeled sheet from a reference frame")
    req.add_argument("--ref", action="append", required=True, help="Reference image(s) - repeatable")
    req.add_argument("--prompt", required=True)
    req.add_argument("--out", required=True)
    req.add_argument("--model", default="google/gemini-2.5-flash-image")

    bld = sub.add_parser("build", help="Slice a generated sheet and assemble the final vanilla-template texture")
    bld.add_argument("--sheet", required=True)
    bld.add_argument("--equip-type", required=True, choices=sorted(EQUIP_TEMPLATES), help="Determines the real output template shape (see module docstring)")
    bld.add_argument("--cols", type=int, required=True, help="Number of equal-width cells in the raw sheet")
    bld.add_argument("--columns", required=True, help="Comma-separated frame indices the sheet's cells map to (index = row*template_cols+col), e.g. 0,1,2,3,4,5,6,7,8,9")
    bld.add_argument("--cell-w", type=int, default=CELL_W)
    bld.add_argument("--cell-h", type=int, default=CELL_H)
    bld.add_argument("--bg", default="magenta", help="'magenta', 'green', or 'r,g,b'")
    bld.add_argument("--out", required=True)
    bld.add_argument("--review-dir", default=None, help="Optional dir to dump per-frame crops for manual review")

    args = ap.parse_args()
    if args.command == "request":
        request_sheet(args.ref, args.prompt, args.out, args.model)
    elif args.command == "build":
        build(args)


if __name__ == "__main__":
    main()
