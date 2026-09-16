using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Content.Projectiles
{
	// Single-target total immobilization - shorter duration than the puppet illusion, but the
	// target can't move at all rather than being steerable.
	public class GenjutsuSleepProjectile : ModProjectile
	{
		public const int SleepDuration = 150; // 2.5 seconds

		private const int FrameCount = 6;
		private const int TicksPerFrame = 6;
		private int animFrame;
		private int animTicks;

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<GenjutsuDamageClass>();
			Projectile.penetrate = 1;
			Projectile.timeLeft = 90;
			Projectile.tileCollide = true;
			Main.projFrames[Projectile.type] = FrameCount;
		}

		public override void AI()
		{
			Projectile.rotation += 0.15f;

			if (Main.rand.NextBool(2))
			{
				Common.VFX.ChakraVFX.SpawnBurstEffect<GenjutsuBurstProjectile>(Projectile.Center, 0.5f);
			}

			animTicks++;
			if (animTicks >= TicksPerFrame)
			{
				animTicks = 0;
				animFrame = (animFrame + 1) % FrameCount;
			}
			Projectile.frame = animFrame;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player owner = Main.player[Projectile.owner];
			int duration = SleepDuration + owner.GetModPlayer<Common.Players.ChakraPlayer>().GenjutsuControlDurationBonus;

			target.AddBuff(ModContent.BuffType<GenjutsuSleepDebuff>(), duration);
			Common.VFX.ChakraVFX.SpawnBurstEffect<GenjutsuBurstProjectile>(target.Center, 1.5f);
		}

		public override void OnKill(int timeLeft)
		{
			Common.VFX.ChakraVFX.SpawnBurstEffect<GenjutsuBurstProjectile>(Projectile.Center, 0.75f);
		}
	}
}
