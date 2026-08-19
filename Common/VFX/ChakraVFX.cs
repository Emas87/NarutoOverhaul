using Microsoft.Xna.Framework;
using NarutoOverhaul.Content.Projectiles;
using NarutoOverhaul.Content.Projectiles.Bursts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.VFX
{
	// Shared VFX helpers so each new jutsu doesn't hand-roll its own effect-spawning loop.
	// SpawnBurst/SpawnDirectionalBurst (plain Dust) are kept for the couple of call sites that are
	// generic hit-feedback rather than a jutsu/element effect (e.g. plain damage-taken blood) - those
	// don't map to any BurstEffectProjectile family and don't need networked animation.
	public static class ChakraVFX
	{
		public static void SpawnBurst(Vector2 position, int dustType = DustID.BlueTorch, int count = 8, float scale = 1.2f, bool noGravity = true)
		{
			for (int i = 0; i < count; i++)
			{
				int index = Dust.NewDust(position, 1, 1, dustType, Scale: scale);
				Main.dust[index].noGravity = noGravity;
			}
		}

		public static void SpawnDirectionalBurst(Vector2 position, Vector2 direction, int dustType = DustID.BlueTorch, int count = 10, float speed = 4f, float scale = 1.2f)
		{
			for (int i = 0; i < count; i++)
			{
				Vector2 velocity = direction.RotatedByRandom(0.6) * speed * Main.rand.NextFloat(0.5f, 1f);
				int index = Dust.NewDust(position, 1, 1, dustType, velocity.X, velocity.Y, Scale: scale);
				Main.dust[index].noGravity = true;
			}
		}

		// Animated-sprite burst effects (Content/Projectiles/Bursts/) - one per element/damage
		// family, each a BurstEffectProjectile. Netcode note: unlike Dust (always client-local, no
		// sync needed), these are real synced Projectiles, so every call site would double-spawn in
		// multiplayer if triggered from code that runs on every client (NPC AI, projectile AI/hit
		// callbacks) - same reasoning already applied to this mod's other NPC-spawned projectiles
		// (see MadaraBoss/PainBoss's `Main.netMode != NetmodeID.MultiplayerClient` guards). Centralizing
		// the guard here means every call site below stays a plain one-liner.
		private static void SpawnBurstEffect<T>(Vector2 position, float scale = 1f, float rotation = 0f) where T : ModProjectile
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			int type = ModContent.ProjectileType<T>();
			int index = Projectile.NewProjectile(new EntitySource_Misc("burst"), position, Vector2.Zero, type, 0, 0f);
			Main.projectile[index].scale = scale;
			Main.projectile[index].rotation = rotation;
		}

		public static void SpawnChakraBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<ChakraBurstProjectile>(position, scale, rotation);
		public static void SpawnGenjutsuBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<GenjutsuBurstProjectile>(position, scale, rotation);
		public static void SpawnFireBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<FireBurstProjectile>(position, scale, rotation);
		public static void SpawnLightningBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<LightningBurstProjectile>(position, scale, rotation);
		public static void SpawnWindBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<WindBurstProjectile>(position, scale, rotation);
		public static void SpawnEarthBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<EarthBurstProjectile>(position, scale, rotation);
		public static void SpawnCoreBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<CoreBurstProjectile>(position, scale, rotation);
		public static void SpawnWaterBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<WaterBurstProjectile>(position, scale, rotation);
		public static void SpawnIceBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<IceBurstProjectile>(position, scale, rotation);
		public static void SpawnBoneBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<BoneBurstProjectile>(position, scale, rotation);
		public static void SpawnCorruptionBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<CorruptionBurstProjectile>(position, scale, rotation);
		public static void SpawnSmokeBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<SmokeBurstProjectile>(position, scale, rotation);
		public static void SpawnSharinganBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<SharinganBurstProjectile>(position, scale, rotation);
		public static void SpawnHealingBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<HealingBurstProjectile>(position, scale, rotation);
		public static void SpawnWoodBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<WoodBurstProjectile>(position, scale, rotation);
		public static void SpawnImpactBurst(Vector2 position, float scale = 1f, float rotation = 0f) => SpawnBurstEffect<ImpactBurstProjectile>(position, scale, rotation);

		// Player-tracking counterpart to SpawnBurstEffect<T> above, for the large Taijutsu kick/punch
		// "blast" VFX that needs to follow the owner through their swing instead of flashing once at
		// a fixed point. `offset` is in the owner's un-flipped facing direction (+X = in front of
		// them); PlayerAnchoredBurstEffectProjectile flips it per-tick off the owner's actual
		// direction. Same MP double-spawn guard as SpawnBurstEffect<T>.
		public static void SpawnPlayerAnchoredBurst<T>(Player owner, Vector2 offset) where T : ModProjectile
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			int type = ModContent.ProjectileType<T>();
			Projectile.NewProjectile(new EntitySource_Misc("playerAnchoredBurst"), owner.Center, Vector2.Zero, type, 0, 0f, owner.whoAmI, offset.X, offset.Y);
		}

		// The log a Substitution dodge leaves behind - see SubstitutionLogProjectile. Given a small
		// upward hop plus a bit of the incoming hit's own sideways kick so it visibly tumbles instead
		// of just dropping straight down. Same multiplayer double-spawn guard as SpawnBurstEffect.
		public static void SpawnSubstitutionLog(Vector2 position, int hitDirection)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			Vector2 velocity = new(hitDirection * Main.rand.NextFloat(1.5f, 3f), -Main.rand.NextFloat(4f, 5.5f));
			Projectile.NewProjectile(new EntitySource_Misc("substitutionLog"), position, velocity, ModContent.ProjectileType<SubstitutionLogProjectile>(), 0, 0f);
		}
	}
}
