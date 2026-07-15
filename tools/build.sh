#!/usr/bin/env bash
# Builds NarutoOverhaul headlessly (no tModLoader GUI needed): compiles via the real .NET SDK
# against tModLoader's tMLMod.targets, which also packages the .tmod into
# ~/.local/share/Terraria/tModLoader/Mods/ as a post-build step.
#
# Requires the .NET 8 SDK at ~/.dotnet (see: https://dot.net/v1/dotnet-install.sh --channel 8.0
# --install-dir ~/.dotnet). tModLoader's own bundled dotnet is runtime-only and can't compile.
set -euo pipefail

export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"

cd "$(dirname "$0")/.."
dotnet build "$@"
