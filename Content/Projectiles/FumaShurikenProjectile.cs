using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Projectiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Heavier, slower-spinning cousin of ShurikenProjectile that pierces more targets.
	public class FumaShurikenProjectile : ModProjectile
	{
		// Hitbox stays at the native 22x22 art size - see KunaiProjectile.SetDefaults for why an
		// inflated hitbox causes spawn/collision problems. Scale alone controls visual size.
		private readonly DelayedTileCollide delayedTileCollide = new(clearanceDistance: 24f);

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.scale = 3f;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Throwing;
			Projectile.penetrate = 5;
			Projectile.timeLeft = 200;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			delayedTileCollide.Update(Projectile);
			Projectile.rotation += 0.35f;
		}

		// See KunaiProjectile.PreDraw - draws at the texture's real center so scale alone controls
		// visual size without needing to touch (or mismatch) the hitbox.
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
