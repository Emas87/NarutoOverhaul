using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Shared by KakuzuBoss's 5 mask phases - which element it represents is passed in via
	// ai[0] at spawn time (0=Fire, 1=Wind, 2=Lightning, 3=Earth, 4=Core), so one projectile
	// class covers all five instead of near-duplicate classes per element.
	public class ElementalBoltProjectile : ModProjectile
	{
		public enum Element
		{
			Fire,
			Wind,
			Lightning,
			Earth,
			Core
		}

		public Element BoltElement
		{
			get => (Element)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}

		private void SpawnElementBurst(float scale)
		{
			switch (BoltElement)
			{
				case Element.Fire:
					Common.VFX.ChakraVFX.SpawnFireBurst(Projectile.Center, scale);
					break;
				case Element.Wind:
					Common.VFX.ChakraVFX.SpawnWindBurst(Projectile.Center, scale);
					break;
				case Element.Lightning:
					Common.VFX.ChakraVFX.SpawnLightningBurst(Projectile.Center, scale);
					break;
				case Element.Earth:
					Common.VFX.ChakraVFX.SpawnEarthBurst(Projectile.Center, scale);
					break;
				default:
					Common.VFX.ChakraVFX.SpawnCoreBurst(Projectile.Center, scale);
					break;
			}
		}

		private const int FrameCount = 7;
		private const int TicksPerFrame = 6;
		private int animFrame;
		private int animTicks;

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Main.projFrames[Projectile.type] = FrameCount;
		}

		public override void AI()
		{
			Projectile.rotation += 0.2f;

			animTicks++;
			if (animTicks >= TicksPerFrame)
			{
				animTicks = 0;
				animFrame = (animFrame + 1) % FrameCount;
			}
			Projectile.frame = animFrame;

			// Lightning bolts curve slightly toward their target's last known direction for a
			// "homing" feel; every other element just flies straight.
			if (BoltElement == Element.Lightning && Projectile.timeLeft % 10 == 0)
			{
				Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
				Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity);
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.15f);
			}

			if (Main.rand.NextBool(3))
			{
				SpawnElementBurst(0.5f);
			}
		}

		public override void OnKill(int timeLeft)
		{
			SpawnElementBurst(0.75f);
		}
	}
}
