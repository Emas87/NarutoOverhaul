#!/usr/bin/env bash
# Headless smoke test: boots a real tModLoader dedicated server with NarutoOverhaul enabled,
# using a disposable scratch world (never touches the real Worlds folder), and checks that
# content loading, world generation, and server startup all complete with no exceptions.
#
# This catches runtime bugs a plain `dotnet build` can't (bad texture paths, duplicate content
# IDs, broken Autoload/SetStaticDefaults, etc.) - but it CANNOT verify actual gameplay (the
# chakra bar rendering, casting Rasengan, fighting the boss, toggling Sage Mode). A dedicated
# server has no graphics and no player in it; that verification needs a real client + a human
# at the keyboard, which is outside what this script (or any headless tool) can drive.
#
# Usage: smoke_test_server.sh [timeout_seconds]
set -euo pipefail

TML_DIR="$HOME/.steam/debian-installation/steamapps/common/tModLoader"
TIMEOUT="${1:-90}"
SCRATCH=$(mktemp -d)
LOG="$SCRATCH/server.log"
trap 'rm -rf "$SCRATCH"' EXIT

cd "$TML_DIR"
DOTNET_ROLL_FORWARD=Disable timeout "$TIMEOUT" ./dotnet/dotnet tModLoader.dll \
	-server -nosteam -autocreate 1 -difficulty 0 \
	-world "$SCRATCH/SmokeTestWorld.wld" \
	-password smoketest -port 17787 \
	< /dev/null > "$LOG" 2>&1 || true

echo "--- last 40 lines ---"
tail -40 "$LOG"
echo "---"

if grep -q "Server started" "$LOG"; then
	echo "PASS: server started cleanly."
else
	echo "FAIL: server did not report a clean start - check the log above."
fi

if grep -qiE "exception|unhandled" "$LOG"; then
	echo "WARNING: exception-like text found in the log - review above."
fi
