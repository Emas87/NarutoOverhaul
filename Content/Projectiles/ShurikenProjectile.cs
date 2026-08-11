using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Projectiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Unlike KunaiProjectile (single-target, no spin), this pierces multiple enemies and spins
	// fast in flight - the "shuriken" identity within the same basic-thrown-weapon niche.
	public class ShurikenProjectile : ModProjectile
	{
		// Hitbox stays at the native 14x14 art size - see KunaiProjectile.SetDefaults for why an
		// inflated hitbox causes spawn/collision problems. Scale alone controls visual size.
		private readonly DelayedTileCollide delayedTileCollide = new(clearanceDistance: 24f);

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.scale = 22f / 14f;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Throwing;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			delayedTileCollide.Update(Projectile);
			Projectile.rotation += 0.5f;
		}

		// See KunaiProjectile.PreDraw - vanilla's default draw origin uses half the hitbox height,
		// not half the texture height, so scaling the hitbox past the native 14x14 art throws off
		// the rotation pivot. Drawing it ourselves keeps the spin centered on the actual sprite.
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
