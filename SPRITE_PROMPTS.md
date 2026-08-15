# Sprite Art Prompts

Every sprite in the mod is currently a placeholder (flat-colored PNG, a few hundred bytes). This file lists every single-frame sprite that needs real art, with a ready-to-paste prompt for Gemini/Nano Banana or Rodin.

## Workflow notes

- **Generate large, then downscale.** Ask for a big canvas (e.g. 1024x1024) — AI tools render tiny exact pixel grids poorly. Downscale to the "Canvas" size listed per row in an image editor (nearest-neighbor / point sampling, not smooth resampling) before importing into the mod.
- **Every prompt already includes "sprite" and a background spec** per row — no need to add it yourself, just paste as-is.
- **Never ask for "transparent background" literally.** Gemini/nano banana can't emit real alpha — it fakes "transparent" with an opaque, baked-in gray checkerboard-look background regardless of wording (confirmed via raw pixel dumps, see `tools/convert_gemini_sprite.sh`'s header comment), which is a worse keying target than a real flat color. Rows added since this was caught instead ask for an explicit **solid flat background color chosen to be nowhere near the subject's own palette** (e.g. green for the grey-steel/tan/red/blue thrown-weapon batch, since none of those items contain any green), then key that exact color out afterward with `tools/convert_gemini_sprite.sh <input> <output> <w> <h> "<bg_color>" <fuzz>%` (or `tools/prepare_sprite.sh` for a plain, non-nano-banana source). Pick the color per-batch, not globally — reuse magenta/green the way `ANIMATION_PIPELINE.md`'s boss/burst sheets do, switching to whichever of the two is farther from that batch's actual colors.
- **Global style guide** (apply mentally to every prompt, not repeated in full each row): hand-painted pixel art matching Terraria's native item/NPC aesthetic, bold clean dark outlines, saturated colors, single object isolated and centered in frame, no drop shadow, no ground/scenery/background elements.
- **Say "not photorealistic" explicitly, in strong terms, or it drifts anyway.** Learned the hard way on the Burst Effects batch (see `ANIMATION_PIPELINE.md`'s Style drift note): a mild "pixel art sprite" suffix alone still came back as a soft photorealistic glow render. Rows added since then spell it out per-prompt: "...pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, solid flat green background, no checkered pattern."
- **`tools/convert_gemini_sprite.sh` had two real bugs, both fixed 2026-07-21 while processing the thrown-weapon batch.** (1) Its watermark-corner blanking used `-channel A -fill black -draw rectangle ... +channel`, which reads as "set alpha to 0 there" but under ImageMagick 7 actually draws a fully **opaque** black rectangle instead — the opposite of intended, and it silently wrecked the subsequent `-trim`/resize (the bogus opaque corner got treated as real content). Now uses `-region W×H+X+Y -evaluate set 0 +region` instead, which is IM7-correct. (2) A `read` from `convert ... info:` (no trailing newline) reported failure even after successfully populating its variables — a standard bash `read`-at-EOF gotcha — which killed the whole script under `set -e`; fixed with `|| true` on that line. If either symptom resurfaces (a black square baked into a processed sprite, or the script exiting with no output and no error), check these two spots first.
- **Color-keying alone leaves "spill"** — opaque edge/anti-aliasing pixels blended partway toward the background hue, which no `fuzz%` value fixes (too low leaves it, too high eats real edge detail before the spill clears, since a fuzz raise treats *all* pixels within that color-distance the same). `convert_gemini_sprite.sh` now does a second pass after the resize: a **hue-based** filter (auto-detected from `bg_color`'s dominant channel) that drops any pixel whose color leans the same direction as the background, cheap to do once the image is already down to its tiny final size.
- **A decorative element sharing the background's hue family defeats keying entirely, at any fuzz.** Hit this on `ShurikenProjectile.png`: the "motion-blur spin lines" rendered as pale sage-green rings around the star — same hue family as the green background — so no fuzz/spill-cleanup setting could separate "ring" from "background" from "grey star" (they overlap in color space). Fixed with the script's new optional `crop=WxH+X+Y` parameter: found the tight bounding box by trimming with a much higher throwaway fuzz (which happens to erase the same-hue decorative element too, giving a clean box read off `identify -format "%wx%h%O"`), then applied that exact box as a hard crop on the real (lower-fuzz) keyed image instead of relying on auto-`-trim`.
- **Chakra-nature color language**, kept consistent across related entries: Fire (Katon) = orange/red, Water (Suiton) = blue/cyan, Wind (Fuuton) = white/pale green, Lightning (Raiton) = electric blue/yellow-white, Genjutsu = purple/magenta, Taijutsu = red/orange, Ninjutsu = chakra-blue, Sage/nature = green-orange, Rinnegan/Otsutsuki = violet with ripple-eye motifs.
- **Rarity column is a glow-intensity hint**, not a literal paint color: White/no rarity = no glow, Blue/Green = faint shimmer, Orange = warm glow, LightRed = noticeable pink-red glow, Red = strong glow, Pink/Purple = colored glow matching the name, Expert = dark red-black trim.

---

## Weapons — Jutsu - Done

`Content/Items/Weapons/Jutsu/`

| File | Canvas | Rarity | Prompt |
|---|---|---|---|
| AllKillingAshBonesItem.png | 30x30 | Red | Pale bone-white spear made of jagged bundled bone spikes, faint purple Rinnegan-tinted chakra aura along the shaft, strong red rarity glow outline, as a 2D game item icon sprite, transparent background. |
| ChidoriItem.png | 34x34 | Orange | A clenched fist wreathed in crackling blue-white lightning, chirping electric arcs and sparks radiating outward, warm orange rarity glow, as a 2D game item icon sprite, transparent background. |
| FireballItem.png | 30x30 | LightRed | A compact swirling orange-red fireball with flame licks, ember particles, and a hot white core, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| FrontLotusItem.png | 30x30 | Orange | A leg mid spinning kick wrapped in a red-orange chakra silhouette with dynamic speed lines behind it, warm orange rarity glow, as a 2D game item icon sprite, transparent background. |
| GenjutsuIllusionItem.png | 28x28 | LightRed | A hypnotic purple-magenta spiral eye motif with wisps of violet smoke curling around it, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| GenjutsuNightmareItem.png | 40x40 | LightRed | A twisted screaming-face illusion mask half-formed from black smoke with jagged crimson eye highlights, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| GenjutsuSleepItem.png | 28x28 | LightRed | A soft indigo-purple crescent moon with drifting sleep-dust "Z" particles around it, dreamy glow, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| GentleFistItem.png | 26x26 | LightRed | A pale lavender-white open palm strike with small radiating chakra-point pressure rings, soft white-blue glow, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| GreatBreakthroughItem.png | 32x22 | LightRed | A crescent blade of compressed white-green wind with sharp cutting air-slash lines, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| IronLegItem.png | 30x30 | LightRed | A stone-grey armored shin and boot mid low sweeping kick, small cracked ground fragments and dust burst, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| LeafHurricaneItem.png | 44x44 | LightRed | A dynamic circular motion-blur silhouette of a spinning heel-drop kick with swirling green leaf particles, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| RasenganItem.png | 38x38 | LightRed | A swirling blue spiral sphere of chakra with visible inward-spinning ribbon layers, bright white core, small chakra wisps peeling off the edges, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| ShadowCloneItem.png | 30x30 | LightRed | Two overlapping translucent blue-tinted ninja silhouettes forming a hand seal together, faint smoke-poof particles, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| ShinraTenseiItem.png | 36x36 | Red | A dark violet-black repulsive shockwave ring radiating from a glowing Rinnegan eye at its center, small debris fragments flying outward, strong red rarity glow, as a 2D game item icon sprite, transparent background. |
| SusanooItem.png | 40x40 | Red | A giant translucent purple ethereal ribcage and armored fist reaching forward, glowing rib outlines, strong red rarity glow, as a 2D game item icon sprite, transparent background. |
| WaterDragonItem.png | 34x34 | LightRed | A curling serpentine dragon made of flowing blue water with foam highlights and droplets, jaws open, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |

## Weapons — Tools & Summon Scrolls - Done

`Content/Items/Weapons/`. All 5 thrown-weapon icons already have real distinct art (checked visually,
not just by file size) — rows below are retroactive documentation, not a pending prompt batch. The
matching in-flight **projectile** sprites (`Content/Projectiles/`, see the Projectiles table) got
their own real-art pass on 2026-07-21 using these same prompts.

| File | Canvas | Rarity | Prompt |
|---|---|---|---|
| ExplosiveKunaiItem.png | 26x26 | Blue | A steel kunai throwing knife with a ring pommel, a red explosive paper tag with kanji marking and a lit fuse wrapped around the grip, plain metallic shading, faint blue shimmer, pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, as a 2D game item icon sprite, solid flat green background, no checkered pattern, no gradient. |
| FumaShurikenItem.png | 32x32 | Blue | A large heavy black-iron fuma shuriken with four long curved folding blades and a spiked center hub, plain dull metallic shading, faint blue shimmer, pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, as a 2D game item icon sprite, solid flat green background, no checkered pattern, no gradient. |
| KunaiItem.png | 26x26 | White | A simple steel kunai throwing knife with a ring pommel and wrapped grip, plain metallic shading, no glow, pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, as a 2D game item icon sprite, solid flat green background, no checkered pattern, no gradient. |
| PaperBombItem.png | 20x28 | Blue | A small folded tan paper explosive tag bound with thin cord, red kanji-style ink markings, a coiled unlit fuse, plain paper shading, faint blue shimmer, pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, as a 2D game item icon sprite, solid flat green background, no checkered pattern, no gradient. |
| ShurikenItem.png | 26x26 | White | A four-pointed steel shuriken throwing star with sharp beveled edges, plain metallic shading, no glow, pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, as a 2D game item icon sprite, solid flat green background, no checkered pattern, no gradient. |
| NinjaHoundSummonScrollItem.png | 32x32 | Blue | A rolled tan parchment scroll tied with a blue ribbon, a paw-print seal stamp on the wrapper, faint blue shimmer, as a 2D game item icon sprite, transparent background. |
| SlugSummonScrollItem.png | 32x32 | Blue | A rolled tan parchment scroll, wrapper stamped with a spiral slug-shell sigil, faint blue shimmer, as a 2D game item icon sprite, transparent background. |
| SnakeSummonScrollItem.png | 32x32 | Blue | A rolled tan parchment scroll bound with purple cord, wrapper stamped with a coiled-snake sigil, faint blue shimmer, as a 2D game item icon sprite, transparent background. |
| ToadSummonScrollItem.png | 32x32 | Blue | A rolled tan parchment scroll, wrapper stamped with a toad sigil, faint blue shimmer, as a 2D game item icon sprite, transparent background. |

## Accessories - Done

`Content/Items/Accessories/`

| File | Canvas | Rarity | Prompt |
|---|---|---|---|
| ChakraPaperItem.png | 28x28 | LightRed | A single square of pale blue-white chakra-reactive paper, one corner crinkled and scorched from an elemental reaction, faint blue glow, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| ChakraWingsItem.png | 32x32 | Red | A small folded pair of glowing golden-white chakra wings pinned like a badge, radiant feather-light energy strands, strong red rarity glow, as a 2D game item icon sprite, transparent background. |
| GenjutsuEmblemItem.png | 28x28 | Pink | A circular violet clan emblem badge engraved with a stylized closed hypnotic eye, pink rarity glow, as a 2D game item icon sprite, transparent background. |
| GenjutsuVeilItem.png | 28x28 | LightRed | A sheer translucent purple-grey veil of cloth draped over an open hand, faint shimmer of concealment, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| IllusionCharmItem.png | 28x28 | LightRed | A small carved violet gemstone charm on a cord, faint swirling illusion-smoke aura around it, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| MonstrousStrengthGlovesItem.png | 28x28 | Green | Chunky reinforced brown-leather fingerless gloves with metal knuckle plates, faint green rarity glow, as a 2D game item icon sprite, transparent background. |
| NinjutsuEmblemItem.png | 28x28 | Pink | A circular blue clan emblem badge engraved with a spiral chakra symbol, pink rarity glow, as a 2D game item icon sprite, transparent background. |
| NinjutsuFocusSealItem.png | 28x28 | LightRed | A small paper seal tag covered in blue kanji-style ink markings with glowing chakra circuit lines, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| TaijutsuEmblemItem.png | 28x28 | Pink | A circular red-orange clan emblem badge engraved with a clenched-fist symbol, pink rarity glow, as a 2D game item icon sprite, transparent background. |
| TaijutsuWrapsItem.png | 28x28 | LightRed | Red-and-white bandage hand wraps coiled into a loop, worn training-cloth texture, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| WeightedLegWarmersItem.png | 28x28 | LightRed | Heavy grey iron-weighted leg warmers with metal buckles, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |

## Armor — item icons - Done

`Content/Items/Armor/`. Three matching sets (Genjutsu = purple illusionist robes, Ninjutsu = navy jonin gear, Taijutsu = green/orange training gear), plus the plain Leaf headband.

| File | Canvas | Rarity | Prompt |
|---|---|---|---|
| GenjutsuHelmetItem.png | 26x26 | Orange | A dark purple pointed hood with a violet eye-shaped emblem on the brow, warm orange rarity glow, as a 2D game item icon sprite, transparent background. |
| GenjutsuBodyItem.png | 26x26 | Orange | A flowing dark purple robe with violet trim and a hypnotic-spiral clasp, warm orange rarity glow, as a 2D game item icon sprite, transparent background. |
| GenjutsuLegsItem.png | 26x26 | Orange | Dark purple loose robe pants with violet wrap ties, warm orange rarity glow, as a 2D game item icon sprite, transparent background. |
| NinjutsuHelmetItem.png | 26x26 | Orange | A navy-blue forehead-protector style headband with a small chakra-spiral metal plate, warm orange rarity glow, as a 2D game item icon sprite, transparent background. |
| NinjutsuBodyItem.png | 26x26 | Orange | A dark navy flak-style jonin vest with pouches and a high collar, warm orange rarity glow, as a 2D game item icon sprite, transparent background. |
| NinjutsuLegsItem.png | 26x26 | Orange | Navy-blue shinobi pants with a kunai holster strap, warm orange rarity glow, as a 2D game item icon sprite, transparent background. |
| TaijutsuHelmetItem.png | 26x26 | Orange | A red cloth headband with a bold sun/flame emblem, warm orange rarity glow, as a 2D game item icon sprite, transparent background. |
| TaijutsuBodyItem.png | 26x26 | Orange | A green sleeveless training gi with an orange wide obi belt, warm orange rarity glow, as a 2D game item icon sprite, transparent background. |
| TaijutsuLegsItem.png | 26x26 | Orange | Green training pants with ankle wraps, warm orange rarity glow, as a 2D game item icon sprite, transparent background. |
| LeafVillageHeadbandItem.png | 26x26 | Blue | A navy-blue cloth headband with a polished steel plate engraved with a spiral-leaf insignia, faint blue rarity glow, as a 2D game item icon sprite, transparent background. |

## Armor — equip-layer art - Done

`Content/Items/Armor/` (`_Head`/`_Body`/`_Arms`/`_Legs` suffixes) and `Content/Items/Accessories/ChakraWingsItem_Wings.png`. These render directly ON the player model — describe the piece in isolation as a flat rig layer, not a full character.

| File | Canvas | Prompt |
|---|---|---|
| GenjutsuHelmetItem_Head.png | 40x40 | The dark purple pointed illusionist hood, front-facing head-layer game sprite worn on a humanoid character rig, transparent background. |
| GenjutsuBodyItem_Body.png | 40x56 | The dark purple illusionist robe torso with violet trim, front-facing body-layer game sprite for a humanoid character rig, transparent background. |
| GenjutsuBodyItem_Arms.png | 40x56 | Matching dark purple robe sleeves, front-facing arm-layer game sprite for a humanoid character rig, transparent background. |
| GenjutsuLegsItem_Legs.png | 40x56 | The dark purple robe pants with violet wrap ties, front-facing leg-layer game sprite for a humanoid character rig, transparent background. |
| NinjutsuHelmetItem_Head.png | 40x40 | The navy forehead-protector headband, front-facing head-layer game sprite worn on a humanoid character rig, transparent background. |
| NinjutsuBodyItem_Body.png | 40x56 | The navy flak-style jonin vest torso, front-facing body-layer game sprite for a humanoid character rig, transparent background. |
| NinjutsuBodyItem_Arms.png | 40x56 | Matching navy jonin vest sleeves, front-facing arm-layer game sprite for a humanoid character rig, transparent background. |
| NinjutsuLegsItem_Legs.png | 40x56 | The navy shinobi pants with a kunai holster strap, front-facing leg-layer game sprite for a humanoid character rig, transparent background. |
| TaijutsuHelmetItem_Head.png | 40x40 | The red flame-emblem headband, front-facing head-layer game sprite worn on a humanoid character rig, transparent background. |
| TaijutsuBodyItem_Body.png | 40x56 | The green sleeveless training gi torso with an orange obi belt, front-facing body-layer game sprite for a humanoid character rig, transparent background. |
| TaijutsuBodyItem_Arms.png | 40x56 | Bandage-wrapped arms with training gi shoulder straps, front-facing arm-layer game sprite for a humanoid character rig, transparent background. |
| TaijutsuLegsItem_Legs.png | 40x56 | The green training pants with ankle wraps, front-facing leg-layer game sprite for a humanoid character rig, transparent background. |
| LeafVillageHeadbandItem_Head.png | 40x40 | The navy Leaf Village headband with steel spiral-leaf plate, front-facing head-layer game sprite worn on a humanoid character rig, transparent background. |
| ChakraWingsItem_Wings.png | 40x40 | A full pair of glowing golden-white ethereal chakra wings spread open, radiant feather-like energy strands, back-attachment wing-layer game sprite for a humanoid character rig, transparent background. |

## Consumables - Done

`Content/Items/Consumables/`. Three icons are **shared by multiple items** — generate each once, not per item.

| File | Canvas | Rarity | Shared by | Prompt |
|---|---|---|---|---|
| ChakraCrystal.png | 28x36 | Blue | ChakraScroll1Item .. ChakraScroll7Item | A glowing faceted blue crystal shard pulsing with chakra energy, wisps of blue mist rising off it, faint blue rarity glow, as a 2D game item icon sprite, transparent background. |
| StaminaCrystal.png | 28x36 | Orange | StaminaScroll1Item .. StaminaScroll7Item | A glowing faceted amber-orange crystal shard pulsing with stamina energy, warm orange mist rising off it, orange rarity glow, as a 2D game item icon sprite, transparent background. |
| BossBagIcon.png | 32x32 | Expert | HakuBossBagItem, ShukakuBossBagItem, OrochimaruBossBagItem, KakuzuBossBagItem, PainBossBagItem, MadaraBossBagItem, KaguyaBossBagItem | A dark leather drawstring satchel bag with a small skull-and-bone expert-mode charm tied to the string, deep red-black trim glow, as a 2D game item icon sprite, transparent background. |

| File | Canvas | Rarity | Prompt |
|---|---|---|---|
| ChakraPotionItem.png | 28x36 | White | A small glass vial filled with glowing swirling blue liquid, cork stopper, plain glass shine, no glow, as a 2D game item icon sprite, transparent background. |
| ChakraRegenPotionItem.png | 28x36 | White | A taller glass vial with lighter cyan bubbling liquid and rising bubbles, cork stopper, no glow, as a 2D game item icon sprite, transparent background. |
| StaminaPotionItem.png | 28x36 | White | A small glass vial filled with glowing orange-amber liquid, cork stopper, no glow, as a 2D game item icon sprite, transparent background. |
| StaminaRegenPotionItem.png | 28x36 | White | A taller glass vial with lighter yellow-orange bubbling liquid and rising bubbles, cork stopper, no glow, as a 2D game item icon sprite, transparent background. |
| GenjutsuMasteryScrollItem.png | 32x32 | LightRed | An ornate sealed purple scroll with a wax seal bearing a hypnotic-eye sigil and hanging tassels, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| NinjutsuMasteryScrollItem.png | 32x32 | LightRed | An ornate sealed blue scroll with a wax seal bearing a spiral chakra sigil and hanging tassels, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| TaijutsuMasteryScrollItem.png | 32x32 | LightRed | An ornate sealed red-orange scroll with a wax seal bearing a fist sigil and hanging tassels, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| HakuSummonItem.png | 32x32 | LightRed | A jagged translucent icicle-shard charm bound with cord, faint frosty blue-white glow, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| KaguyaSummonItem.png | 32x32 | LightRed | A floating cracked crystal shard with a swirling violet dimensional rift visible inside the crack, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| KakuzuSummonItem.png | 32x32 | LightRed | A folded bounty-hunter contract paper with a dark thread stitched through it and a black wax seal, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| MadaraSummonItem.png | 32x32 | LightRed | A dark ritual paper talisman covered in glowing red kanji-style markings, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| OrochimaruSummonItem.png | 32x32 | LightRed | A small scroll marked with a purple three-tomoe cursed-seal insignia and a faint sickly purple aura, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| PainSummonItem.png | 32x32 | LightRed | A metal seal talisman engraved with a rippling concentric-circle Rinnegan eye pattern, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| TailedBeastSummonItem.png | 32x32 | LightRed | A tan cloth headband with a sand-village hourglass insignia plate and faint golden sand particles drifting off it, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| OtsutsukiChakraFragmentItem.png | 28x28 | Purple | A small radiant violet crystalline fragment with a swirling rainbow-white energy core and ornate ripple patterns, strong purple rarity glow, as a 2D game item icon sprite, transparent background. |
| RamenItem.png | 30x28 | White | A ceramic bowl of ramen noodles with visible broth, a pork slice, and green onion garnish, rising steam wisps, no glow, as a 2D game item icon sprite, transparent background. |
| ShinobiHandbookItem.png | 28x32 | White | A small worn tan leather-bound handbook with a red diagonal cover strap, no glow, as a 2D game item icon sprite, transparent background. |

## Materials - Done

`Content/Items/Materials/`

| File | Canvas | Rarity | Prompt |
|---|---|---|---|
| CursedSnakeFangItem.png | 26x26 | LightRed | A curved purple-tinted fang dripping a single venom drop, faint sickly purple aura, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| IceMirrorShardItem.png | 26x26 | LightRed | A jagged translucent icy-blue mirror shard reflecting a faint distorted glint, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| KakuzuHeartItem.png | 26x26 | LightRed | A dark red anatomical heart with visible dark thread stitching through it and a faint chakra-pulse glow, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| RinneganFragmentItem.png | 26x26 | LightRed | A shard of a violet eye with concentric ripple rings and a small tomoe mark, faint violet glow, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| SandCoreItem.png | 26x26 | LightRed | A cracked golden-tan sand orb with a faint teal chakra glow leaking from its cracks, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |
| SusanooCoreItem.png | 26x26 | LightRed | A glowing violet ribbed crystal core shaped like a miniature ribcage fragment, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |

## Placeable - Done

`Content/Items/Placeable/`

| File | Canvas | Rarity | Prompt |
|---|---|---|---|
| HiraishinSealItem.png | 18x18 | LightRed | A flat paper seal tag inked with a yellow-gold teleportation kanji circle, glowing amber lines, pink-red rarity glow, as a 2D game item icon sprite, transparent background. |

## Tiles - Done

`Content/Tiles/`. A small non-connecting marker tile (not a tileable terrain block) — a single flat icon-like sprite is correct here.

| File | Canvas | Rarity | Prompt |
|---|---|---|---|
| HiraishinSealTile.png | 16x16 | LightRed | A flat glowing paper seal tile inked with a yellow-gold teleportation kanji circle, faint amber glow lines, placed flush on the ground, pink-red rarity glow, as a 2D game tile sprite, transparent background. |

## Buffs - Done

`Content/Buffs/`

| File | Canvas | Prompt |
|---|---|---|
| SageModeBuff.png | 32x32 | Orange pigmented frog-like eye markings around closed eyes, faint green-orange nature-energy swirl behind, as a 2D game buff icon sprite, transparent background. |
| SharinganFocusBuff.png | 32x32 | A close-up red Sharingan eye with three black tomoe swirling around the pupil, faint red glow, as a 2D game buff icon sprite, transparent background. |
| TailedBeastModeBuff.png | 32x32 | A humanoid silhouette engulfed in bubbling orange-red chakra-cloak flames with a single visible tail, radiant heat glow, as a 2D game buff icon sprite, transparent background. |
| SixPathsSageModeBuff.png | 32x32 | A glowing rinnegan-eye icon with soft golden-white energy flames radiating outward in a halo ring, as a 2D game buff icon sprite, transparent background. |
| EightGatesBuff.png | 32x32 | A stylized acupuncture-gate symbol: a small circular chakra point burning bright red-orange with cracked skin lines radiating from it, as a 2D game buff icon sprite, transparent background. |
| GenjutsuControlDebuff.png | 32x32 | A small marionette puppet with thin purple chakra strings pulling its limbs, faint dark purple glow, as a 2D game debuff icon sprite, transparent background. |
| GenjutsuFearDebuff.png | 32x32 | A pale wide-eyed screaming face with jagged purple-black shadow tendrils closing in around it, as a 2D game debuff icon sprite, transparent background. |
| GenjutsuSleepDebuff.png | 32x32 | A closed eye with a soft indigo crescent moon and drifting violet "Z" sleep symbols, as a 2D game debuff icon sprite, transparent background. |
| NinjaHoundMinionBuff.png | 32x32 | A stylized brown-and-white ninja hound head wearing a tiny forehead-protector strap, as a 2D game buff icon sprite, transparent background. |
| SlugMinionBuff.png | 32x32 | A small pale-blue lavender slug with a glossy segmented shell-like back and a sparkle highlight, as a 2D game buff icon sprite, transparent background. |
| SnakeMinionBuff.png | 32x32 | A coiled purple-scaled snake head with yellow eyes and a faint venom drip, as a 2D game buff icon sprite, transparent background. |
| ToadMinionBuff.png | 32x32 | An orange-spotted toad with a plump body and a small ninja scarf detail, as a 2D game buff icon sprite, transparent background. |
| ChakraRegenBuff.png | 32x32 | A glowing blue spiral chakra-flow icon with small rising energy particles, as a 2D game buff icon sprite, transparent background. |
| StaminaRegenBuff.png | 32x32 | A small green upward-swirling wind-gust icon with light speed lines, as a 2D game buff icon sprite, transparent background. |
| ChakraControlBuff.png | 32x32 | A small blue footprint standing on a rippling water surface with faint chakra glow rings beneath it, as a 2D game buff icon sprite, transparent background. |
| KamuiPhaseBuff.png | 32x32 | A swirling purple dimensional vortex warping into a single point, faint concentric distortion rings, as a 2D game buff icon sprite, transparent background. |

## Projectiles - Done

`Content/Projectiles/`

| File | Canvas | Prompt |
|---|---|---|
| AshBoneProjectile.png | 10x32 | A slender pale bone spike/spear with a sharpened point and chalky grey-white texture, as a 2D game projectile sprite, transparent background. |
| ChidoriProjectile.png | 20x20 | A compact crackling ball of blue-white lightning with jagged electric arcs radiating outward, as a 2D game projectile sprite, transparent background. |
| ElementalBoltProjectile.png | 14x14 | A small glowing multicolor elemental energy bolt blending orange, blue, and green chakra streaks converging on a bright core, as a 2D game projectile sprite, transparent background. |
| ExplosiveKunaiProjectile.png | 8x24 | A thin steel kunai blade spinning in flight with a small red explosive paper tag and a lit fuse spark wrapped around the handle, metallic shine, pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, as a 2D game projectile sprite, solid flat green background, no checkered pattern, no gradient. |
| FireballProjectile.png | 16x16 | A small round fireball with orange-red flame licks and trailing embers, as a 2D game projectile sprite, transparent background. |
| FumaShurikenProjectile.png | 22x22 | A large heavy black-iron fuma shuriken with four long curved blades converging on a spiked center hub, motion-blur spin lines, dull metallic shine, pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, as a 2D game projectile sprite, solid flat green background, no checkered pattern, no gradient. |
| GenjutsuIllusionProjectile.png | 12x12 | A tiny swirling violet spiral wisp of hypnotic smoke, as a 2D game projectile sprite, transparent background. |
| GenjutsuSleepProjectile.png | 12x12 | A tiny drifting indigo dust-mote cluster with a faint crescent-moon sparkle, as a 2D game projectile sprite, transparent background. |
| GreatBreakthroughProjectile.png | 36x20 | A wide crescent blade of compressed white-green cutting wind, as a 2D game projectile sprite, transparent background. |
| KunaiProjectile.png | 8x24 | A thin steel kunai blade spinning in flight with a metallic shine, pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, as a 2D game projectile sprite, solid flat green background, no checkered pattern, no gradient. |
| PaperBombProjectile.png | 12x16 | A small folded tan paper explosive tag with red kanji-style ink markings and a short lit fuse trailing a spark, pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, as a 2D game projectile sprite, solid flat green background, no checkered pattern, no gradient. |
| RasenganProjectile.png | 28x28 | A swirling blue spiral chakra sphere with visible spinning ribbon layers and a bright white core, as a 2D game projectile sprite, transparent background. |
| SenbonProjectile.png | 4x20 | A tiny thin silver needle with a sharp glint, pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, as a 2D game projectile sprite, solid flat green background, no checkered pattern, no gradient. |
| ShinraTenseiProjectile.png | 220x220 | A massive dark violet-black repulsive shockwave ring expanding outward, cracked-air distortion lines, small floating debris, radial glow at the center, as a 2D game projectile sprite, transparent background. |
| ShurikenProjectile.png | 14x14 | A small spinning four-pointed steel shuriken star with motion-blur spin lines, pixel art sprite in the style of a hand-painted 16-bit SNES game asset, built from flat-color square pixel blocks with hard jagged outlines, not photorealistic, no soft blur or gradients, no lens-flare, as a 2D game projectile sprite, solid flat green background, no checkered pattern, no gradient. |
| SnakeProjectile.png | 10x26 | A small coiling purple-scaled snake body mid-strike with fangs bared, as a 2D game projectile sprite, transparent background. |
| SusanooProjectile.png | 64x64 | A large translucent purple ethereal ribcage and armored fist reaching forward, glowing rib outlines, trailing chakra wisps, as a 2D game projectile sprite, transparent background. |
| WaterDragonProjectile.png | 40x40 | A curling serpentine dragon head made of flowing blue water with foam highlights, jaws open, as a 2D game projectile sprite, transparent background. |

## Minions - Done

`Content/Minions/`

| File | Canvas | Prompt |
|---|---|---|
| NinjaHoundMinionProjectile.png | 26x20 | A small brown-and-white ninja hound in an alert standing pose with a tiny forehead-protector collar, as a 2D game creature sprite, transparent background. |
| ShadowCloneProjectile.png | 20x42 | A translucent blue-tinted ninja silhouette in a full standing pose with faint smoke wisps at the edges, as a 2D game creature sprite, transparent background. |
| SlugMinionProjectile.png | 34x22 | A pale lavender-blue slug with a glossy segmented shell-like back and small eye stalks, as a 2D game creature sprite, transparent background. |
| SnakeMinionProjectile.png | 32x18 | A coiled purple-scaled snake with yellow eyes and a forked tongue, as a 2D game creature sprite, transparent background. |
| ToadMinionProjectile.png | 30x24 | An orange-spotted toad in a plump squat stance with a small ninja scarf, as a 2D game creature sprite, transparent background. |

## Bosses — full sprite - Done

`Content/NPCs/Bosses/`. **Image-edit workflow, not text-to-image**: load a reference image of the canon character into Hyper3D's image editor and use the prompt as an edit instruction, not a from-scratch description. Keep the character's actual design (outfit, face, colors) from the reference image — the prompt only tells the editor how to restyle/pose/crop it. Currently single-frame placeholders — treat each prompt as the boss's idle/reference pose; the user will handle any additional attack/animation frames separately.

**These 7 static idle images are only step zero.** Bosses need real multi-frame animation (walk/attack/phase states) to actually play in-game — see `ANIMATION_PIPELINE.md` for the full image→3D→rig→render pipeline and the per-boss animation/state table. The images generated here work well as the reference image for that pipeline's Step 1 (Hyper3D Rodin image→3D), since they're already cropped and stylized.

| File | Canvas | Prompt |
|---|---|---|
| HakuBoss.png | 40x56 | Using the provided reference image of Haku, redraw him in a clean standing front-facing idle pose, restyle as bold 2D pixel-art matching Terraria's game-character aesthetic, keep his mask and light blue-white outfit, add faint ice-crystal particles near his feet, isolate the character with a transparent background, as a 2D game character sprite. |
| TailedBeastBoss.png | 100x100 | Using the provided reference image of Shukaku, redraw him in a clean standing/hovering front-facing pose, restyle as bold 2D pixel-art matching Terraria's game-character aesthetic, keep his sand-tanuki proportions and coloring, isolate the character with a transparent background, as a 2D game character sprite. |
| OrochimaruBoss.png | 44x60 | Using the provided reference image of Orochimaru, redraw him in a clean standing front-facing idle pose, restyle as bold 2D pixel-art matching Terraria's game-character aesthetic, keep his pale skin, snake eyes, and robe, add a faint purple cursed-chakra aura, isolate the character with a transparent background, as a 2D game character sprite. |
| KakuzuBoss.png | 46x58 | Using the provided reference image of Kakuzu, redraw him in a clean standing front-facing idle pose, restyle as bold 2D pixel-art matching Terraria's game-character aesthetic, keep his mask, stitched skin, and high-collar cloak, isolate the character with a transparent background, as a 2D game character sprite. |
| PainBoss.png | 44x62 | Using the provided reference image of Pain (Yahiko/Deva Path), redraw him in a clean standing front-facing idle pose, restyle as bold 2D pixel-art matching Terraria's game-character aesthetic, keep his orange hair, Rinnegan eyes, piercings, and black cloak with red clouds, isolate the character with a transparent background, as a 2D game character sprite. |
| MadaraBoss.png | 48x64 | Using the provided reference image of Madara Uchiha, redraw him in a clean standing front-facing idle pose, restyle as bold 2D pixel-art matching Terraria's game-character aesthetic, keep his armor, robe, and visible Sharingan eye, isolate the character with a transparent background, as a 2D game character sprite. |
| KaguyaBoss.png | 46x68 | Using the provided reference image of Kaguya Otsutsuki, redraw her in a clean standing front-facing idle pose, restyle as bold 2D pixel-art matching Terraria's game-character aesthetic, keep her white hair, third eye, and kimono, add a faint violet aura, isolate the character with a transparent background, as a 2D game character sprite. |

## Town NPC head icons - Done

`Content/NPCs/Town/*_Head.png`. These are the small bust icons used in the housing/minimap UI — not the full-body sprite. Same **image-edit workflow** as the bosses above: load a reference image of the character, use the prompt to crop/restyle it into a bust icon rather than describing the face from scratch.

| File | Canvas | Prompt |
|---|---|---|
| ShinobiVendorNPC_Head.png | 32x32 | Using the provided reference image of Tenten, crop to a front-facing head-and-shoulders bust, restyle as bold 2D pixel-art matching Terraria's NPC head-icon aesthetic, keep her twin-bun hairstyle and expression, isolate with a transparent background, as a 2D game NPC head icon sprite. |
| GuySenseiNPC_Head.png | 32x32 | Using the provided reference image of Might Guy, crop to a front-facing head-and-shoulders bust, restyle as bold 2D pixel-art matching Terraria's NPC head-icon aesthetic, keep his bowl-cut hair, thick eyebrows, and grin, isolate with a transparent background, as a 2D game NPC head icon sprite. |
| ItachiNPC_Head.png | 32x32 | Using the provided reference image of Itachi Uchiha, crop to a front-facing head-and-shoulders bust, restyle as bold 2D pixel-art matching Terraria's NPC head-icon aesthetic, keep his tied-back hair, tear-line marks, and Sharingan eyes, isolate with a transparent background, as a 2D game NPC head icon sprite. |
| KakashiNPC_Head.png | 32x32 | Using the provided reference image of Kakashi Hatake, crop to a front-facing head-and-shoulders bust, restyle as bold 2D pixel-art matching Terraria's NPC head-icon aesthetic, keep his silver spiky hair, mask, and tilted forehead protector over the Sharingan eye, isolate with a transparent background, as a 2D game NPC head icon sprite. |

---

## Out of scope: Town NPC body sheets

These 4 files are already 35-frame walk/idle/attack spritesheets (40x1400px each) — a single AI image prompt can't produce a coherent animated sheet, so they're not listed above. They need traditional frame-by-frame spriting (or a dedicated sprite-sheet animation tool), ideally reusing the matching head icon above as the character's face reference:

- `Content/NPCs/Town/GuySenseiNPC.png` (Might Guy)
- `Content/NPCs/Town/ItachiNPC.png` (Itachi Uchiha)
- `Content/NPCs/Town/KakashiNPC.png` (Kakashi Hatake)
- `Content/NPCs/Town/ShinobiVendorNPC.png` (Tenten)
