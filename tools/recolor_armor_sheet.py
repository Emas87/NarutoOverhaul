"""Recolor a vanilla armor equip-texture sheet onto a new palette by luminance.

Keeps the source sheet's exact shape, alpha, and shading structure (shadows
stay dark, highlights stay light) but remaps hue/saturation to a target
gradient, so a reskinned sheet reads as "the same silhouette, new color"
rather than a from-scratch redraw.

Usage:
    python3 recolor_armor_sheet.py <input.png> <output.png>
"""

import sys
from PIL import Image


# Ninjutsu palette, sampled from NinjutsuHelmetItem_Head.png / NinjutsuLegsItem_Legs.png:
# dark navy shadow -> mid slate-blue -> lighter blue highlight.
GRADIENT = [
    (18, 21, 38),
    (33, 38, 63),
    (48, 51, 72),
    (72, 79, 112),
    (110, 120, 160),
]


def lerp(a, b, t):
    return a + (b - a) * t


def sample_gradient(t):
    t = max(0.0, min(1.0, t))
    n = len(GRADIENT) - 1
    pos = t * n
    i = min(int(pos), n - 1)
    local_t = pos - i
    c0 = GRADIENT[i]
    c1 = GRADIENT[i + 1]
    return tuple(round(lerp(c0[k], c1[k], local_t)) for k in range(3))


def main():
    if len(sys.argv) != 3:
        print(__doc__)
        sys.exit(1)

    src_path, dst_path = sys.argv[1], sys.argv[2]
    im = Image.open(src_path).convert("RGBA")
    px = im.load()
    w, h = im.size

    opaque_luma = [
        0.299 * px[x, y][0] + 0.587 * px[x, y][1] + 0.114 * px[x, y][2]
        for y in range(h)
        for x in range(w)
        if px[x, y][3] > 0
    ]
    if not opaque_luma:
        print("No opaque pixels found; nothing to recolor.")
        sys.exit(1)
    lo, hi = min(opaque_luma), max(opaque_luma)
    span = max(hi - lo, 1e-6)

    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            luma = 0.299 * r + 0.587 * g + 0.114 * b
            t = (luma - lo) / span
            nr, ng, nb = sample_gradient(t)
            px[x, y] = (nr, ng, nb, a)

    im.save(dst_path)
    print(f"Wrote {dst_path} ({w}x{h})")


if __name__ == "__main__":
    main()
