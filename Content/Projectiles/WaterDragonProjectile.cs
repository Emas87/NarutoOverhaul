using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Water Style: Water Dragon Jutsu - large, slow-moving, pierces through several enemies,
	// the "big and sustained" counterpart to Fireball's quick single hit.
	public class WaterDragonProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 4;
			Projectile.timeLeft = 150;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			Projectile.rotation += 0.1f;
			Common.VFX.ChakraVFX.SpawnBurst(Projectile.Center, DustID.Water, 2, 1.3f);
		}

		public override void OnKill(int timeLeft)
		{
			Common.VFX.ChakraVFX.SpawnBurst(Projectile.Center, DustID.Water, 12, 1.5f);
		}
	}
}
