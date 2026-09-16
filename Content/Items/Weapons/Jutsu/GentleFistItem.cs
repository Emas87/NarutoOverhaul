using System.Collections.Generic;
using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Projectiles.Bursts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// Neji's fighting style, unlocked from the Chunin Exams arc (Shukaku fight). A direct melee
	// strike (not a projectile jutsu like Rasengan) that ignores a chunk of the target's defense -
	// chakra-guided precision strikes to the target's tenketsu points rather than raw damage.
	public class GentleFistItem : TaijutsuKickItemBase<GentleFistBlastProjectile>
	{
		public const float ArmorPenetration = 40f;
		public override float StaminaCost => 8f;
		protected override Vector2 BlastOffset => new Vector2(16f, 0f);
		// Forward nudge applied on every strike - Gentle Fist previously had no dash at all, unlike
		// the 3 kick weapons' DashSpeed lunge-on-hit; a small on-use nudge brings it into the same
		// "dash class" identity without turning a precision punch into a lunging attack.
		protected override float OnUseDashSpeed => 2.5f;
		protected override float ImpactBurstOffsetX => 20f;
		protected override float ImpactBurstScale => 0.9f;

		// Set by UseAnimation, read by ModifyHitNPC/OnHitNPC — the armor-pen and
		// impact VFX bonuses should only land when this swing actually paid its
		// stamina cost, matching Rasengan's own TrySpendChakra gate.
		private bool _staminaSpent;

		public override void SetDefaults()
		{
			// Widened from the item's old 26x26 nominal hitbox - was noticeably tighter than the kick
			// weapons' own hitboxes even though Gentle Fist is meant to land as reliably as they do.
			Item.width = 36;
			Item.height = 36;
			Item.damage = 22;
			Item.DamageType = ModContent.GetInstance<TaijutsuDamageClass>();
			// Thrust = fixed horizontal jab in the player's facing direction, not an overhead sword
			// arc - the same use style GumGum (a One Piece mod, decompiled for reference) uses for
			// its own bare-fist punch item.
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.useAnimation = 18;
			Item.useTime = 18;
			Item.autoReuse = true;
			// No held weapon graphic - this is a bare-handed strike, not a sword swing.
			Item.noUseGraphic = true;
			Item.knockBack = 9f;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedShukaku && player.GetModPlayer<StaminaPlayer>().Stamina >= StaminaCost;
		}

		public override void UseAnimation(Player player)
		{
			_staminaSpent = player.GetModPlayer<StaminaPlayer>().TrySpendStamina(StaminaCost);
			if (!_staminaSpent)
			{
				return;
			}
			// Large body-covering blast - thin precise chakra-point flares along the arm/torso rather
			// than a big blunt kick blast, matching Gentle Fist's surgical/armor-pen flavor.
			base.UseAnimation(player);
		}

		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (!_staminaSpent)
			{
				return;
			}
			modifiers.ArmorPenetration += ArmorPenetration;
		}

		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!_staminaSpent)
			{
				return;
			}
			ChakraVFX.SpawnBurstEffect<ImpactBurstProjectile>(target.Center, 1.3f);
			ChakraVFX.SpawnDirectionalBurst(target.Center, new Vector2(player.direction, -0.2f), speed: 4f, scale: 1f);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "StaminaCost", $"Uses {StaminaCost} Stamina"));
			tooltips.Add(new TooltipLine(Mod, "StoryRequirement", "Requires Shukaku defeated"));
			tooltips.Add(new TooltipLine(Mod, "ArmorPen", $"Strikes ignore {ArmorPenetration} defense"));
		}
	}
}
