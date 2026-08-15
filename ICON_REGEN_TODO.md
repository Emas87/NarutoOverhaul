## STATUS: DONE (completed in follow-up session, 2026-08-15)

All 4 originally-listed items were regenerated and landed successfully via Gemini +
`tools/land_icon.sh`, verified by zoomed-PNG read-back:
- `Weapons/KunaiItem.png` - clear blade/handle distinction now.
- `Weapons/ExplosiveKunaiItem.png` - red paper tag with kanji now clearly visible.
- `Weapons/MinatoKunaiItem.png` - three-prong blade now unmistakable (paper tag renders
  slightly disconnected from the ring at this tiny 34x14 canvas, but silhouette reads
  correctly - accepted as a minor artifact, not worth another regen).
- `Content/Items/Accessories/ChakraWingsItem.png` - regenerated blue as requested. First
  landing attempt hit the known "JPEG noise breaks auto-crop" bug (see below); the retry
  with `fuzz=22%` fixed it.

The full 83-icon audit (see "How to audit" below) was completed this session and found one
**additional** bug not in the original list: `Content/Items/Armor/TaijutsuHelmetItem.png`
(the inventory-slot icon, not the `_Head` equip-layer sprite) had never been keyed at all -
it still had its raw green background baked in, plus a stray uncropped white corner. Root
cause unclear (predates this session, unrelated to the two documented conversion-script
bugs), but it was regenerated from scratch (prompt: red cloth headband with a gold sun/flame
emblem plate) and landed cleanly at default fuzz. No other icons among the 83 showed
problems in the audit.

`tools/build.sh` and `tools/smoke_test_server.sh` both pass. Nothing has been committed -
same "ask before commit" state as noted below.

---

# Session handoff: icon regeneration (needs Chrome/Gemini)

Written 2026-08-15. **Why this exists**: the `claude-in-chrome` MCP tool disconnected mid-session
and repeated `/chrome` reconnect attempts didn't bring it back. The user is moving to a different
session that has working browser capabilities to finish this. Everything needed to pick up
exactly where this session left off is below.

## Context - what already happened

This is the tail end of a larger icon-clarity project (see git log: "Regenerate all item icons
for clarity" and related commits). All 82 item icons in `Content/Items/**` were regenerated once
already via Gemini in the browser, at larger canvases with bolder outlines (see
`SPRITE_PROMPTS.md`). After that pass, the user found 3 more problems by testing in-game:

1. Taijutsu armor set (Body/Helmet/Legs) rendered transparent - **fixed and verified this
   session** (root cause: `tools/convert_gemini_sprite.sh`'s hue-based background-spill cleanup
   was deleting the armor's own green pixels because the subject color matched the green keying
   background - see "Known bugs in the conversion script" below).
2. Fuma Shuriken rendered almost transparent - **fixed and verified this session** (different
   root cause: JPEG compression noise defeated the auto-crop - see below).
3. Icon sizes looked inconsistent across categories in the inventory - **fixed this session**,
   but this was a *code* fix, not an art fix: `Common/GlobalItems/InventoryIconScaleGlobalItem.cs`
   now draws every NarutoOverhaul icon at one consistent on-screen pixel size regardless of the
   source PNG's native canvas, instead of relying on vanilla's inconsistent size-based clamping.

All of the above is done, built, and smoke-tested successfully. **What's left** is 4 icons that
need genuinely new art (not just re-keying), listed below - this is the part that needs the
browser.

## Items that need real regeneration

Confirmed by zooming each PNG 300-400% and looking at it directly (do the same to re-verify, or
to find more candidates - see "How to audit" below):

1. **`Content/Items/Weapons/KunaiItem.png`** (26x26) - current art reads as a thin uniform dark
   diagonal stick, no visible metallic blade vs handle distinction. Doesn't read as "kunai."
2. **`Content/Items/Weapons/ExplosiveKunaiItem.png`** (26x26) - same silhouette problem as
   Kunai, plus the red paper tag is too small to register. **Re-confirmed via zoomed audit this
   session** - still reads as a dark stick with a tiny red dot, not a kunai.
3. **`Content/Items/Weapons/MinatoKunaiItem.png`** (34x14) - the three-prong blade is barely
   visible; reads more like a hook/key than Minato's signature kunai.
4. **`Content/Items/Accessories/ChakraWingsItem.png`** (32x32) - art itself is fine (clean
   glowing wings, correct silhouette, re-confirmed this session) but it's gold/tan. **User wants
   it blue instead.** A programmatic hue-shift was tried and rejected (muddy result - pale
   highlight pixels don't recolor, leftover green edge-spill shifts to the wrong color) - this
   needs a real from-scratch regeneration with a blue color prompt, not a recolor of the existing
   file.

**Not yet fully audited**: only a partial visual sweep of the 82 icons was done this session
(one 6x4 montage grid, ~21 of 83 files, via the "How to audit" method below) before the browser
disconnected. Everything reviewed in that partial sweep looked correct except the 4 above. The
remaining ~62 icons haven't been re-checked since the original regeneration pass - do a full
sweep before or during this work in case there are more.

### How to audit for more candidates

```bash
mkdir -p /tmp/icon_audit
find Content/Items -iname "*.png" -not -name "*_Head*" -not -name "*_Body*" -not -name "*_Arms*" -not -name "*_Legs*" -not -name "*_Wings*" | while read f; do
  convert "$f" -filter point -resize 300% "/tmp/icon_audit/$(basename "$f")"
done
cd /tmp/icon_audit
ls *.png > /tmp/icon_audit_files.txt
split -n l/4 -d /tmp/icon_audit_files.txt /tmp/icon_chunk_
for i in 00 01 02 03; do
  montage $(cat /tmp/icon_chunk_$i) -tile 6x4 -geometry 160x160+6+6 -background "#222222" \
    -label '%f' -font DejaVu-Sans -pointsize 11 -fill white /tmp/icon_grid_$i.png
done
```
Then read each `/tmp/icon_grid_0{0,1,2,3}.png` (Read tool) and cross-reference grid position
against the corresponding `/tmp/icon_chunk_0X` file list (row-major, 6 per row) to identify
filenames. Add anything unclear/wrong to this doc's list before regenerating it.

## Prompts to use for the 4 confirmed items

Reuse the style suffix already established in `SPRITE_PROMPTS.md`'s workflow notes - every
prompt below already has it appended. Paste each prompt into Gemini as-is.

**KunaiItem.png** (target 26x26):
> A simple steel kunai throwing knife with a ring pommel and wrapped grip, high metallic
> contrast between the blade and handle, bold thick dark outlines, flat saturated colors,
> single simplified silhouette, no fine gradients or thin linework, pixel art sprite in the
> style of a hand-painted 16-bit SNES game asset, hand-painted pixel art matching Terraria's
> native item icon aesthetic, single object isolated and centered in frame, not
> photorealistic, no soft blur, no lens-flare, no drop shadow, as a 2D game item icon sprite,
> solid flat green background (#00FF00), no checkered pattern, no gradient.

**ExplosiveKunaiItem.png** (target 26x26):
> A steel kunai throwing knife with a ring pommel, high metallic contrast between the blade
> and handle, a clearly visible red explosive paper tag with kanji marking and a lit fuse
> wrapped around the grip, bold thick dark outlines, flat saturated colors, single simplified
> silhouette, no fine gradients or thin linework, pixel art sprite in the style of a
> hand-painted 16-bit SNES game asset, hand-painted pixel art matching Terraria's native item
> icon aesthetic, single object isolated and centered in frame, not photorealistic, no soft
> blur, no lens-flare, no drop shadow, as a 2D game item icon sprite, solid flat green
> background (#00FF00), no checkered pattern, no gradient.

**MinatoKunaiItem.png** (target 34x14 - wide/short canvas, keep the prong blade prominent and
horizontal so it isn't lost at this aspect ratio):
> A steel kunai throwing knife with three distinctive prong blades fanned out clearly, a paper
> tag tied to the ring pommel, high metallic contrast, bold thick dark outlines, flat
> saturated colors, single simplified silhouette, no fine gradients or thin linework, pixel
> art sprite in the style of a hand-painted 16-bit SNES game asset, hand-painted pixel art
> matching Terraria's native item icon aesthetic, single object isolated and centered in
> frame, not photorealistic, no soft blur, no lens-flare, no drop shadow, as a 2D game item
> icon sprite, solid flat green background (#00FF00), no checkered pattern, no gradient.

**ChakraWingsItem.png** (target 32x32, recolored blue):
> A small folded pair of glowing electric-blue and white chakra wings pinned like a badge,
> radiant feather-light energy strands, bold thick dark outlines, flat saturated colors,
> single simplified silhouette, no fine gradients or thin linework, pixel art sprite in the
> style of a hand-painted 16-bit SNES game asset, hand-painted pixel art matching Terraria's
> native item icon aesthetic, single object isolated and centered in frame, not
> photorealistic, no soft blur, no lens-flare, no drop shadow, as a 2D game item icon sprite,
> solid flat green background (#00FF00), no checkered pattern, no gradient.

## Browser workflow (proven this session)

1. Navigate to `https://gemini.google.com/app`. Either continue an existing chat thread from
   earlier in the project (e.g. one titled "Pixel Art Kunai Game Icon") or start a new
   conversation - state doesn't carry over prompt-to-prompt either way.
2. Click the message composer, type the full prompt (see above), click the send button (the
   circular arrow button bottom-right of the composer - **do not** rely on pressing Enter, it
   just inserts a newline in this multi-line composer and silently fails to send).
3. Wait ~15-40s for "Creating your image" to resolve into the actual image. It's sometimes
   slow, or the page needs an extra screenshot/poll to notice completion - be patient, don't
   assume it's stuck too early.
4. Hover over the generated image to reveal the download icon (top-right corner of the image,
   a downward-arrow in a circle) and click it. It saves to `~/Downloads/Gemini_Generated_*.jpeg`.
5. Run the conversion locally:
   ```
   bash tools/land_icon.sh <RelativePathUnderContentItems> <width> <height> [bg_color] [fuzz]
   ```
   Examples:
   ```
   bash tools/land_icon.sh Weapons/KunaiItem.png 26 26
   bash tools/land_icon.sh Weapons/ExplosiveKunaiItem.png 26 26
   bash tools/land_icon.sh Weapons/MinatoKunaiItem.png 34 14
   bash tools/land_icon.sh Accessories/ChakraWingsItem.png 32 32
   ```
   `tools/land_icon.sh` was added this session - it just finds the newest file in
   `~/Downloads` and pipes it through the existing `tools/convert_gemini_sprite.sh`. It doesn't
   need `chmod +x`; run it with `bash tools/land_icon.sh ...` as shown.
6. **Read the resulting PNG back (e.g. via the Read tool, or the zoom-and-montage technique
   above) before moving on** - visually confirm it actually looks like the intended item and
   isn't transparent/broken. See "Known bugs in the conversion script" below for the two ways
   this can silently go wrong.
7. Once all 4 are landed and look correct, run `tools/build.sh` and
   `tools/smoke_test_server.sh` to confirm nothing broke, same as every other batch in this
   project (see `feedback_build_verify` memory / `CLAUDE.md`).

## Known bugs in the conversion script (both fixed reactively this session - watch for them again)

`tools/convert_gemini_sprite.sh` has two failure modes that produce a technically-valid but
visually-broken PNG (small file size, but wrong/empty content) - neither errors out, so you have
to actually look at the result:

1. **Hue-based spill cleanup eats the subject itself.** Step 4 of the script drops any pixel
   whose color leans toward the keying background's hue, to clean up green-fringe spill on
   edges. If the *subject itself* is close to the background's hue (this happened to Taijutsu's
   green gi against a green background), this deletes real art, not just spill. **Fix: if a
   prompt's subject is itself green, key against magenta instead** - pass `"#FF00FF"` as the
   bg_color arg to `land_icon.sh`/`convert_gemini_sprite.sh`, and change the prompt's background
   line to say "solid flat magenta background (#FF00FF)" instead of green. None of the 4 items
   above are green-dominant, so this shouldn't come up, but keep it in mind if a regenerated
   image comes back looking oddly hollow.
2. **JPEG noise breaks the auto-crop.** A few background-corner pixels can land just outside the
   color-key fuzz threshold, which breaks `-trim`'s border-detection and leaves the whole
   uncropped canvas, shrinking the actual subject into a tiny speck once resized down (this
   happened to Fuma Shuriken). **Fix: if the result looks nearly empty/way too small, rerun with
   a higher fuzz** - try `22%` instead of the default `12%`, e.g.:
   ```
   bash tools/land_icon.sh Weapons/KunaiItem.png 26 26 "#00FF00" 22%
   ```

## Reference: Item.width/height (no code change needed for these 4)

These 4 items already have the correct `Item.width`/`Item.height` values in their `.cs` files
from the previous regeneration pass - only the PNG art needs replacing, not the code:

- `KunaiItem.cs`: 26x26
- `ExplosiveKunaiItem.cs`: 26x26
- `MinatoKunaiItem.cs`: 34x14
- `ChakraWingsItem.cs`: 32x32

## Repo state to be aware of

`git status` currently shows all the icon-fix changes from this session as uncommitted
(numerous `Content/Items/**/*.png` and matching `.cs` width/height edits, plus the new
`Common/GlobalItems/InventoryIconScaleGlobalItem.cs` and `tools/land_icon.sh`). Nothing has been
committed yet - the user hasn't asked for a commit. Don't assume it's safe to `git checkout`/
`reset`/`clean` anything without checking with the user first.
