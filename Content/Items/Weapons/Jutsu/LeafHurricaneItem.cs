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
	// A sweeping spin kick - wide reach so it naturally catches multiple enemies in one swing,
	// unlike Gentle Fist's single precise strike. Damage comes from LeafHurricaneKickProjectile (a
	// player-anchored, invisible projectile) rather than the item's own hitbox - see
	// TaijutsuKickProjectile for why (real body-rotation kick animation).
	public class LeafHurricaneItem : ModItem
	{
		public const float StaminaCost = 12f;

		public override void SetDefaults()
		{
			Item.width = 44;
			Item.height = 44;
			Item.damage = 26;
			Item.DamageType = ModContent.GetInstance<TaijutsuDamageClass>();
			// Thrust = fixed horizontal jab, not an overhead sword arc - paired with
			// TaijutsuKickAnimationPlayer forcing the leg into vanilla's Jump pose during the
			// swing, which is what actually reads as a kick (the arm style alone can't).
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.useAnimation = 26;
			Item.useTime = 26;
			Item.knockBack = 12f;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
			// No held weapon graphic - this is a bare-handed strike, not a sword swing. Damage is
			// entirely projectile-driven (see class summary), so the item's own hitbox is disabled.
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<LeafHurricaneKickProjectile>();
			Item.shootSpeed = 1f;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedShukaku && player.GetModPlayer<StaminaPlayer>().Stamina >= StaminaCost;
		}

		// Forward nudge applied on every swing (not just on a landed hit, unlike
		// LeafHurricaneKickProjectile's DashSpeed lunge) so Taijutsu reads as a dash class even on a
		// whiff. Weakest on-use nudge in the kit - the wide hitbox already covers ground.
		public const float OnUseDashSpeed = 3f;

		public override void UseAnimation(Player player)
		{
			// Wide sweeping kick - two flashes on either side to read as a spin, not a single punch,
			// plus a ring of plain dust connecting them so the sweep reads as one continuous motion
			// instead of two disconnected flashes (Iron Leg/Front Lotus are single decisive kicks,
			// so they don't get this).
			ChakraVFX.SpawnImpactBurst(player.Center + new Vector2(player.direction * 30f, 0f), 1.4f);
			ChakraVFX.SpawnImpactBurst(player.Center + new Vector2(-player.direction * 30f, 0f), 1.1f);
			ChakraVFX.SpawnBurst(player.Center, DustID.Cloud, count: 6, scale: 1.3f);
			// Large body-covering blast - a full rotating ring around the lower body, matching the
			// spin-kick's wide arc.
			ChakraVFX.SpawnPlayerAnchoredBurst<LeafHurricaneBlastProjectile>(player, new Vector2(0f, 16f));

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
