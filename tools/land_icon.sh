#!/usr/bin/env bash
# Finds the most recently downloaded image in ~/Downloads and runs it through
# convert_gemini_sprite.sh into the given repo-relative destination. Companion to the
# Gemini-in-browser icon workflow documented in ICON_REGEN_TODO.md.
#
# Usage: tools/land_icon.sh <dest_relpath_under_Content_Items> <w> <h> [bg_color=#00FF00] [fuzz=12%]
set -euo pipefail

if [ "$#" -lt 3 ]; then
	echo "Usage: $0 <dest_relpath_under_Content_Items> <w> <h> [bg_color=#00FF00] [fuzz=12%]" >&2
	exit 1
fi

dest="Content/Items/$1"
w="$2"
h="$3"
bg_color="${4:-#00FF00}"
fuzz="${5:-12%}"

latest=$(find ~/Downloads -maxdepth 1 -type f \( -iname "*.jpeg" -o -iname "*.jpg" -o -iname "*.png" \) -printf '%T@ %p\n' | sort -rn | head -1 | cut -d' ' -f2-)
echo "using $latest -> $dest (${w}x${h}, bg=$bg_color, fuzz=$fuzz)"
bash "$(dirname "$0")/convert_gemini_sprite.sh" "$latest" "$dest" "$w" "$h" "$bg_color" "$fuzz"
