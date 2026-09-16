using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Projectiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Content.Projectiles
{
	// Water Style: Water Dragon Jutsu - large, slow-moving, pierces through several enemies,
	// the "big and sustained" counterpart to Fireball's quick single hit.
	public class WaterDragonProjectile : ModProjectile
	{
		// nano-banana-generated 6-frame sheet of distinct coiling/swimming poses - a directional
		// creature with its own head/tail read, so it cycles poses in place rather than spinning
		// the whole sprite like the symmetric orb/bolt jutsu.
		private const int FrameCount = 6;
		private const int TicksPerFrame = 8;
		private int animFrame;
		private int animTicks;
		private readonly DelayedTileCollide delayedTileCollide = new(clearanceDistance: 48f);

		public override void SetDefaults()
		{
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.scale = 1.25f;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 4;
			Projectile.timeLeft = 150;
			Projectile.tileCollide = false;
			Main.projFrames[Projectile.type] = FrameCount;
		}

		public override void AI()
		{
			delayedTileCollide.Update(Projectile);

			if (Projectile.velocity.X != 0f)
			{
				Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;
			}

			animTicks++;
			if (animTicks >= TicksPerFrame)
			{
				animTicks = 0;
				animFrame = (animFrame + 1) % FrameCount;
			}
			Projectile.frame = animFrame;
		}

		public override void OnKill(int timeLeft)
		{
			Common.VFX.ChakraVFX.SpawnBurstEffect<WaterBurstProjectile>(Projectile.Center, 2.25f);
		}

		// The sprite sheet's 130x130 frames are much bigger than the hitbox. Terraria's default
		// multi-frame draw anchors the texture using half the HITBOX height as its origin, not
		// half the frame height, so with a mismatch this size it draws the sprite shifted well
		// below Projectile.Center. Drawing it ourselves with the origin at the frame's own center
		// fixes that, and lets Projectile.scale actually make the dragon bigger.
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame(1, FrameCount, 0, Projectile.frame);
			Vector2 origin = frame.Size() / 2f;
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effects, 0);

			return false;
		}
	}
}
