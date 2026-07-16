using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Pain's Almighty Push - a stationary burst centered on the caster. Deliberately simple: a
	// large hitbox with a short lifetime and high Item.knockBack, letting vanilla's own
	// projectile-vs-NPC collision/knockback pipeline do the "everyone nearby gets shoved" work
	// rather than a hand-written NPC-radius loop.
	public class ShinraTenseiProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 220;
			Projectile.height = 220;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<GenjutsuDamageClass>();
			Projectile.penetrate = -1;
			Projectile.timeLeft = 12;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 12)
			{
				Common.VFX.ChakraVFX.SpawnBurst(Projectile.Center, DustID.PurpleTorch, 30, 2.2f);
				SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			}
		}
	}
}
