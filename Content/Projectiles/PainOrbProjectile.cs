using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Pain's own ranged bolt, split off from the shared ElementalBoltProjectile (which Kakuzu keeps)
	// specifically so it can pierce terrain without also changing Kakuzu's - "friendly/hostile
	// projectiles are normally distinct classes even when visually similar" (see
	// FireballProjectile's own comment) applies equally to two hostile projectiles that need
	// genuinely different behavior. Reuses ElementalBoltProjectile's existing art/anim via the
	// Texture override below instead of duplicating the PNG.
	public class PainOrbProjectile : ModProjectile
	{
		public override string Texture => "NarutoOverhaul/Content/Projectiles/ElementalBoltProjectile";

		public ElementalBoltProjectile.Element BoltElement
		{
			get => (ElementalBoltProjectile.Element)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}

		private void SpawnElementBurst(float scale)
		{
			switch (BoltElement)
			{
				case ElementalBoltProjectile.Element.Wind:
					Common.VFX.ChakraVFX.SpawnWindBurst(Projectile.Center, scale);
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

		// Pain reads as an unstoppable force in canon (Bansho Ten'in/Shinra Tensei ignore obstacles),
		// and unlike Kakuzu's own bolts, nothing established these need to be blocked by terrain.
		public override void SetDefaults()
		{
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.scale = 2f;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.tileCollide = false;
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

			if (Main.rand.NextBool(3))
			{
				SpawnElementBurst(0.5f);
			}
		}

		public override void OnKill(int timeLeft)
		{
			SpawnElementBurst(0.75f);
		}

		// "Pain levels the Hidden Leaf Village" - his bolts should read as a real threat to Town
		// NPCs specifically (not just players), so a hit tops up to roughly half the target's max
		// life instead of the flat player-facing damage value, without changing player-facing
		// damage at all (damageDone already applied normally by the engine before this runs).
		private const float TownNpcDamageFraction = 0.5f;

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!target.townNPC)
			{
				return;
			}

			int bonusDamage = (int)(target.lifeMax * TownNpcDamageFraction) - damageDone;

			if (bonusDamage > 0)
			{
				target.SimpleStrikeNPC(bonusDamage, 0, noPlayerInteraction: true);
			}
		}

		public override Color? GetAlpha(Color lightColor)
		{
			Color tint = BoltElement switch
			{
				ElementalBoltProjectile.Element.Wind => new Color(170, 230, 170),
				_ => new Color(160, 90, 190),
			};

			return Color.Lerp(lightColor, tint, 0.5f);
		}
	}
}
