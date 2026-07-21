# Animation Prompts — Bosses, Minions, Town NPCs

Copy-paste prompts for **Step 1** (Hyper3D Rodin: image → rigging-ready 3D model) of the pipeline in `ANIMATION_PIPELINE.md`. Steps 2 (Mixamo rig + clips) and 3 (Blender render) don't take text prompts — use the per-character Mixamo clip table already in `ANIMATION_PIPELINE.md` for those. A reusable Step 4 (pixelate/style-match) prompt is included at the bottom.

## Why these prompts differ from `SPRITE_PROMPTS.md`

The static prompts in `SPRITE_PROMPTS.md` ask for a posed, camera-ready 2D sprite. These ask for the opposite: a **neutral, symmetrical, T-pose/A-pose full-body model** — the pose Mixamo's auto-rigger expects. Feed each prompt into Hyper3D Rodin together with the character's existing reference image (or the static idle sprite already generated for the bosses) so the model keeps the established design/colors.

**Non-humanoid exception**: Mixamo's auto-rig only works on bipedal humanoids. Shukaku and all 4 minions are non-humanoid, so their 3D models (if you generate them at all) will need a hand-built skeleton in Blender instead of Mixamo — flagged per entry below. For minions specifically, `ANIMATION_PIPELINE.md` recommends skipping the 3D pipeline entirely and keeping the single static frame from `SPRITE_PROMPTS.md`; the prompts below are included only in case you want to attempt animating them anyway.

---

## Bosses

`Content/NPCs/Bosses/`. See `ANIMATION_PIPELINE.md`'s Bosses table for the Mixamo clips and per-boss phase/state handling once the model exists.

| Character | Rig type | Rodin prompt |
|---|---|---|
| Haku | Humanoid (Mixamo-ready) | Using the provided reference image of Haku, generate a full-body 3D model in a neutral T-pose (arms straight out to the sides, legs together, palms facing forward), standing perfectly upright and symmetrical, feet flat and shoulder-width apart, no weapon in hand, face and mask fully visible, keep his light blue-white kimono-style outfit and featureless mask design from the reference, clean single-mesh topology suitable for auto-rigging, textured, no base/pedestal. |
| Shukaku (TailedBeastBoss) | Non-humanoid — hand-rig in Blender | Using the provided reference image of Shukaku, generate a full-body 3D model in a neutral standing pose, all four limbs and the tail clearly separated from the torso and not overlapping or touching each other, symmetrical left-right, mouth closed, keep his sand-tanuki proportions and coloring from the reference, clean single-mesh topology, textured, no base/pedestal. |
| Orochimaru | Humanoid (Mixamo-ready) | Using the provided reference image of Orochimaru, generate a full-body 3D model in a neutral T-pose (arms straight out to the sides, legs together, palms facing forward), standing perfectly upright and symmetrical, feet flat and shoulder-width apart, keep his pale skin, snake-slit eyes, and open grey robe with purple sash from the reference, clean single-mesh topology suitable for auto-rigging, textured, no base/pedestal. |
| Kakuzu | Humanoid (Mixamo-ready) | Using the provided reference image of Kakuzu, generate a full-body 3D model in a neutral T-pose (arms straight out to the sides, legs together, palms facing forward), standing perfectly upright and symmetrical, feet flat and shoulder-width apart, keep his mask, stitched skin at the wrists/neck, and black-and-red high-collar cloak from the reference, clean single-mesh topology suitable for auto-rigging, textured, no base/pedestal. |
| Pain | Humanoid (Mixamo-ready) | Using the provided reference image of Pain, generate a full-body 3D model in a neutral T-pose (arms straight out to the sides, legs together, palms facing forward), standing perfectly upright and symmetrical, feet flat and shoulder-width apart, keep his orange hair, Rinnegan eyes, facial piercings, and black cloak with red clouds from the reference, clean single-mesh topology suitable for auto-rigging, textured, no base/pedestal. |
| Madara (base form) | Humanoid (Mixamo-ready) | Using the provided reference image of Madara Uchiha, generate a full-body 3D model in a neutral T-pose (arms straight out to the sides, legs together, palms facing forward), standing perfectly upright and symmetrical, feet flat and shoulder-width apart, keep his red armor plating, dark blue robe, and visible Sharingan eye from the reference, clean single-mesh topology suitable for auto-rigging, textured, no base/pedestal. |
| Madara — Susanoo form (separate asset) | Humanoid-ish giant (Mixamo-ready, scale up) | Generate a full-body 3D model of a titanic translucent purple ethereal samurai-warrior avatar (Susanoo) in a neutral T-pose (arms straight out to the sides, legs together), standing perfectly upright and symmetrical, glowing rib-like armor plating across the chest, a stern armored helm, faint chakra wisp trails at the edges of the limbs, clean single-mesh topology suitable for auto-rigging, textured, no base/pedestal. |
| Kaguya | Humanoid (Mixamo-ready) | Using the provided reference image of Kaguya Otsutsuki, generate a full-body 3D model in a neutral T-pose (arms straight out to the sides, legs together, palms facing forward), standing perfectly upright and symmetrical, feet flat and shoulder-width apart, keep her long white hair, twin horn-like hair buns, third eye, and flowing white kimono from the reference, clean single-mesh topology suitable for auto-rigging, textured, no base/pedestal. |

## Town NPCs

`Content/NPCs/Town/`. All fully humanoid and Mixamo-ready. See `ANIMATION_PIPELINE.md`'s Town NPCs table for the 25-frame vanilla `Guide` layout notes and recommended clips. Pilot with Kakashi first (no follow-up code needed for Town NPCs).

| Character | Rig type | Rodin prompt |
|---|---|---|
| Kakashi Hatake | Humanoid (Mixamo-ready) | Using the provided reference image of Kakashi Hatake, generate a full-body 3D model in a neutral T-pose (arms straight out to the sides, legs together, palms facing forward), standing perfectly upright and symmetrical, feet flat and shoulder-width apart, keep his silver spiky hair, dark face mask, and forehead protector tilted over one eye from the reference, clean single-mesh topology suitable for auto-rigging, textured, no base/pedestal. |
| Itachi Uchiha | Humanoid (Mixamo-ready) | Using the provided reference image of Itachi Uchiha, generate a full-body 3D model in a neutral T-pose (arms straight out to the sides, legs together, palms facing forward), standing perfectly upright and symmetrical, feet flat and shoulder-width apart, keep his tied-back hair, tear-line marks, and black high-collar cloak from the reference, clean single-mesh topology suitable for auto-rigging, textured, no base/pedestal. |
| Might Guy | Humanoid (Mixamo-ready) | Using the provided reference image of Might Guy, generate a full-body 3D model in a neutral T-pose (arms straight out to the sides, legs together, palms facing forward), standing perfectly upright and symmetrical, feet flat and shoulder-width apart, keep his bowl-cut hair, thick eyebrows, and green flak-jacket from the reference, clean single-mesh topology suitable for auto-rigging, textured, no base/pedestal. |
| Tenten (ShinobiVendorNPC) | Humanoid (Mixamo-ready) | Using the provided reference image of Tenten, generate a full-body 3D model in a neutral T-pose (arms straight out to the sides, legs together, palms facing forward), standing perfectly upright and symmetrical, feet flat and shoulder-width apart, keep her twin-bun hairstyle and tool-vendor themed outfit from the reference, clean single-mesh topology suitable for auto-rigging, textured, no base/pedestal. |

## Minions

`Content/Minions/`. **`ANIMATION_PIPELINE.md` recommends skipping the 3D pipeline for these** (no frame-animation code exists, and they're tiny) — these prompts are here only if you decide to animate them anyway. All 4 are non-humanoid, so Mixamo auto-rig won't apply; any rig needs to be hand-built in Blender. `ShadowCloneProjectile` needs no model at all — it already draws the real animated player character.

| Character | Rig type | Rodin prompt |
|---|---|---|
| Ninja Hound | Non-humanoid quadruped — hand-rig in Blender | Generate a full-body 3D model of a brown-and-white ninja hound wearing a small forehead-protector collar, in a neutral standing pose with all four legs clearly separated and evenly weighted, tail relaxed, head facing forward, symmetrical left-right, clean single-mesh topology, textured, no base/pedestal. |
| Slug | Non-humanoid, no limbs — hand-rig a simple spine chain in Blender | Generate a full-body 3D model of a pale lavender-blue slug with a glossy segmented shell-like back and small eye stalks, in a neutral straight resting pose (not curled or in motion), symmetrical left-right, clean single-mesh topology, textured, no base/pedestal. |
| Snake (summon) | Non-humanoid, no limbs — hand-rig a simple spine chain in Blender | Generate a full-body 3D model of a purple-scaled snake with yellow eyes, in a neutral straight resting pose (not coiled or striking), symmetrical left-right, clean single-mesh topology, textured, no base/pedestal. |
| Toad | Non-humanoid, bipedal-ish — hand-rig in Blender | Generate a full-body 3D model of an orange-spotted toad wearing a small ninja scarf, in a neutral squat resting pose with both arms and both legs clearly separated and evenly weighted, head facing forward, symmetrical left-right, clean single-mesh topology, textured, no base/pedestal. |

---

## Step 4 reusable prompt: pixelate / style-match

Run this against each rendered Blender frame (batch, via Hyper3D or Gemini image-edit) to bring it in line with the mod's existing hand-painted pixel-art look, reusing the style guide from `SPRITE_PROMPTS.md`:

> Convert this render into hand-painted pixel art matching a Terraria-style 2D game character sprite: bold clean dark outlines, saturated flattened colors, visible pixel grid at the target resolution, no smooth gradients or photorealistic shading, keep the character's silhouette, pose, and colors unchanged, transparent background.
