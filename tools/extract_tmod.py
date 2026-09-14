#!/usr/bin/env python3
"""Extracts files (including .rawimg textures, decoded to real .png) out of any .tmod file -
including other people's Workshop mods, not just our own. `.tmod` is a custom binary container
(magic "TMOD", .NET-string-prefixed header fields, then a file table, then each file's bytes
individually DeflateStream-compressed unless it's already-compressed content like .png/.rawimg,
below 1KB, or opted out). Format ground-truthed 2026-08-04 against tModLoader's own
Terraria/ModLoader/Core/TmodFile.cs while investigating why our armor equip textures were
rendering wrong - see ARMOR_REFERENCE_IMAGES.md "Template shape correction" for why this mattered.

.rawimg (tModLoader's internal texture format, what .png assets get converted to when packaged)
is: 3x little-endian int32 header (version, width, height), then width*height*4 bytes of BGRA
pixel data, 4 bytes per pixel unconditionally (fully-transparent pixels are still 4 bytes, just
written as a single zero int rather than 4 explicit bytes - same size either way).

Usage:
  # List every file in a mod whose path contains a filter string (case-insensitive):
  tools/extract_tmod.py list <path/to/Mod.tmod> [filter]

  # Extract one file by its exact in-archive path, decoding .rawimg to .png automatically:
  tools/extract_tmod.py extract <path/to/Mod.tmod> <in-archive-path> <out.png>

Installed mods (including Workshop) live under:
  ~/.local/share/Terraria/tModLoader/Mods/*.tmod                                  (local/dev)
  <tModLoader install dir>/steamapps/workshop/content/1281930/<id>/<tml-version>/*.tmod  (Workshop)
"""
import struct
import sys
import zlib
from pathlib import Path

from PIL import Image


def _read_7bit_string(f):
    result = 0
    shift = 0
    while True:
        b = f.read(1)
        if not b:
            raise EOFError(
                f"Unexpected end of file while reading a 7-bit-encoded string length "
                f"at offset {f.tell()} - the .tmod file is truncated or its format "
                f"doesn't match what this script expects."
            )
        b = b[0]
        result |= (b & 0x7F) << shift
        if not (b & 0x80):
            break
        shift += 7
    return f.read(result).decode("utf-8")


def parse_tmod(path):
    """Returns (raw file-table-relative data bytes, mod name, mod version,
    list of [name, length, compressedLength, offset-into-data])."""
    with open(path, "rb") as f:
        magic = f.read(4)
        if magic != b"TMOD":
            sys.exit(f"Not a .tmod file (bad magic {magic!r}): {path}")
        f.seek(0)
        f.read(4)
        _tml_version = _read_7bit_string(f)
        _hash = f.read(20)
        _signature = f.read(256)
        _datalen = struct.unpack("<i", f.read(4))[0]
        name = _read_7bit_string(f)
        modversion = _read_7bit_string(f)
        numfiles = struct.unpack("<i", f.read(4))[0]
        entries = []
        for _ in range(numfiles):
            ename = _read_7bit_string(f)
            length = struct.unpack("<i", f.read(4))[0]
            clength = struct.unpack("<i", f.read(4))[0]
            entries.append([ename, length, clength, None])
        data = f.read()
    offset = 0
    for e in entries:
        e[3] = offset
        offset += e[2]
    return data, name, modversion, entries


def extract_file(data, entry):
    """Returns raw decompressed bytes for one file table entry."""
    _name, length, clength, off = entry
    raw = data[off : off + clength]
    if clength != length:
        raw = zlib.decompress(raw, -15)  # raw deflate, no zlib/gzip header
    return raw


def rawimg_to_pil(raw):
    version, width, height = struct.unpack("<iii", raw[:12])
    body = bytearray(raw[12 : 12 + width * height * 4])
    for i in range(0, len(body), 4):  # BGRA -> RGBA
        body[i], body[i + 2] = body[i + 2], body[i]
    return Image.frombytes("RGBA", (width, height), bytes(body)), version


def main():
    if len(sys.argv) < 3:
        sys.exit(__doc__)
    cmd, tmod_path = sys.argv[1], sys.argv[2]
    data, name, modversion, entries = parse_tmod(tmod_path)
    print(f"Mod: {name} v{modversion}, {len(entries)} files", file=sys.stderr)

    if cmd == "list":
        filt = sys.argv[3] if len(sys.argv) > 3 else ""
        for e in entries:
            if filt.lower() in e[0].lower():
                print(e[0], e[1], e[2])
    elif cmd == "extract":
        in_path, out_path = sys.argv[3], sys.argv[4]
        matches = [e for e in entries if e[0] == in_path]
        if not matches:
            sys.exit(f"No such file in archive: {in_path}")
        raw = extract_file(data, matches[0])
        if in_path.lower().endswith(".rawimg"):
            img, version = rawimg_to_pil(raw)
            img.save(out_path)
            print(f"Decoded rawimg v{version} {img.size} -> {out_path}")
        else:
            Path(out_path).write_bytes(raw)
            print(f"Wrote {len(raw)} bytes -> {out_path}")
    else:
        sys.exit(__doc__)


if __name__ == "__main__":
    main()
