using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Projectiles.Bursts;

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
					Common.VFX.ChakraVFX.SpawnBurstEffect<FireBurstProjectile>(Projectile.Center, scale);
					break;
				case Element.Wind:
					Common.VFX.ChakraVFX.SpawnBurstEffect<WindBurstProjectile>(Projectile.Center, scale);
					break;
				case Element.Lightning:
					Common.VFX.ChakraVFX.SpawnBurstEffect<LightningBurstProjectile>(Projectile.Center, scale);
					break;
				case Element.Earth:
					Common.VFX.ChakraVFX.SpawnBurstEffect<EarthBurstProjectile>(Projectile.Center, scale);
					break;
				default:
					Common.VFX.ChakraVFX.SpawnBurstEffect<CoreBurstProjectile>(Projectile.Center, scale);
					break;
			}
		}

		private const int FrameCount = 7;
		private const int TicksPerFrame = 6;
		private int animFrame;
		private int animTicks;

		public override void SetDefaults()
		{
			// Read as too small next to the bosses that fire it - width/height and scale both need
			// to be set directly here (unlike NPC.SetDefaults(int), Projectile.SetDefaults(int) does
			// NOT auto-multiply width/height by scale, confirmed by decompile), so there's no
			// double-multiply trap to worry about the way there was for the NPC bosses this session.
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.scale = 2f;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			// Kakuzu-only now (Pain has his own PainOrbProjectile, which pierces terrain) - normal
			// tile collision here.
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

				if (target.active)
				{
					Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity);
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.15f);
				}
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

		// The bolt sprite itself was previously identical regardless of element (only the trailing
		// burst particles differed) - mirrors KakuzuBoss's own GetAlpha() tint table so his 5 phases
		// (and, before this, Madara/Kaguya's shared use of this same class) read as visually distinct
		// in flight, not just via the occasional particle.
		public override Color? GetAlpha(Color lightColor)
		{
			Color tint = BoltElement switch
			{
				Element.Fire => new Color(230, 140, 60),
				Element.Wind => new Color(170, 230, 170),
				Element.Lightning => new Color(130, 200, 255),
				Element.Earth => new Color(150, 110, 70),
				Element.Core => new Color(160, 90, 190),
				_ => Color.White,
			};

			return Color.Lerp(lightColor, tint, 0.5f);
		}
	}
}
