using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Simple thrown senbon needle - straight-line flight, no homing, used by HakuBoss's volleys.
	public class SenbonProjectile : ModProjectile
	{
		// The 4x20 art isn't a vertical needle - it's a short diagonal stroke already baked in at
		// roughly this angle (measured directly from the pixels: it runs from (1,9) to (3,11)), same
		// situation as KunaiProjectile's baked-angle stroke. The old `velocity.ToRotation() +
		// PiOver2` assumed a straight-up needle sprite, which fought this baked angle and made the
		// senbon visibly not point along its own travel direction.
		private static readonly float BakedArtAngle = MathF.Atan2(2f, 2f);

		public override void SetDefaults()
		{
			// Native art is only 4x20 - nearly invisible at 1x, same issue KunaiProjectile/
			// ShurikenProjectile had. Hitbox stays at native size (see KunaiProjectile.SetDefaults
			// for why an inflated hitbox on a thin rotating projectile causes problems); scale +
			// PreDraw below handle the visual size instead.
			Projectile.width = 4;
			Projectile.height = 20;
			Projectile.scale = 5f;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() - BakedArtAngle;
		}

		public override void OnKill(int timeLeft)
		{
			Common.VFX.ChakraVFX.SpawnIceBurst(Projectile.Center, 0.5f);
		}

		// See KunaiProjectile.PreDraw - vanilla's default draw origin uses half the hitbox height,
		// not half the texture height, so scaling past the native 4x20 art throws off the rotation
		// pivot. Drawing it ourselves keeps the spin centered on the actual sprite.
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = texture.Size() / 2f;
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;

			Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}
	}
}
