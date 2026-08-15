# NarutoOverhaul Manual Test Checklist

## Done (tested & working)

**Genjutsu (full class)** - Sleep, Illusion, Nightmare, scare/control tile-clipping fix, armor, Emblem, Veil, Mastery Scroll.

**Ninjutsu (full class)**
- Shadow Clone (visibility + position fixed)
- Water Dragon (size, trail, floor-collision fixed)
- Fireball
- Great Breakthrough
- Rasengan
- Chidori
- All-Killing Ash Bones
- Ninjutsu armor set, Emblem, Focus Seal accessory, Mastery Scroll

**Taijutsu (non-weapon)**
- Taijutsu armor set, Emblem, Wraps accessory, Mastery Scroll

**Throwing weapons (class-agnostic, craftable - no story gate)**
- Kunai (resized, rotation fixed to point at target, floor-collision fixed)
- Explosive Kunai (same fixes as Kunai; no longer detonates instantly on throw)
- Shuriken
- Fuma Shuriken (resized 3x)
- Paper Bomb (now sticks to enemies like a sticky bomb, rides along, explodes on fuse)

**Accessories**
- Chakra Paper
- Chakra Wings (bigger + recolored blue, sprite only shows while actively flying, flight now
  chakra-bound instead of a fixed timer, no regen in midair while equipped)
- Illusion Charm
- Monstrous Strength Gloves
- Weighted Leg Warmers
- Substitution Scroll (log-drop animation added, 2s visible before fading)
- Sharingan Awakening (new consumable - Sharingan's dodge passive is no longer a silent unlock on
  downing Haku, now requires drinking this)

**Transformations / forms (full class - behavior validated, art still outstanding)**
- Kamui Phase (true intangibility to NPCs/projectiles, sprite only shows while actively flying,
  chakra-bound, no wall-occlusion fix yet - see NOTE below)
- Chakra Control (wall/tree climbing now uses vanilla's real Climbing Claws mechanic
  (`Player.spikedBoots`) instead of the old broken manual collision-box heuristic)
- Byakugan, Sage Mode, Six Paths Sage Mode, Tailed Beast Mode, Curse Mark, Eight Gates,
  Sharingan Focus buff - all validated in-game 2026-08-11

NOTE: all 8 forms still share one flat translucent orange square placeholder above the head
(`Common/PlayerDrawLayers/SageModeDrawLayer.cs`) instead of real per-form animated art - explicitly
deferred until behavior across all forms is solid first (now confirmed solid). Kamui Phase also
still visually clips into/behind solid tiles while phasing (cosmetic only, deferred alongside the
aura art). Real per-form art is now the only remaining item for this class.

**World / tiles (partial - see Testing now for the rest)**
- Hiraishin Seal tile - now placed by throwing a Minato Kunai (`MinatoKunaiItem`/
  `MinatoKunaiProjectile`) that arcs under gravity and plants the seal on impact, instead of
  instant vanilla-furniture-style placement. Mining a seal now correctly removes it from the warp
  network (was silently not working - `KillMultiTile` never fires for a 1x1 tile).
- Ramen Stand tile - validated 2026-08-13. Its proximity effect now grants a dedicated
  `RamenComfortBuff` (small life regen, no timer shown, like vanilla's Sunflower) instead of reusing
  vanilla's `WellFed` - the old shared-ID version was continuously wiping out a real 10-minute Well
  Fed buff from eating actual food down to a fraction of a second every tick spent nearby.

**Village headbands** - all 5 validated 2026-08-13 (given via test kit, vanity-only by design - no
stats). Leaf and Mist were recolored (forest green / deepened teal) after reading near-identical to
the Ninjutsu Helmet's navy palette at icon size; Cloud/Sand/Stone were already distinct.
- [x] Leaf
- [x] Cloud
- [x] Mist
- [x] Sand
- [x] Stone

**Summons / minions** (renamed to character names: Manda/Gamabunta/Katsuyu/Pakkun/Bull/Bisuke -
see Localization). Custom hand-written `AI()` (no vanilla `aiStyle` number), ground-based via
`tileCollide=true` + `Main.projPet=true` + manual gravity/`Collision.StepUp`/wall-hop (borrowed
from vanilla `Projectile.AI_026`, the style Pygmy/Parrot use). Fixed across several passes: no
longer fall through tiles, no longer sink into the ground, no longer get stuck on 1-2 tile walls
(custom `PreDraw` now anchors each sheet's feet to the hitbox bottom instead of vanilla's default
hitbox-center anchor).
- [x] Manda (was Snake)
- [x] Gamabunta (was Toad)
- [x] Katsuyu (was Slug)
- [x] Pakkun (was Ninja Hound)
- [x] Bull (was Bulldog Hound)
- [x] Bisuke (was Scout Hound)

**Bosses (fight + drops + summon items)** - all 7 validated in-game 2026-08-13 after several fix
passes: stale-facing/"walking backwards" fixes, 2x-3x sizing with proper hitbox/PreDraw anchoring,
new leap/jump attacks (Shukaku/Orochimaru/Kakuzu), safe-teleport landing checks (no more
substitution/teleport into terrain), universal 3-second stuck-escape (repositions to a clear spot
without disabling ground collision), Kakuzu/Madara/Kaguya given distinct projectiles instead of
sharing one, Pain's town-NPC "almost invincible" defense mechanic + Deva pull/push/gravity-flip
rework, Madara's Susanoo made actually visible (tint/scale/recurring pulse), and Kaguya's new
dimension-shift attack (banishes both her and the player together to a real biome, including the
Underworld, cycling through all before repeating) plus a Final-phase return-to-origin + gravity-slam
hazard.
- [x] Haku
- [x] Kakuzu
- [x] Orochimaru
- [x] Pain
- [x] Madara
- [x] Kaguya
- [x] Shukaku / Tailed Beast Boss

**Consumables**
- Chakra/Stamina potions + regen potions - validated 2026-08-13. Added `ChakraPotionSicknessDebuff`/
  `StaminaPotionSicknessDebuff` (real countdown timer, unlike Ramen Comfort's) so the existing
  repeated-use-diminishing-returns mechanic is now visible instead of a silent internal stack
  counter.
- Ramen - validated 2026-08-13 as part of tracking down the Ramen Stand buff conflict (see World /
  tiles) - eating it now reliably grants the full 10-minute Well Fed buff even standing next to the
  stand.
- Otsutsuki Chakra Fragment - validated 2026-08-13 against a real Moon Lord fight. Reworked from a
  flat damage/defense debuff (tooltip used to spell out "weakens Moon Lord") into homing assist
  bolts (`KaguyaBlessingBoltProjectile`/`MoonLordAssistGlobalNPC`) fired at whichever Moon Lord part
  is active, tuned down after live testing to 100 damage every 6 seconds per part; tooltip no longer
  hints at what the blessing does.
- Chakra/Stamina numbered scrolls (1-7 each) - validated 2026-08-13.
- Shinobi Handbook - validated 2026-08-13.
- Cursed Seal Fragment - validated 2026-08-13.

**Town NPCs** - all 4 validated 2026-08-14. Fixed along the way: minimap head icons were missing
(`_Head.png` art existed but classes lacked `[AutoloadHead]`); walking left showed the body frozen
facing right (`AI_007_TownEntities`, vanilla's own town-NPC AI, doesn't reliably update
`NPC.direction`/`spriteDirection` for these house-less test-spawned NPCs - now forced from velocity
each tick in a new `PostAI()`, with `spriteDirection`'s flip sign the *inverse* of `direction` for
this art, confirmed empirically); walk-cycle animation flickered back to frame 0 most ticks (a plain
`velocity.X != 0f` check flickers through vanilla's deceleration - debounced with a small threshold).
Also resized all 4 to 1.5x. Note: Guy Sensei/Itachi/Ten Ten's walk-cycle art itself still reads as
weak/not-clearly-walking even with the animation logic fixed - a known art-quality gap, not a code
bug (Kakashi's sheet is fine). Held off on regenerating per your call 2026-08-13.
- [x] Guy Sensei
- [x] Kakashi
- [x] Itachi
- [x] Ten Ten (was "Shinobi Vendor")

**Boss head icons** - all 7 validated 2026-08-14 (`[AutoloadBossHead]` + generated portrait art;
health bar/minimap icon was missing entirely before).
- [x] Haku
- [x] Kakuzu
- [x] Orochimaru
- [x] Pain
- [x] Madara
- [x] Kaguya
- [x] Shukaku / Tailed Beast Boss

## Known broken - needs fixing (don't test yet, do fix)

**Taijutsu weapons**
- Gentle Fist
- Iron Leg
- Leaf Hurricane
- Front Lotus

**Art quality gaps (not blocking, low priority)**
- Guy Sensei / Itachi / Ten Ten walk-cycle sprite sheets don't read clearly as "walking" - Kakashi's
  sheet is the good reference for what these should look like if regenerated.

## Testing now

Nothing queued - everything above is either Done or in Known broken. Next work is fixing the
Taijutsu weapons bug (see Known broken) so they can move into this section.
