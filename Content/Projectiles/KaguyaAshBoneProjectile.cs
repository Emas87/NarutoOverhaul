using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Kaguya's own ranged attack, previously sharing ElementalBoltProjectile with Kakuzu/Madara
	// (all three read as an identical generic orb). Reuses AshBoneProjectile's existing art/anim
	// via the Texture override below instead of duplicating the PNG - explicitly Kaguya-themed
	// already (its player-item counterpart is "Kaguya's All-Killing Ash Bones", and
	// ANIMATION_PIPELINE.md documents Kaguya sharing this bone/dimensional palette).
	// friendly/hostile projectiles are normally distinct classes even when visually similar (see
	// FireballProjectile's own comment), so this is a small hostile sibling rather than
	// repurposing the player-only class.
	public class KaguyaAshBoneProjectile : ModProjectile
	{
		public override string Texture => "NarutoOverhaul/Content/Projectiles/AshBoneProjectile";

		public override void SetDefaults()
		{
			// 5x AshBoneProjectile's native 10x32 - still read as too small at both the previous 2x
			// and 3x attempts. width/height and scale both need to be set directly here
			// (Projectile.SetDefaults doesn't auto-multiply width/height by scale the way
			// NPC.SetDefaults(int) does), so there's no double-multiply trap to worry about. Also adds
			// an explicit PreDraw below (see there) so scale is guaranteed to actually apply, removing
			// any doubt after two rounds of "still too small" reports.
			Projectile.width = 50;
			Projectile.height = 160;
			Projectile.scale = 5f;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Main.rand.NextBool(3))
			{
				Common.VFX.ChakraVFX.SpawnBoneBurst(Projectile.Center, 0.5f);
			}
		}

		public override void OnKill(int timeLeft)
		{
			Common.VFX.ChakraVFX.SpawnBoneBurst(Projectile.Center, 0.75f);
		}

		// Explicit draw call (rather than relying on vanilla's default projectile draw) so
		// Projectile.scale is guaranteed to be applied - removes any doubt after this projectile
		// still read as too small across two previous size increases.
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
