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
	public class IronLegItem : TaijutsuKickItemBase<IronLegBlastProjectile>
	{
		public override float StaminaCost => 6f;
		protected override Vector2 BlastOffset => new Vector2(20f, 14f);
		protected override float OnUseDashSpeed => 4f;
		protected override float ImpactBurstScale => 1.2f;

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

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "StaminaCost", $"Uses {StaminaCost} Stamina"));
			tooltips.Add(new TooltipLine(Mod, "StoryRequirement", "Requires Shukaku defeated"));
		}
	}
}
