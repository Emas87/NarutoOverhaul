using NarutoOverhaul.Common.GlobalNPCs;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Content.Projectiles
{
	// Low direct damage by design - Genjutsu's identity is the control debuff it applies, not
	// raw damage (see GenjutsuGlobalNPC.PreAI for the actual puppeting logic).
	public class GenjutsuIllusionProjectile : ModProjectile
	{
		public const int ControlDuration = 300; // 5 seconds

		private const int FrameCount = 10;
		private const int TicksPerFrame = 4;
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
				ChakraVFX.SpawnBurstEffect<GenjutsuBurstProjectile>(Projectile.Center, 0.5f);
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
			int duration = ControlDuration + owner.GetModPlayer<Common.Players.ChakraPlayer>().GenjutsuControlDurationBonus;

			target.AddBuff(ModContent.BuffType<GenjutsuControlDebuff>(), duration);
			target.GetGlobalNPC<GenjutsuGlobalNPC>().ControllingPlayerIndex = Projectile.owner;
			target.netUpdate = true; // force an immediate sync so the server/other clients learn who's controlling this puppet
			ChakraVFX.SpawnBurstEffect<GenjutsuBurstProjectile>(target.Center, 1.5f);
		}

		public override void OnKill(int timeLeft)
		{
			ChakraVFX.SpawnBurstEffect<GenjutsuBurstProjectile>(Projectile.Center, 0.75f);
		}
	}
}
