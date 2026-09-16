using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Content.Projectiles
{
	// Kaguya's All-Killing Ash Bones - a plain traveling spike, fired 3-at-once in a spread by
	// AllKillingAshBonesItem.Shoot (same "manual multi-fire, return false" idiom as boss AI's
	// spread attacks, just from a player weapon instead).
	public class AshBoneProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 32;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<NinjutsuDamageClass>();
			Projectile.penetrate = 2;
			Projectile.timeLeft = 240;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Main.rand.NextBool(3))
			{
				Common.VFX.ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(Projectile.Center, 0.5f);
			}
		}

		public override void OnKill(int timeLeft)
		{
			Common.VFX.ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(Projectile.Center, 0.75f);
		}
	}
}
