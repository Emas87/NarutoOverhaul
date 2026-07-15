#!/usr/bin/env bash
# Converts a Gemini-generated image into a tModLoader-ready sprite.
#
# IMPORTANT: despite looking like they have a transparent checkerboard background when previewed,
# these images have NO real alpha - the "checkerboard" is a flat opaque gray (~RGB 82-85) baked
# into the pixels, and every image viewer we tried (including flattening onto solid purple and
# re-exporting as JPEG, which cannot carry alpha at all) still showed a checker grid - proving
# it's just decorative chrome the viewer always draws, not evidence of real transparency. Verified
# via raw pixel dumps (`convert file.png -crop WxH+X+Y txt:-`), which is the only reliable way to
# check alpha/color here - never trust a rendered preview for that.
#
# So background removal here is color-keying (fuzzy match on that gray), not alpha-based.
#
# Usage: convert_gemini_sprite.sh <input_image> <output_png> <width> <height> [bg_color=#545454] [fuzz=12%]
set -euo pipefail

if [ "$#" -lt 4 ]; then
	echo "Usage: $0 <input_image> <output_png> <width> <height> [bg_color=#545454] [fuzz=12%]" >&2
	exit 1
fi

input="$1"
output="$2"
width="$3"
height="$4"
bg_color="${5:-#545454}"
fuzz="${6:-12}%"

tmp1=$(mktemp --suffix=.png)
tmp2=$(mktemp --suffix=.png)
trap 'rm -f "$tmp1" "$tmp2"' EXIT

img_width=$(identify -format "%w" "$input")
img_height=$(identify -format "%h" "$input")
corner_x=$((img_width * 82 / 100))
corner_y=$((img_height * 70 / 100))

# 1. Key out the baked-in gray background -> real transparency.
convert "$input" -fuzz "$fuzz" -transparent "$bg_color" "$tmp1"

# 2. Blank the watermark corner too (it's opaque, not gray, so the color-key above won't catch it).
convert "$tmp1" -channel A -fill black -draw "rectangle ${corner_x},${corner_y} ${img_width},${img_height}" +channel "$tmp2"

# 3. Trim to the actual subject, then nearest-neighbor resize into the exact target canvas.
convert "$tmp2" -trim +repage -filter point -resize "${width}x${height}" \
	-background none -gravity center -extent "${width}x${height}" \
	"$output"

echo "Wrote $output (${width}x${height})"
echo "Verify with: convert $output -crop 4x4+0+0 txt:-  (should show alpha=0 at the edges)"
echo "Then check edges/fringing in GIMP/Aseprite before using it in-game."
