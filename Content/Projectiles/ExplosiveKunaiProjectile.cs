using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Projectiles;
using NarutoOverhaul.Common.VFX;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Flies like a kunai, then detonates on the first thing it touches using the vanilla-grenade
	// pattern: swap into a short-lived oversized hitbox so everything in the blast takes the hit.
	public class ExplosiveKunaiProjectile : ModProjectile
	{
		private const int BlastRadius = 72;

		private bool Exploding => Projectile.ai[1] == 1f;

		// See KunaiProjectile.SetDefaults - small square hitbox decoupled from the taller 8x24 art
		// so it doesn't stay upright (and clip the floor) once the sprite visually rotates to point
		// at its target. tileCollide starts off and only turns on once clear of the spawn point.
		private readonly DelayedTileCollide delayedTileCollide = new(clearanceDistance: 24f);

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.scale = 3.5f;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Throwing;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = false;
		}

		// See KunaiProjectile.BakedArtAngle - same fix, but this is a separate 8x24 asset whose
		// diagonal stroke runs the other way (measured from the pixels: (6,9) to (0,13)).
		private static readonly float BakedArtAngle = MathF.Atan2(-4f, 7f);

		public override void AI()
		{
			if (!Exploding)
			{
				delayedTileCollide.Update(Projectile);
				// See KunaiProjectile.AI - +Pi so the tip leads instead of the trailing/hilt end.
				Projectile.rotation = Projectile.velocity.ToRotation() - BakedArtAngle + MathHelper.Pi;
			}
		}

		private void Explode()
		{
			if (Exploding)
			{
				return;
			}

			Projectile.ai[1] = 1f;
			Projectile.tileCollide = false;
			Projectile.velocity = Vector2.Zero;
			Projectile.alpha = 255;
			Projectile.Resize(BlastRadius, BlastRadius);
			Projectile.timeLeft = 3;
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			ChakraVFX.SpawnFireBurst(Projectile.Center, scale: 2.2f);
			for (int i = 0; i < 8; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke);
			}
		}

		// See KunaiProjectile.PreDraw - same hitbox/texture-size mismatch fix. Not relevant during
		// the exploding phase since Projectile.alpha is already 255 (fully invisible) there.
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = texture.Size() / 2f;
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;

			Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Explode();
			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Explode();
		}
	}
}
