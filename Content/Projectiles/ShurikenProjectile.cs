using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Unlike KunaiProjectile (single-target, no spin), this pierces multiple enemies and spins
	// fast in flight - the "shuriken" identity within the same basic-thrown-weapon niche.
	public class ShurikenProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Throwing;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			Projectile.rotation += 0.5f;
		}
	}
}
