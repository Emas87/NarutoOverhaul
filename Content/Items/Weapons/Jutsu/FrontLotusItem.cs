using System.Collections.Generic;
using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// Guy/Lee's inner-gate assisted strike - gated on Kakuzu (the first Hardmode boss) rather than
	// Shukaku like the rest of Guy's stock, filling Taijutsu's previously-nonexistent Hardmode tier.
	// Damage comes from FrontLotusKickProjectile (a player-anchored, invisible projectile) rather
	// than the item's own hitbox - see TaijutsuKickProjectile for why (real body-rotation kick
	// animation plus the dash-through lunge).
	public class FrontLotusItem : ModItem
	{
		public const float StaminaCost = 16f;

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.damage = 58;
			Item.DamageType = ModContent.GetInstance<TaijutsuDamageClass>();
			// Thrust = fixed horizontal jab, not an overhead sword arc - paired with
			// TaijutsuKickAnimationPlayer forcing the leg into vanilla's Jump pose during the
			// swing, which is what actually reads as a kick (the arm style alone can't).
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.useAnimation = 22;
			Item.useTime = 22;
			Item.autoReuse = true;
			// Heaviest hit in Taijutsu's kit - the knockback should feel like it, on top of the
			// per-hit bonus in FrontLotusKickProjectile.ModifyKickHit.
			Item.knockBack = 11f;
			Item.value = Item.sellPrice(gold: 14);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item1;
			// No held weapon graphic - this is a bare-handed strike, not a sword swing. Damage is
			// entirely projectile-driven (see class summary), so the item's own hitbox is disabled.
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<FrontLotusKickProjectile>();
			Item.shootSpeed = 1f;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedKakuzu && player.GetModPlayer<StaminaPlayer>().Stamina >= StaminaCost;
		}

		public override void UseAnimation(Player player)
		{
			// Kick-flash in front of the player, syncing the swing to a visible strike instead of a
			// bare-handed no-op (noUseGraphic means there's otherwise no weapon sprite at all).
			ChakraVFX.SpawnImpactBurst(player.Center + new Vector2(player.direction * 24f, 0f), 1.3f);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			return player.GetModPlayer<StaminaPlayer>().TrySpendStamina(StaminaCost);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "StaminaCost", $"Uses {StaminaCost} Stamina"));
			tooltips.Add(new TooltipLine(Mod, "StoryRequirement", "Requires Kakuzu defeated"));
			tooltips.Add(new TooltipLine(Mod, "ArmorPen", $"Strikes ignore {FrontLotusKickProjectile.ArmorPenetrationAmount} defense and hit with extra force"));
		}
	}
}
