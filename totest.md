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

**Transformations / forms (partial - see Testing now for the rest)**
- Kamui Phase (true intangibility to NPCs/projectiles, sprite only shows while actively flying,
  chakra-bound, no wall-occlusion fix yet - see NOTE below)
- Chakra Control (wall/tree climbing now uses vanilla's real Climbing Claws mechanic
  (`Player.spikedBoots`) instead of the old broken manual collision-box heuristic)

NOTE: all 8 forms still share one flat translucent orange square placeholder above the head
(`Common/PlayerDrawLayers/SageModeDrawLayer.cs`) instead of real per-form animated art - explicitly
deferred until behavior across all forms is solid first. Kamui Phase also still visually clips
into/behind solid tiles while phasing (cosmetic only, deferred alongside the aura art).

**World / tiles (partial - see Testing now for the rest)**
- Hiraishin Seal tile - now placed by throwing a Minato Kunai (`MinatoKunaiItem`/
  `MinatoKunaiProjectile`) that arcs under gravity and plants the seal on impact, instead of
  instant vanilla-furniture-style placement. Mining a seal now correctly removes it from the warp
  network (was silently not working - `KillMultiTile` never fires for a 1x1 tile).

## Known broken - needs fixing (don't test yet, do fix)

**Taijutsu weapons**
- Gentle Fist
- Iron Leg
- Leaf Hurricane
- Front Lotus

## Testing now

**Transformations / forms**
- [ ] Byakugan
- [ ] Sage Mode
- [ ] Six Paths Sage Mode
- [ ] Tailed Beast Mode
- [ ] Curse Mark
- [ ] Eight Gates
- [ ] Sharingan Focus buff

**Summons / minions**
- [ ] Snake
- [ ] Toad
- [ ] Slug
- [ ] Ninja Hound
- [ ] Bulldog Hound
- [ ] Scout Hound

**Bosses (fight + drops + summon items)**
- [ ] Haku (also throws Senbon - only place that projectile is used)
- [ ] Kakuzu
- [ ] Orochimaru
- [ ] Pain
- [ ] Madara
- [ ] Kaguya
- [ ] Shukaku / Tailed Beast Boss

**NPCs**
- [ ] Guy Sensei
- [ ] Kakashi
- [ ] Itachi
- [ ] Shinobi Vendor

**World / tiles**
- [ ] Ramen Stand tile

**Village headbands**
- [ ] Leaf
- [ ] Cloud
- [ ] Mist
- [ ] Sand
- [ ] Stone

**Consumables**
- [ ] Chakra/Stamina potions + regen potions
- [ ] Chakra/Stamina numbered scrolls (1-7 each)
- [ ] Ramen
- [ ] Shinobi Handbook
- [ ] Otsutsuki Chakra Fragment, Cursed Seal Fragment
