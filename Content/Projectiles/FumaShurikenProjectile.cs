using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Heavier, slower-spinning cousin of ShurikenProjectile that pierces more targets.
	public class FumaShurikenProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Throwing;
			Projectile.penetrate = 5;
			Projectile.timeLeft = 200;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			Projectile.rotation += 0.35f;
		}
	}
}
