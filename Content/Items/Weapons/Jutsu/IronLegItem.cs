using System.Collections.Generic;
using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Projectiles;
using NarutoOverhaul.Content.Projectiles.Bursts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// A single heavy kick - high damage/knockback, cheap stamina cost, small hitbox. The
	// power-over-reach counterpart to Leaf Hurricane's reach-over-power. Damage comes from
	// IronLegKickProjectile (a player-anchored, invisible projectile) rather than the item's own
	// hitbox - see TaijutsuKickProjectile for why (real body-rotation kick animation).
	public class IronLegItem : ModItem
	{
		public const float StaminaCost = 6f;

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.damage = 40;
			Item.DamageType = ModContent.GetInstance<TaijutsuDamageClass>();
			// Thrust = fixed horizontal jab, not an overhead sword arc - paired with
			// TaijutsuKickAnimationPlayer forcing the leg into vanilla's Jump pose during the
			// swing, which is what actually reads as a kick (the arm style alone can't).
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.useAnimation = 20;
			Item.useTime = 20;
			// The heaviest base knockback in Taijutsu's kit, on brand for a single heavy kick -
			// IronLegKickProjectile's ModifyKickHit pushes it further still.
			Item.knockBack = 18f;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
			// No held weapon graphic - this is a bare-handed strike, not a sword swing. Damage is
			// entirely projectile-driven (see class summary), so the item's own hitbox is disabled.
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<IronLegKickProjectile>();
			Item.shootSpeed = 1f;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedShukaku && player.GetModPlayer<StaminaPlayer>().Stamina >= StaminaCost;
		}

		// Forward nudge applied on every swing (not just on a landed hit, unlike
		// IronLegKickProjectile's DashSpeed lunge) so Taijutsu reads as a dash class even on a whiff.
		public const float OnUseDashSpeed = 4f;

		public override void UseAnimation(Player player)
		{
			ChakraVFX.SpawnImpactBurst(player.Center + new Vector2(player.direction * 24f, 0f), 1.2f);
			// Large body-covering blast, offset toward the lower body / kicking leg - tracks the
			// player through the whole swing via PlayerAnchoredBurstEffectProjectile.
			ChakraVFX.SpawnPlayerAnchoredBurst<IronLegBlastProjectile>(player, new Vector2(20f, 14f));

			if (player.whoAmI == Main.myPlayer)
			{
				player.velocity.X = player.direction * OnUseDashSpeed;
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			return player.GetModPlayer<StaminaPlayer>().TrySpendStamina(StaminaCost);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "StaminaCost", $"Uses {StaminaCost} Stamina"));
			tooltips.Add(new TooltipLine(Mod, "StoryRequirement", "Requires Shukaku defeated"));
		}
	}
}
