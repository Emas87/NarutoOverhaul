#!/usr/bin/env bash
# Converts a raw AI-generated image (Gemini, etc.) into a tModLoader-ready sprite:
# strips the background to transparency, then downscales with nearest-neighbor
# (never blurry bilinear/bicubic) into an exact WxH canvas, centered.
#
# Usage: prepare_sprite.sh <input_image> <output_png> <width> <height> [bg_color] [fuzz%]
#   bg_color defaults to "white" - match whatever background you told Gemini to use.
#   fuzz defaults to 8% - how close a pixel's color must be to bg_color to count as background;
#   raise it if fringing remains, lower it if you're eating into the subject itself.
set -euo pipefail

if [ "$#" -lt 4 ]; then
	echo "Usage: $0 <input_image> <output_png> <width> <height> [bg_color=white] [fuzz=8%]" >&2
	exit 1
fi

input="$1"
output="$2"
width="$3"
height="$4"
bg_color="${5:-white}"
fuzz="${6:-8}%"

tmp=$(mktemp --suffix=.png)
trap 'rm -f "$tmp"' EXIT

# 1. Key out the background color -> transparency, then trim empty space around the subject.
convert "$input" -fuzz "$fuzz" -transparent "$bg_color" -trim +repage "$tmp"

# 2. Nearest-neighbor resize into the exact target canvas, subject centered.
convert "$tmp" -filter point -resize "${width}x${height}" \
	-background none -gravity center -extent "${width}x${height}" \
	"$output"

echo "Wrote $output (${width}x${height})"
echo "Now open it in Aseprite/GIMP and check edges for color fringing before using it in-game."
