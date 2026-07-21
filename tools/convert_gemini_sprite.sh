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
# Color-key fuzz alone leaves faint background-tinted "spill" on anti-aliased edges (blended
# pixels that are opaque but colored partway toward the background hue - a standard green-screen
# keying problem, not something raising the fuzz% fixes: too low leaves spill, too high eats real
# edge detail before the spill is gone). Step 4 below does a second, hue-based pass instead of a
# color-distance one: it drops any pixel whose dominant channel matches the background's dominant
# channel by a wide margin, which catches spill without needing a color-distance threshold that
# fights the subject's own grays.
#
# Usage: convert_gemini_sprite.sh <input_image> <output_png> <width> <height> [bg_color=#545454] [fuzz=12%] [crop=WxH+X+Y]
#   crop: optional manual crop region (ImageMagick geometry, e.g. 458x441+475+163) applied instead
#   of auto -trim. Needed when the art has a decorative element (motion-blur rings, glow halo) in
#   the same hue family as the background - auto-trim's bounding box then includes it, and no
#   fuzz/spill-cleanup setting can cleanly separate two things sharing a hue. Find it by trimming
#   with a much higher throwaway fuzz (which happens to erase the decorative element too) and
#   reading that bounding box off `identify -format "%wx%h%O"`.
set -euo pipefail

if [ "$#" -lt 4 ]; then
	echo "Usage: $0 <input_image> <output_png> <width> <height> [bg_color=#545454] [fuzz=12%] [crop=WxH+X+Y]" >&2
	exit 1
fi

input="$1"
output="$2"
width="$3"
height="$4"
bg_color="${5:-#545454}"
fuzz="${6:-12}%"
crop="${7:-}"

tmp1=$(mktemp --suffix=.png)
tmp2=$(mktemp --suffix=.png)
tmp3=$(mktemp --suffix=.png)
trap 'rm -f "$tmp1" "$tmp2" "$tmp3"' EXIT

img_width=$(identify -format "%w" "$input")
img_height=$(identify -format "%h" "$input")
corner_x=$((img_width * 82 / 100))
corner_y=$((img_height * 70 / 100))

# 1. Key out the baked-in gray background -> real transparency.
convert "$input" -fuzz "$fuzz" -transparent "$bg_color" "$tmp1"

# 2. Blank the watermark corner too (it's opaque, not gray, so the color-key above won't catch it).
# NOTE: "-channel A -fill black -draw rectangle ... +channel" looks like it should zero the alpha
# in that region, but under ImageMagick 7 it instead draws a fully OPAQUE black rectangle (alpha=255)
# - the exact opposite of intended, verified via raw pixel dump. -region + -evaluate set 0 is the
# IM7-correct way to force alpha to 0 in a sub-rectangle regardless of what's under it.
corner_w=$((img_width - corner_x))
corner_h=$((img_height - corner_y))
convert "$tmp1" -channel A -region "${corner_w}x${corner_h}+${corner_x}+${corner_y}" -evaluate set 0 +region +channel "$tmp2"

# 3. Crop to the actual subject (manual crop if given, otherwise auto-trim), then nearest-neighbor
# resize into the exact target canvas.
if [ -n "$crop" ]; then
	convert "$tmp2" -crop "$crop" +repage -filter point -resize "${width}x${height}" \
		-background none -gravity center -extent "${width}x${height}" \
		"$tmp3"
else
	convert "$tmp2" -trim +repage -filter point -resize "${width}x${height}" \
		-background none -gravity center -extent "${width}x${height}" \
		"$tmp3"
fi

# 4. Hue-based spill cleanup at the final small size (cheap - only width*height pixels here).
# Auto-detects which channel bg_color is dominant in, then drops any opaque pixel that shares that
# same dominant-channel signature by a wide margin (>12%), regardless of exact color distance.
# `|| true`: convert's info: output has no trailing newline, so `read` correctly populates the
# variables but still returns 1 at EOF - without the override, `set -e` would kill the script here.
read -r bg_r bg_g bg_b < <(convert xc:"$bg_color" -format "%[fx:int(255*r)] %[fx:int(255*g)] %[fx:int(255*b)]" info:) || true
if [ "$bg_g" -gt "$bg_r" ] && [ "$bg_g" -gt "$bg_b" ]; then
	spill_fx="(g>r*1.12)&&(g>b*1.12) ? 0 : a"
elif [ "$bg_r" -gt "$bg_g" ] && [ "$bg_b" -gt "$bg_g" ]; then
	spill_fx="(r>g*1.12)&&(b>g*1.12) ? 0 : a"
else
	spill_fx="a"
fi
convert "$tmp3" -channel A -fx "$spill_fx" +channel "$output"

echo "Wrote $output (${width}x${height})"
echo "Verify with: convert $output -crop 4x4+0+0 txt:-  (should show alpha=0 at the edges)"
echo "Then check edges/fringing in GIMP/Aseprite before using it in-game."
