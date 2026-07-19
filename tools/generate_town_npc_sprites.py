#!/usr/bin/env python3
# Procedurally generates the 25-frame (40x56 each, 40x1400 sheet) Town NPC animation sheets.
#
# Why procedural 2D instead of the 3D pipeline in ANIMATION_PIPELINE.md: no Rodin/Mixamo/Meshy
# access in this environment and no character meshes exist, so the sheets are drawn as flat
# pixel-art rigs instead. All inter-frame deltas are kept <=3px on purpose: vanilla's exact
# Guide frame-slot semantics (which of the 25 frames is idle/walk/talk) can't be extracted
# here (LZX-compressed XNB), so every frame must read as a plausible idle variant even if it
# plays in the "wrong" state.
#
# Usage:
#   tools/generate_town_npc_sprites.py                 # regenerate all 4 sheets in place
#   tools/generate_town_npc_sprites.py --character Kakashi --contact-sheet
import argparse
from dataclasses import dataclass
from pathlib import Path

from PIL import Image, ImageDraw

CANVAS_W, CANVAS_H = 40, 56
FRAME_COUNT = 25
REPO = Path(__file__).resolve().parent.parent
OUT_DIR = REPO / "Content" / "NPCs" / "Town"

OUTLINE = (25, 22, 28, 255)


@dataclass
class Palette:
    hair: tuple
    hair_shade: tuple
    skin: tuple
    face_accent: tuple      # mask / tear-marks / blush
    outfit_primary: tuple   # torso main
    outfit_secondary: tuple # sleeves / under-layer
    outfit_trim: tuple
    pants: tuple
    metal: tuple = (170, 175, 185, 255)


@dataclass
class Character:
    name: str
    palette: Palette
    hairstyle: str      # spiky | tied_back | bowlcut | twin_buns
    outfit_shape: str   # flak_jacket | high_collar_cloak | jumpsuit | vendor_vest
    has_mask: bool = False
    has_forehead_protector: bool = True
    brow_thickness: int = 1


CHARACTERS = {
    "Kakashi": Character(
        "KakashiNPC",
        Palette(
            hair=(208, 208, 214, 255), hair_shade=(160, 160, 172, 255),
            skin=(233, 195, 160, 255), face_accent=(58, 72, 84, 255),
            outfit_primary=(76, 110, 72, 255), outfit_secondary=(42, 56, 78, 255),
            outfit_trim=(96, 134, 90, 255), pants=(42, 56, 78, 255),
        ),
        hairstyle="spiky", outfit_shape="flak_jacket", has_mask=True,
    ),
    "Itachi": Character(
        "ItachiNPC",
        Palette(
            hair=(38, 34, 44, 255), hair_shade=(24, 22, 30, 255),
            skin=(226, 188, 156, 255), face_accent=(150, 120, 104, 255),
            outfit_primary=(30, 28, 36, 255), outfit_secondary=(30, 28, 36, 255),
            outfit_trim=(168, 44, 52, 255), pants=(30, 28, 36, 255),
        ),
        hairstyle="tied_back", outfit_shape="high_collar_cloak",
        has_forehead_protector=True,
    ),
    "GuySensei": Character(
        "GuySenseiNPC",
        Palette(
            hair=(30, 28, 32, 255), hair_shade=(20, 18, 24, 255),
            skin=(224, 178, 140, 255), face_accent=(30, 28, 32, 255),
            outfit_primary=(52, 122, 62, 255), outfit_secondary=(38, 96, 50, 255),
            outfit_trim=(214, 110, 40, 255), pants=(38, 96, 50, 255),
        ),
        hairstyle="bowlcut", outfit_shape="jumpsuit", brow_thickness=2,
    ),
    "ShinobiVendor": Character(
        "ShinobiVendorNPC",
        Palette(
            hair=(74, 48, 36, 255), hair_shade=(56, 36, 28, 255),
            skin=(236, 198, 164, 255), face_accent=(200, 120, 110, 255),
            outfit_primary=(178, 62, 66, 255), outfit_secondary=(232, 224, 208, 255),
            outfit_trim=(120, 84, 60, 255), pants=(64, 72, 88, 255),
        ),
        hairstyle="twin_buns", outfit_shape="vendor_vest",
        has_forehead_protector=False,
    ),
}

# Layout constants (y coords, from top of 56px canvas; character occupies most of it).
HEAD_TOP = 4
HEAD_H = 14
NECK_Y = HEAD_TOP + HEAD_H          # 18
TORSO_H = 16
LEGS_Y = NECK_Y + TORSO_H           # 34
LEGS_H = 18                          # feet end at 52, leaving ground margin
CX = CANVAS_W // 2                   # 20


def r(d, x0, y0, x1, y1, color):
    d.rectangle([x0, y0, x1, y1], fill=color)


def draw_legs(d, pal, leg_l, leg_r, bob):
    y0 = LEGS_Y + bob
    y1 = y0 + LEGS_H - 1
    # left leg (viewer left), shifted horizontally by leg offset
    r(d, CX - 6 + leg_l, y0, CX - 2 + leg_l, y1, pal.pants)
    r(d, CX + 1 + leg_r, y0, CX + 5 + leg_r, y1, pal.pants)
    # sandals/feet
    foot = (60, 52, 46, 255)
    r(d, CX - 7 + leg_l, y1 - 2, CX - 1 + leg_l, y1, foot)
    r(d, CX + 0 + leg_r, y1 - 2, CX + 6 + leg_r, y1, foot)


def draw_torso(d, ch, bob):
    pal = ch.palette
    y0 = NECK_Y + bob
    y1 = y0 + TORSO_H
    if ch.outfit_shape == "flak_jacket":
        r(d, CX - 7, y0, CX + 7, y1, pal.outfit_secondary)   # undersuit
        r(d, CX - 6, y0 + 2, CX + 6, y1, pal.outfit_primary) # jacket
        r(d, CX - 2, y0 + 2, CX + 2, y1, pal.outfit_trim)    # zipper panel
        r(d, CX - 6, y0 + 4, CX - 4, y0 + 7, pal.outfit_trim)  # pocket
        r(d, CX + 4, y0 + 4, CX + 6, y0 + 7, pal.outfit_trim)
    elif ch.outfit_shape == "high_collar_cloak":
        r(d, CX - 8, y0 - 2, CX + 8, y1 + 6, pal.outfit_primary)  # long cloak overlaps legs
        r(d, CX - 8, y0 - 2, CX + 8, y0, pal.outfit_trim)          # collar trim
        # red cloud motif
        r(d, CX - 5, y0 + 5, CX - 2, y0 + 7, pal.outfit_trim)
        r(d, CX + 2, y0 + 9, CX + 5, y0 + 11, pal.outfit_trim)
    elif ch.outfit_shape == "jumpsuit":
        r(d, CX - 7, y0, CX + 7, y1, pal.outfit_primary)
        r(d, CX - 7, y0 + 6, CX + 7, y0 + 7, pal.outfit_secondary)  # belt line
        r(d, CX - 7, y0, CX - 5, y1, pal.outfit_secondary)          # side shading
    else:  # vendor_vest
        r(d, CX - 7, y0, CX + 7, y1, pal.outfit_secondary)   # blouse
        r(d, CX - 7, y0 + 1, CX - 3, y1, pal.outfit_primary) # vest halves
        r(d, CX + 3, y0 + 1, CX + 7, y1, pal.outfit_primary)
        r(d, CX - 7, y0 + 10, CX + 7, y0 + 11, pal.outfit_trim)  # belt


def draw_arms(d, ch, arm_l, arm_r, bob):
    pal = ch.palette
    y0 = NECK_Y + 2 + bob
    sleeve = pal.outfit_primary if ch.outfit_shape in ("high_collar_cloak", "jumpsuit") else pal.outfit_secondary
    # arm_l/arm_r shift the hands vertically (swing); arms hang at torso sides
    r(d, CX - 10, y0 + arm_l, CX - 8, y0 + 11 + arm_l, sleeve)
    r(d, CX + 8, y0 + arm_r, CX + 10, y0 + 11 + arm_r, sleeve)
    # hands
    r(d, CX - 10, y0 + 11 + arm_l, CX - 8, y0 + 12 + arm_l, pal.skin)
    r(d, CX + 8, y0 + 11 + arm_r, CX + 10, y0 + 12 + arm_r, pal.skin)


def draw_head(d, ch, tilt, bob):
    pal = ch.palette
    x0 = CX - 7 + tilt
    y0 = HEAD_TOP + bob
    r(d, x0, y0 + 3, x0 + 14, y0 + HEAD_H, pal.skin)


def draw_hair(d, ch, tilt, bob):
    pal = ch.palette
    x0 = CX - 7 + tilt
    y0 = HEAD_TOP + bob
    if ch.hairstyle == "spiky":
        r(d, x0 - 1, y0, x0 + 15, y0 + 5, pal.hair)
        # spikes up
        for sx in (x0 + 1, x0 + 5, x0 + 9, x0 + 13):
            r(d, sx, y0 - 3, sx + 1, y0, pal.hair)
        r(d, x0 - 1, y0 + 5, x0 + 1, y0 + 9, pal.hair_shade)  # side tuft
    elif ch.hairstyle == "tied_back":
        r(d, x0, y0, x0 + 14, y0 + 4, pal.hair)
        r(d, x0 - 1, y0 + 2, x0 + 1, y0 + 10, pal.hair_shade)   # framing strands
        r(d, x0 + 13, y0 + 2, x0 + 15, y0 + 10, pal.hair_shade)
        r(d, x0 + 15, y0 + 6, x0 + 16, y0 + 16, pal.hair)       # ponytail behind
    elif ch.hairstyle == "bowlcut":
        r(d, x0 - 1, y0, x0 + 15, y0 + 6, pal.hair)
        r(d, x0 - 1, y0 + 6, x0, y0 + 8, pal.hair)
        r(d, x0 + 14, y0 + 6, x0 + 15, y0 + 8, pal.hair)
    else:  # twin_buns
        r(d, x0, y0 + 1, x0 + 14, y0 + 5, pal.hair)
        r(d, x0 - 3, y0 - 1, x0 + 1, y0 + 3, pal.hair)     # left bun
        r(d, x0 + 13, y0 - 1, x0 + 17, y0 + 3, pal.hair)   # right bun
        r(d, x0 - 2, y0, x0, y0 + 2, pal.hair_shade)
        r(d, x0 + 14, y0, x0 + 16, y0 + 2, pal.hair_shade)


def draw_face(d, ch, tilt, bob, blink):
    pal = ch.palette
    x0 = CX - 7 + tilt
    y0 = HEAD_TOP + bob
    eye_y = y0 + 7
    eye = (30, 30, 36, 255)
    if blink:
        r(d, x0 + 3, eye_y + 1, x0 + 5, eye_y + 1, eye)
        r(d, x0 + 9, eye_y + 1, x0 + 11, eye_y + 1, eye)
    else:
        r(d, x0 + 3, eye_y, x0 + 5, eye_y + 2, (250, 250, 250, 255))
        r(d, x0 + 4, eye_y, x0 + 5, eye_y + 2, eye)
        r(d, x0 + 9, eye_y, x0 + 11, eye_y + 2, (250, 250, 250, 255))
        r(d, x0 + 10, eye_y, x0 + 11, eye_y + 2, eye)
    # brows
    bt = ch.brow_thickness
    r(d, x0 + 3, eye_y - 1 - bt, x0 + 5, eye_y - 1, ch.palette.hair)
    r(d, x0 + 9, eye_y - 1 - bt, x0 + 11, eye_y - 1, ch.palette.hair)
    if ch.has_mask:
        r(d, x0, eye_y + 3, x0 + 14, y0 + HEAD_H, pal.face_accent)
    elif ch.name == "ItachiNPC":
        # tear-line marks
        r(d, x0 + 4, eye_y + 3, x0 + 4, eye_y + 5, pal.face_accent)
        r(d, x0 + 10, eye_y + 3, x0 + 10, eye_y + 5, pal.face_accent)
    else:
        mouth_y = y0 + HEAD_H - 2
        r(d, x0 + 6, mouth_y, x0 + 8, mouth_y, (150, 90, 80, 255))


def draw_accessory(d, ch, tilt, bob):
    pal = ch.palette
    x0 = CX - 7 + tilt
    y0 = HEAD_TOP + bob
    if ch.has_forehead_protector:
        band_y = y0 + 4
        r(d, x0 - 1, band_y, x0 + 15, band_y + 2, (40, 48, 66, 255))
        r(d, x0 + 4, band_y, x0 + 10, band_y + 2, pal.metal)
        if ch.name == "KakashiNPC":
            # tilted over left eye
            r(d, x0 + 1, band_y, x0 + 6, band_y + 4, (40, 48, 66, 255))
            r(d, x0 + 2, band_y + 1, x0 + 5, band_y + 3, pal.metal)


def outline_pass(img):
    """1px dark outline around every opaque region (hard-edged, flood from alpha)."""
    px = img.load()
    w, h = img.size
    to_outline = []
    for y in range(h):
        for x in range(w):
            if px[x, y][3] == 0:
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < w and 0 <= ny < h and px[nx, ny][3] != 0 and px[nx, ny] != OUTLINE:
                        to_outline.append((x, y))
                        break
    for x, y in to_outline:
        px[x, y] = OUTLINE
    return img


def render_frame(ch, bob=0, leg_l=0, leg_r=0, arm_l=0, arm_r=0, tilt=0, blink=False):
    img = Image.new("RGBA", (CANVAS_W, CANVAS_H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    draw_legs(d, ch.palette, leg_l, leg_r, bob)
    draw_arms(d, ch, arm_l, arm_r, bob)
    draw_torso(d, ch, bob)
    draw_head(d, ch, tilt, bob)
    draw_face(d, ch, tilt, bob, blink)
    draw_hair(d, ch, tilt, bob)
    draw_accessory(d, ch, tilt, bob)
    return outline_pass(img)


def build_frames(ch):
    frames = []
    # 0-5: breathing bob
    for bob in (0, -1, -1, 0, 0, 1):
        frames.append(render_frame(ch, bob=bob))
    # 6-7: blink
    for _ in range(2):
        frames.append(render_frame(ch, blink=True))
    # 8-19: walk cycle - triangle wave on legs, opposite-phase arm swing, synced bob
    walk = [0, 1, 2, 1, 0, -1, -2, -1, 0, 1, 2, 1]
    for i, off in enumerate(walk):
        bob = -1 if abs(off) == 2 else 0
        frames.append(render_frame(
            ch, bob=bob,
            leg_l=off, leg_r=-off,
            arm_l=-off // 2, arm_r=off // 2,
        ))
    # 20-24: talk/gesture
    frames.append(render_frame(ch, tilt=1))
    frames.append(render_frame(ch, tilt=-1))
    frames.append(render_frame(ch, arm_r=-3))
    frames.append(render_frame(ch, arm_r=-2, tilt=1))
    frames.append(render_frame(ch))
    assert len(frames) == FRAME_COUNT
    return frames


def assemble_sheet(frames):
    sheet = Image.new("RGBA", (CANVAS_W, CANVAS_H * FRAME_COUNT), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        sheet.paste(f, (0, i * CANVAS_H))
    return sheet


def contact_sheet(frames, path):
    cols, rows, pad = 5, 5, 4
    cs = Image.new("RGBA", (cols * (CANVAS_W + pad) + pad, rows * (CANVAS_H + pad) + pad),
                   (90, 90, 100, 255))
    for i, f in enumerate(frames):
        x = pad + (i % cols) * (CANVAS_W + pad)
        y = pad + (i // cols) * (CANVAS_H + pad)
        cs.paste(f, (x, y), f)
    cs = cs.resize((cs.width * 3, cs.height * 3), Image.NEAREST)
    cs.save(path)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--character", action="append", choices=sorted(CHARACTERS))
    ap.add_argument("--contact-sheet", action="store_true")
    ap.add_argument("--out-dir", type=Path, default=OUT_DIR)
    args = ap.parse_args()

    names = args.character or sorted(CHARACTERS)
    for key in names:
        ch = CHARACTERS[key]
        frames = build_frames(ch)
        out = args.out_dir / f"{ch.name}.png"
        assemble_sheet(frames).save(out)
        print(f"Wrote {out} ({CANVAS_W}x{CANVAS_H * FRAME_COUNT})")
        if args.contact_sheet:
            cs_path = args.out_dir / f"{ch.name}_contact.png"
            contact_sheet(frames, cs_path)
            print(f"Wrote {cs_path} (preview only - do not commit)")


if __name__ == "__main__":
    main()
