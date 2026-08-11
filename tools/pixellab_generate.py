#!/usr/bin/env python3
"""Generate armor sheets / head recolors via the PixelLab API (api.pixellab.ai/v2).

Usage:
  python3 pixellab_generate.py edit-image <base_png> <out_png> \
      --w <target_w> --h <target_h> --desc "<prompt>" [--no-bg/--bg]

Requires PIXELLAB_API_KEY env var (or --key).
"""
import argparse
import base64
import json
import os
import sys
import time
import urllib.request
import urllib.error

from PIL import Image

API_BASE = "https://api.pixellab.ai/v2"


def _post(path, payload, key):
    req = urllib.request.Request(
        API_BASE + path,
        data=json.dumps(payload).encode(),
        headers={
            "Authorization": f"Bearer {key}",
            "Content-Type": "application/json",
        },
        method="POST",
    )
    try:
        with urllib.request.urlopen(req, timeout=60) as resp:
            return json.loads(resp.read())
    except urllib.error.HTTPError as e:
        body = e.read().decode(errors="replace")
        raise RuntimeError(f"HTTP {e.code} on {path}: {body}")


def _get(path, key):
    req = urllib.request.Request(
        API_BASE + path,
        headers={"Authorization": f"Bearer {key}"},
    )
    try:
        with urllib.request.urlopen(req, timeout=30) as resp:
            return json.loads(resp.read())
    except urllib.error.HTTPError as e:
        body = e.read().decode(errors="replace")
        raise RuntimeError(f"HTTP {e.code} on {path}: {body}")


def png_to_b64(path):
    with open(path, "rb") as f:
        return "data:image/png;base64," + base64.b64encode(f.read()).decode()


def b64_to_png(data_uri, out_path, width=None, height=None):
    if "," in data_uri:
        data_uri = data_uri.split(",", 1)[1]
    raw = base64.b64decode(data_uri)
    # PixelLab's background-job responses return raw RGBA pixel bytes (no PNG
    # container), not a data URI despite the field name. Detect real PNGs
    # (magic bytes) vs raw RGBA and handle both.
    if raw[:8] == b"\x89PNG\r\n\x1a\n":
        with open(out_path, "wb") as f:
            f.write(raw)
        return
    if width is None or height is None:
        raise RuntimeError("Raw RGBA payload received but width/height unknown")
    img = Image.frombytes("RGBA", (width, height), raw)
    img.save(out_path)


def edit_image(base_png, out_png, ref_w, ref_h, out_w, out_h, desc, no_bg, key, seed=None):
    payload = {
        "image": {"base64": png_to_b64(base_png)},
        "image_size": {"width": ref_w, "height": ref_h},
        "description": desc,
        "width": out_w,
        "height": out_h,
        "no_background": no_bg,
    }
    if seed is not None:
        payload["seed"] = seed
    print(f"[edit-image] {base_png} -> {out_png} ({out_w}x{out_h}) ...")
    resp = _post("/edit-image", payload, key)
    job_id = resp["background_job_id"]
    print(f"  job {job_id} submitted, polling...")
    while True:
        time.sleep(5)
        status = _get(f"/background-jobs/{job_id}", key)
        st = status["status"]
        if st == "completed":
            last = status["last_response"]
            img = last.get("image") or (last.get("images") or [None])[0]
            if img is None:
                raise RuntimeError(f"No image in completed response: {json.dumps(last)[:500]}")
            b64_to_png(img["base64"], out_png, width=out_w, height=out_h)
            print(f"  done -> {out_png}")
            return out_png
        if st == "failed":
            raise RuntimeError(f"Job failed: {json.dumps(status.get('last_response', {}))[:500]}")
        print(f"  ...still {st}")


def main():
    ap = argparse.ArgumentParser()
    sub = ap.add_subparsers(dest="cmd", required=True)

    e = sub.add_parser("edit-image")
    e.add_argument("base_png")
    e.add_argument("out_png")
    e.add_argument("--ref-w", type=int, required=True)
    e.add_argument("--ref-h", type=int, required=True)
    e.add_argument("--w", type=int, required=True)
    e.add_argument("--h", type=int, required=True)
    e.add_argument("--desc", required=True)
    e.add_argument("--bg", action="store_true", help="keep background (default: transparent)")
    e.add_argument("--seed", type=int, default=None)
    e.add_argument("--key", default=os.environ.get("PIXELLAB_API_KEY"))

    args = ap.parse_args()
    if not args.key:
        print("ERROR: set PIXELLAB_API_KEY or pass --key", file=sys.stderr)
        sys.exit(1)

    if args.cmd == "edit-image":
        edit_image(
            args.base_png, args.out_png,
            args.ref_w, args.ref_h, args.w, args.h,
            args.desc, not args.bg, args.key, args.seed,
        )


if __name__ == "__main__":
    main()
