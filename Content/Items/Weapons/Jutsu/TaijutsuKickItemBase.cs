using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.VFX;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// Shared UseAnimation shape for the Taijutsu/Genjutsu blast-VFX items (impact flash + a
	// player-anchored blast that tracks the swing + a forward dash nudge). IronLeg/FrontLotus use
	// the default single-flash shape as-is; LeafHurricane overrides PlayImpactFlash for its wider
	// two-flash sweep; GentleFist (no Item.shoot, so Shoot() below never fires for it) wraps its own
	// stamina gate around base.UseAnimation instead of using the Shoot-based stamina spend.
	public abstract class TaijutsuKickItemBase<TBlast> : ModItem where TBlast : ModProjectile
	{
		public abstract float StaminaCost { get; }
		protected abstract Vector2 BlastOffset { get; }
		protected abstract float OnUseDashSpeed { get; }
		protected virtual float ImpactBurstOffsetX => 24f;
		protected virtual float ImpactBurstScale => 1.2f;

		protected virtual void PlayImpactFlash(Player player)
		{
			ChakraVFX.SpawnBurstEffect<ImpactBurstProjectile>(player.Center + new Vector2(player.direction * ImpactBurstOffsetX, 0f), ImpactBurstScale);
		}

		protected void PlayBlast(Player player)
		{
			ChakraVFX.SpawnPlayerAnchoredBurst<TBlast>(player, BlastOffset);
		}

		protected void ApplyDash(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				player.velocity.X = player.direction * OnUseDashSpeed;
			}
		}

		protected bool StaminaSpent { get; private set; }

		public override void UseAnimation(Player player)
		{
			StaminaSpent = player.GetModPlayer<StaminaPlayer>().TrySpendStamina(StaminaCost);

			if (!StaminaSpent)
			{
				return;
			}

			PlayImpactFlash(player);
			PlayBlast(player);
			ApplyDash(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			return StaminaSpent;
		}
	}
}
