#!/usr/bin/env python3
# Procedurally generates boss animation sheets (idle + 1-2 attack poses per boss).
#
# Same rationale as tools/generate_town_npc_sprites.py: no Rodin/Mixamo/Blender-rig access in
# this environment, so sheets are drawn as flat pixel-art rigs instead of the 3D pipeline in
# ANIMATION_PIPELINE.md. Per-phase/per-element visual variety (the doc's "vary palette/aura tint"
# guidance) is intentionally NOT baked into extra frames here - it's applied at runtime via each
# boss's GetAlpha() override instead, keyed off the same NPC.ai phase/element fields the AI already
# uses. That avoids multiplying frame count by phase count (e.g. Kakuzu would need 5x the frames)
# for a purely cosmetic tint that GetAlpha gives for free.
#
# Usage:
#   tools/generate_boss_sprites.py                       # regenerate all 7 boss sheets in place
#   tools/generate_boss_sprites.py --character Haku --contact-sheet
import argparse
from dataclasses import dataclass, field
from pathlib import Path

from PIL import Image, ImageDraw

REPO = Path(__file__).resolve().parent.parent
OUT_DIR = REPO / "Content" / "NPCs" / "Bosses"

OUTLINE = (18, 16, 20, 255)
FRAMES_PER_BLOCK = 6


@dataclass
class Palette:
    hair: tuple
    hair_shade: tuple
    skin: tuple
    outfit_primary: tuple
    outfit_secondary: tuple
    outfit_trim: tuple
    eye: tuple = (30, 30, 36, 255)
    accent: tuple = (120, 200, 255, 255)  # chakra/glow accent at hands during cast


@dataclass
class BossChar:
    file_name: str
    canvas_w: int
    canvas_h: int
    palette: Palette
    outfit_shape: str   # kimono | flowing_robe | stitched_cloak | armored_cloak | purple_kimono | akatsuki_cloak
    hairstyle: str      # senbon_bun | horned_updo | slicked_back | wild_spikes | long_straight | spiky_orange
    blocks: list = field(default_factory=lambda: ["idle", "melee", "cast"])  # order = sheet order


BOSSES = {
    "Haku": BossChar(
        "HakuBoss", 40, 56,
        Palette(
            hair=(50, 46, 60, 255), hair_shade=(34, 30, 42, 255),
            skin=(238, 214, 196, 255),
            outfit_primary=(210, 228, 236, 255), outfit_secondary=(140, 176, 196, 255),
            outfit_trim=(90, 130, 156, 255), accent=(170, 225, 255, 255),
        ),
        outfit_shape="kimono", hairstyle="senbon_bun", blocks=["idle", "melee", "cast"],
    ),
    "Kaguya": BossChar(
        "KaguyaBoss", 46, 68,
        Palette(
            hair=(240, 240, 244, 255), hair_shade=(206, 200, 214, 255),
            skin=(244, 226, 214, 255),
            outfit_primary=(232, 226, 240, 255), outfit_secondary=(180, 160, 206, 255),
            outfit_trim=(140, 100, 170, 255), accent=(196, 140, 230, 255),
        ),
        outfit_shape="flowing_robe", hairstyle="horned_updo", blocks=["idle", "cast", "telegraph"],
    ),
    "Kakuzu": BossChar(
        "KakuzuBoss", 46, 58,
        Palette(
            hair=(30, 28, 26, 255), hair_shade=(20, 18, 18, 255),
            skin=(150, 168, 150, 255),
            outfit_primary=(58, 70, 56, 255), outfit_secondary=(40, 48, 40, 255),
            outfit_trim=(90, 60, 40, 255), accent=(220, 140, 60, 255),
        ),
        outfit_shape="stitched_cloak", hairstyle="slicked_back", blocks=["idle", "cast"],
    ),
    "Madara": BossChar(
        "MadaraBoss", 48, 64,
        Palette(
            hair=(24, 22, 26, 255), hair_shade=(14, 13, 16, 255),
            skin=(224, 190, 168, 255),
            outfit_primary=(32, 30, 34, 255), outfit_secondary=(48, 20, 24, 255),
            outfit_trim=(150, 30, 34, 255), eye=(180, 24, 30, 255), accent=(200, 60, 70, 255),
        ),
        outfit_shape="armored_cloak", hairstyle="wild_spikes", blocks=["idle", "melee", "cast"],
    ),
    "Orochimaru": BossChar(
        "OrochimaruBoss", 44, 60,
        Palette(
            hair=(20, 18, 22, 255), hair_shade=(12, 11, 14, 255),
            skin=(232, 214, 206, 255),
            outfit_primary=(240, 236, 226, 255), outfit_secondary=(96, 60, 120, 255),
            outfit_trim=(70, 40, 90, 255), eye=(210, 200, 40, 255), accent=(160, 90, 190, 255),
        ),
        outfit_shape="purple_kimono", hairstyle="long_straight", blocks=["idle", "melee", "cast"],
    ),
    "Pain": BossChar(
        "PainBoss", 44, 62,
        Palette(
            hair=(214, 110, 40, 255), hair_shade=(170, 84, 30, 255),
            skin=(226, 190, 176, 255),
            outfit_primary=(26, 24, 30, 255), outfit_secondary=(26, 24, 30, 255),
            outfit_trim=(176, 40, 46, 255), eye=(180, 140, 210, 255), accent=(190, 120, 220, 255),
        ),
        outfit_shape="akatsuki_cloak", hairstyle="spiky_orange", blocks=["idle", "cast"],
    ),
}

CX_FRACTION = 0.5


def r(d, x0, y0, x1, y1, color):
    d.rectangle([x0, y0, x1, y1], fill=color)


def outline_pass(img):
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


def draw_hair(d, ch, cx, head_x0, head_y0, head_w, lean):
    pal = ch.palette
    style = ch.hairstyle
    x0, y0 = head_x0, head_y0
    if style == "senbon_bun":
        r(d, x0 - 1, y0, x0 + head_w + 1, y0 + 5, pal.hair)
        r(d, cx - 2, y0 - 4, cx + 2, y0, pal.hair)  # top bun
    elif style == "horned_updo":
        r(d, x0, y0 - 2, x0 + head_w, y0 + 4, pal.hair)
        r(d, x0 - 3, y0 - 6, x0 + 1, y0, pal.hair_shade)   # left horn-bun
        r(d, x0 + head_w - 1, y0 - 6, x0 + head_w + 3, y0, pal.hair_shade)  # right horn-bun
    elif style == "slicked_back":
        r(d, x0, y0 - 1, x0 + head_w, y0 + 3, pal.hair)
    elif style == "wild_spikes":
        r(d, x0 - 2, y0, x0 + head_w + 2, y0 + 4, pal.hair)
        for sx in range(x0 - 2, x0 + head_w + 2, 3):
            r(d, sx, y0 - 4, sx + 2, y0 + 1, pal.hair)
    elif style == "long_straight":
        r(d, x0, y0, x0 + head_w, y0 + 4, pal.hair)
        r(d, x0 - 2, y0 + 2, x0, y0 + head_w + 10, pal.hair_shade)
        r(d, x0 + head_w, y0 + 2, x0 + head_w + 2, y0 + head_w + 10, pal.hair_shade)
    elif style == "spiky_orange":
        r(d, x0 - 1, y0, x0 + head_w + 1, y0 + 4, pal.hair)
        for sx in (x0, x0 + 4, x0 + 8, x0 + 12):
            r(d, sx, y0 - 3, sx + 1, y0, pal.hair)


def draw_torso(d, ch, cx, y0, y1, lean, cast):
    pal = ch.palette
    x0 = cx - 7 + lean
    x1 = cx + 7 + lean
    shape = ch.outfit_shape
    if shape == "kimono":
        r(d, x0, y0, x1, y1 + 4, pal.outfit_secondary)
        r(d, x0 + 1, y0, x1 - 1, y1, pal.outfit_primary)
        r(d, cx - 1 + lean, y0, cx + 1 + lean, y1, pal.outfit_trim)
    elif shape == "flowing_robe":
        r(d, x0 - 2, y0, x1 + 2, y1 + 8, pal.outfit_primary)
        r(d, x0 - 2, y0, x1 + 2, y0 + 3, pal.outfit_trim)
    elif shape == "stitched_cloak":
        r(d, x0 - 1, y0 - 2, x1 + 1, y1 + 3, pal.outfit_primary)
        for sy in range(y0 + 2, y1, 4):
            r(d, x0 - 1, sy, x1 + 1, sy, pal.outfit_trim)
    elif shape == "armored_cloak":
        r(d, x0, y0, x1, y1, pal.outfit_secondary)
        r(d, x0 + 1, y0 + 1, x1 - 1, y1 - 2, pal.outfit_primary)
        r(d, cx - 3 + lean, y0 + 3, cx + 3 + lean, y0 + 7, pal.outfit_trim)  # fan crest
    elif shape == "purple_kimono":
        r(d, x0, y0, x1, y1, pal.outfit_primary)
        r(d, x0, y0 + 6, x1, y0 + 8, pal.outfit_trim)  # rope belt
        r(d, cx - 1 + lean, y0, cx + 1 + lean, y0 + 6, pal.skin)  # open chest
    elif shape == "akatsuki_cloak":
        r(d, x0 - 1, y0, x1 + 1, y1 + 2, pal.outfit_primary)
        r(d, cx - 2 + lean, y0 + 3, cx + 1 + lean, y0 + 6, pal.outfit_trim)  # red cloud
        r(d, cx - 4 + lean, y0 + 9, cx - 1 + lean, y0 + 12, pal.outfit_trim)

    if cast:
        r(d, cx - 2 + lean, y0 - 3, cx + 2 + lean, y0 - 1, pal.accent)


def draw_head_and_face(d, ch, cx, y0, lean, roar):
    pal = ch.palette
    head_w = 12
    x0 = cx - head_w // 2 + lean
    r(d, x0, y0 + 2, x0 + head_w, y0 + 12, pal.skin)
    eye_y = y0 + 6
    mouth = (150, 60, 60, 255) if roar else (140, 100, 96, 255)
    if roar:
        r(d, x0 + 3, eye_y + 3, x0 + head_w - 3, eye_y + 6, mouth)
    else:
        r(d, x0 + 3, eye_y + 4, x0 + head_w - 3, eye_y + 4, mouth)
    r(d, x0 + 2, eye_y, x0 + 4, eye_y + 2, pal.eye)
    r(d, x0 + head_w - 4, eye_y, x0 + head_w - 2, eye_y + 2, pal.eye)
    draw_hair(d, ch, cx + lean, x0, y0, head_w, lean)
    return x0, y0, head_w


def draw_arms(d, ch, cx, y0, y1, pose, lean):
    pal = ch.palette
    sleeve = pal.outfit_secondary
    if pose == "melee":
        r(d, cx - 11 + lean * 2, y0 + 2, cx - 7 + lean * 2, y0 + 12, sleeve)
        r(d, cx + 7 + lean, y0 - 2, cx + 13 + lean, y0 + 4, sleeve)   # forward weapon arm, raised
        r(d, cx + 12 + lean, y0 - 4, cx + 14 + lean, y0 + 2, (200, 200, 210, 255))  # weapon glint
    elif pose == "cast":
        r(d, cx - 9 + lean, y0 - 6, cx - 3 + lean, y0 + 2, sleeve)
        r(d, cx + 3 + lean, y0 - 6, cx + 9 + lean, y0 + 2, sleeve)
    elif pose == "telegraph":
        r(d, cx - 10 + lean, y0 - 10, cx - 6 + lean, y0 + 1, sleeve)
        r(d, cx + 6 + lean, y0 - 10, cx + 10 + lean, y0 + 1, sleeve)
    else:  # idle
        r(d, cx - 10 + lean, y0 + 1, cx - 8 + lean, y0 + 12, sleeve)
        r(d, cx + 8 + lean, y0 + 1, cx + 10 + lean, y0 + 12, sleeve)


def draw_legs(d, ch, cx, y0, y1, stride):
    pal = ch.palette
    foot = (40, 36, 34, 255)
    r(d, cx - 6 - stride, y0, cx - 2 - stride, y1, pal.outfit_secondary)
    r(d, cx + 1 + stride, y0, cx + 5 + stride, y1, pal.outfit_secondary)
    r(d, cx - 7 - stride, y1 - 2, cx - 1 - stride, y1, foot)
    r(d, cx + 0 + stride, y1 - 2, cx + 6 + stride, y1, foot)


def render_frame(ch, block, i, count):
    w, h = ch.canvas_w, ch.canvas_h
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cx = w // 2
    legs_y0 = int(h * 0.55)
    legs_y1 = h - 4
    torso_y0 = int(h * 0.28)
    torso_y1 = legs_y0
    head_y0 = int(h * 0.04)

    if block == "idle":
        bob_cycle = [0, -1, -1, 0, 0, 1]
        bob = bob_cycle[i % len(bob_cycle)]
        draw_legs(d, ch, cx, legs_y0 + bob, legs_y1, stride=0)
        draw_arms(d, ch, cx, torso_y0 + bob, torso_y1 + bob, "idle", lean=0)
        draw_torso(d, ch, cx, torso_y0 + bob, torso_y1 + bob, lean=0, cast=False)
        draw_head_and_face(d, ch, cx, head_y0 + bob, lean=0, roar=False)
    elif block == "melee":
        lean_cycle = [0, 2, 4, 4, 2, 0]
        lean = lean_cycle[i % len(lean_cycle)]
        draw_legs(d, ch, cx, legs_y0, legs_y1, stride=lean // 2)
        draw_arms(d, ch, cx, torso_y0, torso_y1, "melee", lean=lean)
        draw_torso(d, ch, cx, torso_y0, torso_y1, lean=lean // 2, cast=False)
        draw_head_and_face(d, ch, cx, head_y0, lean=lean // 2, roar=False)
    elif block == "cast":
        glow_cycle = [0, 0, 1, 1, 1, 0]
        pulse = glow_cycle[i % len(glow_cycle)]
        draw_legs(d, ch, cx, legs_y0, legs_y1, stride=0)
        draw_arms(d, ch, cx, torso_y0, torso_y1, "cast", lean=0)
        draw_torso(d, ch, cx, torso_y0, torso_y1, lean=0, cast=bool(pulse))
        draw_head_and_face(d, ch, cx, head_y0, lean=0, roar=False)
    elif block == "telegraph":
        raise_cycle = [0, 1, 2, 2, 1, 0]
        rise = raise_cycle[i % len(raise_cycle)]
        draw_legs(d, ch, cx, legs_y0, legs_y1, stride=0)
        draw_arms(d, ch, cx, torso_y0 - rise, torso_y1 - rise, "telegraph", lean=0)
        draw_torso(d, ch, cx, torso_y0 - rise, torso_y1 - rise, lean=0, cast=rise >= 2)
        draw_head_and_face(d, ch, cx, head_y0 - rise, lean=0, roar=rise >= 2)
    elif block == "chase":
        stride_cycle = [0, 2, 3, 2, 0, -2]
        stride = stride_cycle[i % len(stride_cycle)]
        draw_legs(d, ch, cx, legs_y0, legs_y1, stride=stride)
        draw_arms(d, ch, cx, torso_y0, torso_y1, "idle", lean=stride // 2)
        draw_torso(d, ch, cx, torso_y0, torso_y1, lean=stride // 2, cast=False)
        draw_head_and_face(d, ch, cx, head_y0, lean=stride // 2, roar=False)
    elif block == "charge":
        rear_cycle = [0, 1, 2, 2, 2, 1]
        rear = rear_cycle[i % len(rear_cycle)]
        draw_legs(d, ch, cx, legs_y0, legs_y1, stride=0)
        draw_arms(d, ch, cx, torso_y0 - rear, torso_y1 - rear, "telegraph", lean=-rear)
        draw_torso(d, ch, cx, torso_y0 - rear, torso_y1 - rear, lean=-rear, cast=False)
        draw_head_and_face(d, ch, cx, head_y0 - rear, lean=-rear, roar=rear >= 2)

    return outline_pass(img)


def build_frames(ch):
    frames = []
    for block in ch.blocks:
        for i in range(FRAMES_PER_BLOCK):
            frames.append(render_frame(ch, block, i, FRAMES_PER_BLOCK))
    return frames


def assemble_sheet(ch, frames):
    sheet = Image.new("RGBA", (ch.canvas_w, ch.canvas_h * len(frames)), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        sheet.paste(f, (0, i * ch.canvas_h))
    return sheet


def contact_sheet(ch, frames, path):
    cols = FRAMES_PER_BLOCK
    rows = len(frames) // cols
    pad = 4
    cs = Image.new("RGBA", (cols * (ch.canvas_w + pad) + pad, rows * (ch.canvas_h + pad) + pad), (90, 90, 100, 255))
    for i, f in enumerate(frames):
        x = pad + (i % cols) * (ch.canvas_w + pad)
        y = pad + (i // cols) * (ch.canvas_h + pad)
        cs.paste(f, (x, y), f)
    cs = cs.resize((cs.width * 3, cs.height * 3), Image.NEAREST)
    cs.save(path)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--character", action="append", choices=sorted(BOSSES))
    ap.add_argument("--contact-sheet", action="store_true")
    ap.add_argument("--out-dir", type=Path, default=OUT_DIR)
    args = ap.parse_args()

    names = args.character or sorted(BOSSES)
    for key in names:
        ch = BOSSES[key]
        frames = build_frames(ch)
        out = args.out_dir / f"{ch.file_name}.png"
        assemble_sheet(ch, frames).save(out)
        print(f"Wrote {out} ({ch.canvas_w}x{ch.canvas_h * len(frames)}, blocks={ch.blocks})")
        if args.contact_sheet:
            cs_path = args.out_dir / f"{ch.file_name}_contact.png"
            contact_sheet(ch, frames, cs_path)
            print(f"Wrote {cs_path} (preview only - do not commit)")


if __name__ == "__main__":
    main()
