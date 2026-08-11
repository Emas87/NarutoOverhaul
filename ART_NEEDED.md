# Pixel art needed: 3 assets, 8 form variants

Every item, armor set, NPC, and projectile in the mod already has finished art.
What's left is player-overlay and UI art that currently renders as flat
placeholder rectangles in code. This is the complete list — nothing else in
the mod needs a sprite.

- **Engine:** tModLoader (Terraria 1.4)
- **Style ref:** existing armor sheets in this repo (`Content/Items/Armor/`)
- **Compiled:** 2026-08-11

## At a glance

| Asset | Category | Count | Priority |
|---|---|---|---|
| Transformation aura overlay | Player overlay | 8 forms | High — biggest job, most visible |
| Chakra bar | UI | 1 | Medium |
| Stamina bar | UI | 1 | Medium |
| Active-form indicator | UI | 1 | Low — text label works, icon is a nice-to-have |

## 1. Transformation aura overlay

Right now, activating *any* transformation — Sage Mode, Tailed Beast Mode,
Six Paths Sage Mode, Eight Gates, Chakra Control, Kamui Phase, Byakugan, or
Curse Mark — draws the exact same flat translucent orange square above the
player's head (`Common/PlayerDrawLayers/SageModeDrawLayer.cs`). This is the
single biggest visual gap in the mod: it's the thing a player looks at for
the entire duration of the fight, and all eight forms currently look
identical.

- **Draw hook:** PlayerDrawLayer, after skin
- **Anchor:** `player.Top` ± (−8, −20)
- **Frame grid (body):** 360×224px, 9×4 cells of 40×56
- **Frame grid (head/legs):** 40×1120px, 1×20 vertical strip

An overlay that has to move and rotate with the player's body (not a static
icon) needs a frame sheet built on the **same grid tModLoader already slices
armor layers on** — see the dimensions above, ground-truthed from a real
shipped armor piece elsewhere in this codebase. A full walk/run/jump-cycle
overlay is the "correct" version; a simpler starting point (see options
below) is acceptable for a first pass.

### The 8 forms

| Form | Suggested visual direction |
|---|---|
| Sage Mode | Orange pigment marks around the eyes (toad-sage style) |
| Six Paths Sage Mode | Sage Mode marks + a subtle halo/ripple ring, distinct from base Sage Mode |
| Tailed Beast Mode | Chakra cloak silhouette with visible bubbling/flame edge |
| Eight Gates | Red skin flush + green/teal chakra vapor around limbs |
| Chakra Control | Faint blue chakra outline, subtle — this form is about wall-walking, not raw power |
| Kamui Phase | Warping/intangible edge distortion, partial transparency |
| Byakugan | Eye overlay only (veins + pupil change) — smallest of the eight |
| Curse Mark | Dark flame-mark patches spreading across visible skin |

### Two ways to scope this

1. **Full animated overlay** (matches the grid above exactly) — looks
   correct in every player pose, most work per form.
2. **Static badge/aura sprite** anchored near the player (current
   placeholder's approach, just replacing the flat square with real art) —
   much less work, ships all 8 forms faster, can be upgraded to (1) later
   per-form without touching the other seven.

Recommend starting with option 2 for all 8 forms, then upgrading the
most-used forms (Sage Mode, Tailed Beast Mode) to option 1 if budget allows.

## 2 & 3. Resource bars

Two HUD bars, stacked in the gap between the inventory panel and minimap,
top-right of screen. Both currently render as a solid-black background
rectangle plus a solid-color fill rectangle — no frame, no texture, just
flat color blocks.

### Chakra bar

Ninjutsu resource. Fills as the player spends chakra on jutsu; text overlay
shows current/max.

- **Footprint:** 150 × 20px
- **Position:** top bar of the pair
- **Current fill color:** rgb(40, 130, 220) — chakra blue
- **Current bg color:** black @ 60% alpha

### Stamina bar

Taijutsu resource. Same mechanic as chakra, separate pool for taijutsu
abilities.

- **Footprint:** 150 × 20px
- **Position:** 25px below the chakra bar
- **Current fill color:** rgb(235, 210, 30) — stamina gold
- **Current bg color:** black @ 60% alpha

Deliverable: a 9-slice or fixed-size frame texture plus a fill texture per
bar (matching Terraria's own vanilla life/mana bar convention is a safe
reference point). Numeric text is drawn separately in code and doesn't need
to be part of the art.

## 4. Active-form indicator (low priority)

A plain white bordered text label naming whichever transformation is
currently active, shown just under the two resource bars. Fully functional
as-is — an icon or small styled badge next to the text would be a polish
pass, not a fix. Only worth scheduling after the aura overlay (item 1) is
done.

---

Everything not listed above — all weapon, armor, accessory, NPC, boss,
projectile, minion, and tile sprites — is already finished art in the repo
and out of scope for this job.
