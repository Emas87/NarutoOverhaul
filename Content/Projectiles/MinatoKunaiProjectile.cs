using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Projectiles;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Tiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Flies like a kunai (see KunaiProjectile/ExplosiveKunaiProjectile - same DelayedTileCollide
	// spawn-clearance pattern), but instead of dealing damage or exploding, plants a
	// HiraishinSealTile wherever it hits a wall. Purely a utility/marker item, not a weapon -
	// CanDamage() is false, same as SubstitutionLogProjectile/BurstEffectProjectile.
	public class MinatoKunaiProjectile : ModProjectile
	{
		private readonly DelayedTileCollide delayedTileCollide = new(clearanceDistance: 24f);

		// Leftover from copying ExplosiveKunaiProjectile's template - that projectile's native art
		// is a tiny 10x10, so it needs 3.5x scale to read on screen. This one's art is already
		// sized to look right natively (see the art-resize step in tools/ history), so no extra
		// scale multiplier - that 3.5x was making it render far too large.
		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override bool? CanDamage() => false;

		// Gravity constant matches PaperBombProjectile's already-tuned throwing-weapon arc feel
		// ("no longer detonates instantly on throw" per totest.md) - a real kunai throw should
		// fall, not fly in a dead-straight line.
		private const float Gravity = 0.25f;

		public override void AI()
		{
			delayedTileCollide.Update(Projectile);
			Projectile.velocity.Y += Gravity;
			// The art is drawn pointing along +X (tip right) by construction, unlike
			// KunaiProjectile/ExplosiveKunaiProjectile's hand-drawn art (which needed a measured
			// BakedArtAngle offset) - so the raw velocity angle is already correct, and naturally
			// tips the nose down as gravity pulls it into an arc.
			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		// See KunaiProjectile.PreDraw - same hitbox/texture-size mismatch fix: draw centered on the
		// texture's own size, not the (smaller, decoupled) hitbox.
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
			PlantSeal();
			return false;
		}

		private void PlantSeal()
		{
			Point16 tileCoord = Projectile.Center.ToTileCoordinates16();

			if (IsSolid(tileCoord))
			{
				// Step back one tile opposite the direction of travel, into the open space just
				// before the wall the kunai embedded into - WorldGen.PlaceTile can't target a tile
				// that's already occupied by the solid block it just hit. Point16's X/Y are
				// readonly, so this rebuilds rather than mutates in place.
				tileCoord = new Point16(
					tileCoord.X - System.Math.Sign(Projectile.velocity.X),
					tileCoord.Y - System.Math.Sign(Projectile.velocity.Y));
			}

			if (IsSolid(tileCoord))
			{
				// Both the hit tile and the one-step fallback are solid (e.g. a corner) - bail
				// rather than guessing further; the player just wastes this one kunai.
				Projectile.Kill();
				return;
			}

			if (Projectile.owner == Main.myPlayer)
			{
				// HiraishinSealTile is TileObjectData-registered (Style1x1), not a legacy simple
				// tile - WorldGen.PlaceTile is the wrong API for that (doesn't run TileObject's
				// CanPlace/Place/SquareTileFrame pipeline), which is why the tile was never really
				// persisting or minable. WorldGen.PlaceObject is the correct one and also triggers
				// HiraishinSealTile.PlaceInWorld properly, which already calls AddMark - the
				// explicit AddMark below is just a harmless belt-and-suspenders (idempotent).
				bool placed = WorldGen.PlaceObject(tileCoord.X, tileCoord.Y, ModContent.TileType<HiraishinSealTile>(), mute: true);

				if (placed)
				{
					HiraishinMarkerSystem.AddMark(tileCoord);

					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						NetMessage.SendTileSquare(-1, tileCoord.X, tileCoord.Y, 1);
					}

					SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
					ChakraVFX.SpawnChakraBurst(Projectile.Center, 0.8f);
				}
			}

			Projectile.Kill();
		}

		private static bool IsSolid(Point16 tileCoord)
		{
			if (!WorldGen.InWorld(tileCoord.X, tileCoord.Y))
			{
				return true;
			}

			Tile tile = Main.tile[tileCoord.X, tileCoord.Y];
			return tile.HasTile && Main.tileSolid[tile.TileType];
		}
	}
}
