using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.VFX;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Arcs under gravity, sticks to whatever it hits first - an enemy (rides along with it) or a
	// tile (stays put) - then detonates when its fuse (timeLeft) runs out. Same oversized-hitbox
	// explosion pattern as ExplosiveKunaiProjectile.
	public class PaperBombProjectile : ModProjectile
	{
		private const int BlastRadius = 110;
		private const int FuseTicks = 180;

		private bool Stuck => Projectile.ai[0] == 1f;
		private bool Exploding => Projectile.ai[1] == 1f;

		// Not synced over ai[] (already full at 2 slots) - fine for this mod's current netcode
		// scope, same as the other single-player-assumed projectile state elsewhere in the mod.
		private int stuckNpcIndex = -1;
		private Vector2 stuckOffsetFromNpcCenter;

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Throwing;
			Projectile.penetrate = -1;
			Projectile.timeLeft = FuseTicks;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			if (Exploding)
			{
				return;
			}

			if (Stuck)
			{
				if (stuckNpcIndex != -1)
				{
					NPC npc = Main.npc[stuckNpcIndex];
					if (!npc.active)
					{
						Explode();
						return;
					}

					Projectile.position = npc.Center + stuckOffsetFromNpcCenter - Projectile.Size / 2f;
				}

				Projectile.velocity = Vector2.Zero;
				// fizzing fuse sparks
				if (Main.rand.NextBool(4))
				{
					Dust.NewDust(Projectile.position, Projectile.width, 4, DustID.Torch, Scale: 0.8f);
				}
			}
			else
			{
				Projectile.velocity.Y += 0.25f;
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}

			if (Projectile.timeLeft <= 3)
			{
				Explode();
			}
		}

		private void Explode()
		{
			if (Exploding)
			{
				return;
			}

			Projectile.ai[1] = 1f;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.velocity = Vector2.Zero;
			Projectile.alpha = 255;
			Projectile.Resize(BlastRadius, BlastRadius);
			Projectile.timeLeft = 3;
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			ChakraVFX.SpawnFireBurst(Projectile.Center, scale: 3.5f);
			for (int i = 0; i < 12; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (!Stuck)
			{
				Projectile.ai[0] = 1f;
			}

			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Stuck)
			{
				return;
			}

			Projectile.ai[0] = 1f;
			stuckNpcIndex = target.whoAmI;
			stuckOffsetFromNpcCenter = Projectile.Center - target.Center;
			// Stops dealing the direct throwing hit every tick while riding along - the fuse
			// explosion (which re-enables friendly) is what actually punishes the target.
			Projectile.friendly = false;
		}
	}
}
