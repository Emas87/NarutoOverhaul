# AI-Assisted Animation Pipeline

Bosses, minions, and Town NPCs need actual moving sprite sheets, not a single flat icon — this doc covers that, separately from the static icon/idle-pose prompts in `SPRITE_PROMPTS.md`.

**Code reality check first**, since it changes what "done" means per category:
- **Town NPCs are done** — see below. All 4 have real nano-banana-sourced idle+walking art and a custom `FindFrame()` (vanilla's `AnimationType = NPCID.Guide`/25-slot layout was dropped, since our 2-row sheets don't match its frame semantics).
- **Bosses are done** — see below. All 7 have real nano-banana-sourced art, `Main.npcFrameCount`/`FindFrame()`/`GetAlpha()` wired up, verified via clean `dotnet build` + clean headless smoke-test load.
- **Minions are done** — see below. All 6 (4 originals + 2 new hound variants) have real art and hand-rolled `AI()`-driven frame advance (`ModProjectile` has no `FindFrame()` hook).
- **Projectiles are done** — see below. All 9 chakra/jutsu effects have real art and hand-rolled `AI()`-driven frame advance, either looping or one-shot/timeLeft-synced depending on the effect.
- **Burst Effects are done** — see below. 15 element/damage-family animated one-shot bursts replace almost every plain-Dust `ChakraVFX.SpawnBurst` call across the mod.
- **`ShadowCloneProjectile` needs no art at all.** It already renders the owner's real, fully-animated player character via `Main.PlayerRenderer.DrawPlayer`, falling back to its flat placeholder only if that fails. Leave it alone.

## The pipeline (as actually run for Bosses)

The originally-planned **3D model → rig → render frames** route (Hyper3D Rodin → Mixamo → Blender) was never executed — no access to those tools in this environment. What actually shipped instead:

1. **Sprite sheet, not single frames, straight from nano banana (Gemini image-gen).** One prompt per boss asks directly for a labeled multi-row sheet (e.g. "1. IDLE", "2. MELEE LUNGE", "3. RANGED CAST"), each row a real posed sequence — not one hero frame repeated. Image-edited from a canon reference for the idle row; later rows chained from that same result to keep the character consistent. Prompt requests a solid flat background in a color absent from that character's own palette (magenta or green, picked per-character — e.g. green for anyone with a purple/violet aura, since magenta reads too close to that hue) instead of "transparent background", since these models can't actually emit alpha and instead fake it with a baked-in checkerboard that's hard to key out cleanly.
2. **Slice the sheet.** Sheets are irregular — different frame counts per row, uneven cell widths, sometimes zero background gap between adjacent poses (an outstretched fist or trailing effect touching the next frame). Row/column boundaries were found from the pixel data (background-color-fraction bands for rows, content-density gaps for columns), not assumed to be a uniform grid, with a few frames needing hand-picked split points where two frames touched with no gap at all.
3. **Key the background out by hue, not brightness.** A sheet's background isn't always one flat color end to end — cell-divider lines or shading can render the same hue much darker. Keying on hue (R≈B and G much lower, for magenta) rather than a brightness threshold catches both in one pass without eating into real character pixels.
4. **Scale + ground consistently.** nano banana doesn't reliably hold one character scale across rows (e.g. Haku's whole cast row came out ~33% smaller than idle/melee for no pose-related reason). Each block's scale is normalized off its own reference frame, *unless* that reference frame is itself a genuine crouch/lunge pose (wider AND shorter, not just uniformly smaller) — in that case the whole character shares one scale so the pose isn't artificially stretched. Every frame is then re-centered on the same foot-baseline so the character doesn't visually jump size or position when the animation switches blocks.
5. **Wire up the frame-playback code.** Each boss's `SetDefaults()` sets `Main.npcFrameCount[Type]` to the real per-block frame counts (not a fixed count — blocks are different sizes per boss), and `FindFrame()` maps each `AttackState` to its block. Lunge/dash-style attacks (`Strike`, `Lunge`, `Charge`) play their block **once**, synced to that state's own tick constant (`StrikeTicks`, `LungeTicks`, `ChargeTicks`), instead of looping — they read as a discrete beat, not a repeating cycle. Idle/cast-style states loop continuously.
6. **Phase/element variation via `GetAlpha()`, not baked frames.** Rather than generating separate art per HP phase or elemental mask (which would multiply frame count for a purely cosmetic difference), each boss's `GetAlpha()` override tints the sprite based on its existing `CurrentPhase`/`CurrentElement` field — e.g. Kakuzu's 5 elemental masks reuse one cast animation, tinted per element.

## Bosses — Done

`Content/NPCs/Bosses/`. All 7 have real art + working frame-playback code (see pipeline above).

| Boss | Canvas (per frame) | Blocks (frame counts) | Phase/element handling |
|---|---|---|---|
| HakuBoss | 104x56 | idle(10) / melee(7, plays once) / cast(8) | Phase 2 = brighter icy-blue tint |
| KaguyaBoss | 73x56 | idle(4) / cast(7) / telegraph(4) | Escalate/Final = increasing violet tint |
| KakuzuBoss | 62x56 | idle(4) / cast(7) | 5 elemental masks = per-element tint (fire/wind/lightning/earth/core) |
| MadaraBoss | 52x56 | idle(5) / melee(4, plays once) / cast(4) | Susanoo = existing `NPC.scale` growth + purple tint (one shared rig, not a second model) |
| OrochimaruBoss | 78x56 | idle(4) / melee(6, plays once) / cast(6) | Phase 2/3 = increasing sickly-purple tint |
| PainBoss | 85x56 | idle(4) / attack(6) | No tint — the 6 Paths are already differentiated by projectile VFX, not pose |
| TailedBeastBoss (Shukaku) | 77x56 | idle/hover(8) / chase(8) / charge(7, plays once) | Phase 2/3 = increasing blue chakra-cloak tint |

## Town NPCs — Done

`Content/NPCs/Town/`. Scoped down to **idle + walking only** (no talk/gesture row) — sufficient for a passive town NPC. Same nano-banana pipeline as Bosses (see above), with one added wrinkle: several of these sheets packed a row's frames into a 2-row sub-grid (e.g. Itachi's 8 walking frames as a 2x4 block) rather than one long row, so those blocks were assembled from 2 separate crops concatenated in order.

| NPC | Canvas (per frame) | Blocks (frame counts) |
|---|---|---|
| KakashiNPC | 37x40 | idle(6) / walking(7) |
| ItachiNPC | 35x40 | idle(5) / walking(8, from a 2x4 sub-grid) |
| GuySenseiNPC | 43x40 | idle(4) / walking(6, from a 2x3 sub-grid) |
| ShinobiVendorNPC | 42x40 | idle(8, from a 2x4 sub-grid) / walking(8, from a 2x4 sub-grid) |

`FindFrame()` switches idle/walk blocks off `NPC.velocity.X != 0` rather than reading any AI state (these are passive vendors, `NPCAIStyleID.Passive` drives movement already).

## Minions — Done

`Content/Minions/`. The "not worth the 3D pipeline overhead" call in the original version of this doc was specifically about Rodin/Mixamo/Blender being overkill for 26x20px creatures — it doesn't apply to the much cheaper direct-nano-banana-sheet approach used for Bosses/Town NPCs, so these got animated too. Scoped to **idle/follow + attack/chase** (2 blocks), matching each minion's existing binary AI branch in the shared `AnimalMinionProjectile` base class (`FollowOwner` vs `AttackTarget`).

`ShadowCloneProjectile` still needs no art (renders the owner's real player character, see the code-reality note at the top).

Unlike `ModNPC`, `ModProjectile` has no `FindFrame()` hook — animation is driven by hand from `AI()` (`AnimalMinionProjectile.UpdateAnimationFrame`), with `Projectile.frame` set directly as a frame index (not a `Rectangle`, unlike `NPC.frame`).

| Minion | Canvas (per frame) | Blocks (frame counts) |
|---|---|---|
| NinjaHoundMinionProjectile | 111x56 | idle(6) / attack(8) |
| SlugMinionProjectile | 139x56 | idle(4) / attack(12) |
| SnakeMinionProjectile | 92x56 | idle(5) / attack(8) |
| ToadMinionProjectile | 116x56 | idle(4) / attack(6) |

Two new hound variants were added alongside the original (player wanted all 3 generated hound designs usable via separate summoning tools, each its own minion/buff/item rather than a reskin):

| Minion | Canvas (per frame) | Blocks (frame counts) | Flavor |
|---|---|---|---|
| BulldogHoundMinionProjectile | 98x56 | idle(6) / attack(9) | Slower, hits harder |
| ScoutHoundMinionProjectile | 98x56 | idle(6) / attack(9) | Longest attack range, hits softest |

## Projectiles — Done

`Content/Projectiles/`. Scoped to the 9 chakra/jutsu effects (not the 6 thrown-weapon projectiles —
Kunai, Shuriken, Fuma Shuriken, Explosive Kunai, Paper Bomb, boss-only Senbon — which stay
single-frame since they already spin in flight via code, not frame-swapping; see `SPRITE_PROMPTS.md`
for their separate art-quality-only pass). Same nano-banana-sheet pipeline as Bosses/Town
NPCs/Minions, with a floating-VFX wrinkle: frames are **center-grounded** rather than foot-grounded
(`process_fx.py`, a variant of the boss/NPC slicing pipeline), since these have no "feet" concept.

Two prompt/slicing issues came up specifically here:
- **Vague per-frame phrasing produces near-duplicate frames.** Water Dragon and Rasengan both came back the first time with all frames looking almost identical, because the prompt just asked for poses to "ripple/flicker slightly differently." Rewriting with explicit, numbered, distinct per-frame descriptions (exact curve shape, rotation angle, wisp position) fixed both.
- **Frame bounding boxes can overlap even when the actual art doesn't touch.** Water Dragon's bottom-row coiled frame and the extended frame next to it have bounding boxes that overlap in x (a curvy dragon's neck reaches further right at the top than its tail does at the bottom, while the neighboring frame's curl starts in that same x-range one row down) — handled by cropping generously then masking out the contaminating corner rather than trying to find one clean rectangular split.

Like Minions, `ModProjectile` has no `FindFrame()` hook, so each class hand-rolls its own frame advance in `AI()`. Two timing patterns are used:
- **Looping** (Rasengan, Chidori, ElementalBolt, Fireball, GenjutsuIllusion, GenjutsuSleep, Susanoo, WaterDragon): a simple tick-counter cycling through all frames continuously for as long as the projectile is alive, layered on top of each projectile's existing motion/rotation logic rather than replacing it. WaterDragon's old placeholder whole-sprite rotation was removed instead, since a directional creature with distinct head/tail poses shouldn't spin like a symmetric orb — it flips via `spriteDirection` and cycles poses in place instead.
- **One-shot, timeLeft-synced** (ShinraTensei): its 6 frames are an expanding ring (tiny → huge, then fading), so `Projectile.frame` is computed directly from `LifetimeTicks - Projectile.timeLeft` rather than an independent counter — the visual ring size always matches how far into its short 12-tick life it is.

| Projectile | Canvas (per frame) | Frames | Timing |
|---|---|---|---|
| RasenganProjectile | 32x32 | 9 | Loop, 4 ticks/frame |
| ChidoriProjectile | 24x24 | 6 | Loop, 4 ticks/frame |
| ElementalBoltProjectile | 18x18 | 7 | Loop, 6 ticks/frame |
| FireballProjectile | 20x20 | 6 | Loop, 6 ticks/frame |
| GenjutsuIllusionProjectile | 16x16 | 10 | Loop, 4 ticks/frame |
| GenjutsuSleepProjectile | 16x16 | 6 | Loop, 6 ticks/frame |
| SusanooProjectile | 170x130 | 6 | Loop, 8 ticks/frame |
| WaterDragonProjectile | 130x130 | 6 | Loop, 8 ticks/frame |
| ShinraTenseiProjectile | 220x220 | 6 | One-shot, synced to 12-tick lifetime |

## Burst Effects — Done

`Content/Projectiles/Bursts/`, driven by `Common/VFX/BurstEffectProjectile.cs`. Replaces almost every `ChakraVFX.SpawnBurst`/`SpawnDirectionalBurst` (plain Dust) call across the mod with a real animated sprite: one `BurstEffectProjectile` subclass per element/damage family, each a short one-shot expand-and-fade animation (same `LifetimeTicks`-synced frame-advance pattern as `ShinraTenseiProjectile`, purely cosmetic — `CanDamage()` returns false). The 2 remaining plain-Dust `SpawnBurst` calls (`TailedBeastBoss`/`KakuzuBoss` HitEffect) are generic damage-taken blood feedback, not a jutsu/element effect, so they were deliberately left alone.

Art here came from nano banana prompts run by the player directly (not through this session's image-gen access), then sliced/keyed with the same Python pipeline as everything else above. The first pass had two wrinkles, both since fixed by a targeted re-roll of the 5 affected families (Chakra, Genjutsu, Healing, Wood, Earth):
- **Style drift**: those 5 families first rendered as soft photorealistic glow renders rather than strict pixel art, despite "not photorealistic, pixel art sprite" prompt language. Fixed by pushing the prompt much harder on the point ("like a hand-drawn 16-bit SNES asset... every shape built from visible flat-color square pixel blocks with hard jagged edges... no soft blur, no lens-flare, no airbrushed glow") — the reroll came back as genuine crisp pixel art.
- **Hue-collision on fade frames**: a burst's last ("almost gone") frame is, by design, a pale/desaturated version of its own color — for Healing, Wood, and Earth specifically that pale color drifted close enough to the magenta keying background that the hue mask couldn't cleanly separate it, leaving a flat-colored blob instead of a proper fade. Fixed by (a) swapping that family's background to a color nowhere near its own palette (Healing/Earth → blue, Wood → green, since both are far from green/gold/white and brown respectively) and (b) explicitly instructing the prompt that the faintest frame must stay recognizably tinted, never fade to white/pink. `gridtools.py` also gained a `blue` hue mask (`b > 100 and b > r*1.4 and b > g*1.4`) alongside magenta/green/orange to support this.

Netcode: unlike Dust (always client-local, never synced), these are real `Projectile.NewProjectile` spawns, so every call site would double-spawn per client in multiplayer if unguarded — same reasoning already applied to this mod's NPC-spawned jutsu projectiles. The guard is centralized once in `ChakraVFX.SpawnBurstEffect<T>` (`if (Main.netMode == NetmodeID.MultiplayerClient) return;`) so every one of the ~54 call sites stays a plain one-liner like `ChakraVFX.SpawnFireBurst(position, scale)`.

| Family | Canvas (per frame) | Frames | Replaces |
|---|---|---|---|
| ChakraBurst | 28x28 | 5 | `DustID.BlueTorch` (ninjutsu default) |
| GenjutsuBurst | 28x28 | 5 | `DustID.PurpleTorch` |
| FireBurst | 28x28 | 5 | `DustID.Torch` |
| LightningBurst | 24x24 | 4 | `DustID.Electric` |
| WindBurst | 28x28 | 5 | `DustID.Cloud` |
| EarthBurst | 26x26 | 5 | `DustID.Stone` |
| CoreBurst | 26x26 | 6 | `DustID.Shadowflame` |
| WaterBurst | 28x28 | 5 | `DustID.Water` |
| IceBurst | 28x28 | 5 | `DustID.IceTorch` |
| BoneBurst | 28x28 | 5 | `DustID.WhiteTorch` + `DustID.Bone` (Kaguya + Ash Bone share one white/bone-dimensional palette) |
| CorruptionBurst | 28x28 | 5 | `DustID.Corruption` |
| SmokeBurst | 28x28 | 5 | `DustID.Smoke` |
| SharinganBurst | 26x26 | 5 | `DustID.RedTorch` |
| HealingBurst | 28x28 | 5 | `DustID.HealingPlus` |
| WoodBurst | 28x28 | 5 | `DustID.WoodFurniture` |

`ElementalBoltProjectile` and `KakuzuBoss`'s mask-transition flash both pick the right family at runtime off the existing `Element` enum (`SpawnElementBurst`/inline switch) rather than a 6th hardcoded call, so Kakuzu's mask-swap flash now actually matches the element it's transitioning into (previously always `Torch` regardless of the new mask — a minor pre-existing inconsistency fixed as part of this pass).
