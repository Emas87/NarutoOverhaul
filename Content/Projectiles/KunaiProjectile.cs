using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Projectiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Plain thrown kunai - no chakra, no jutsu mechanic, just the starter weapon's projectile.
	public class KunaiProjectile : ModProjectile
	{
		// PreDraw below always centers the texture on Projectile.Center (the hitbox's own center),
		// so the hitbox doesn't need to match the art's 8x24 size for the sprite to draw correctly.
		// Kept small and roughly square instead - Terraria hitboxes don't rotate with the sprite, so
		// the native portrait-shaped 8x24 box was staying tall/upright even once the kunai visually
		// rotated to point sideways at its target, which meant it kept clipping the floor almost
		// immediately on anything but a dead-flat throw.
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
			Projectile.penetrate = 1;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = false;
		}

		// The 8x24 art isn't a blade silhouette pointing along one axis - it's a short ~13px diagonal
		// stroke already baked in at roughly this angle (measured directly from the pixels: it runs
		// from (0,9) to (7,13)). Any fixed rotation offset fights that baked-in angle depending on
		// throw direction, which is what was making it look wrong on release. Subtracting it here
		// cancels it out so the stroke's own direction tracks the throw angle instead.
		private static readonly float BakedArtAngle = MathF.Atan2(4f, 7f);

		public override void AI()
		{
			delayedTileCollide.Update(Projectile);
			// +Pi on top of the angle cancellation - canceling the baked angle alone pointed the
			// (0,9) end forward, but that's the trailing/hilt end, not the tip.
			Projectile.rotation = Projectile.velocity.ToRotation() - BakedArtAngle + MathHelper.Pi;
		}

		// Vanilla's default draw anchors the origin at half the HITBOX height (Projectile.height),
		// not half the texture's own height - see WaterDragonProjectile.cs for the full writeup of
		// this bug. Drawing it ourselves with the origin at the texture's real center avoids that
		// mismatch entirely and lets Projectile.scale make the sprite bigger without touching the
		// hitbox.
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
