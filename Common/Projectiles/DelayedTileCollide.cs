using Microsoft.Xna.Framework;
using Terraria;

namespace NarutoOverhaul.Common.Projectiles
{
	// Thrown/shot projectiles spawn overlapping whatever tile the caster is standing on, so turning
	// tileCollide on immediately makes them collide with that tile the instant they're cast/thrown.
	// Call Update(Projectile) once per AI tick with tileCollide left off in SetDefaults - it flips
	// tileCollide on once the projectile has actually traveled clear of its spawn point (or after
	// fallbackTicks, as a safety net for anything that spawns with near-zero velocity).
	public class DelayedTileCollide
	{
		private readonly float clearanceDistance;
		private readonly int fallbackTicks;
		private Vector2 spawnCenter;
		private bool spawnCenterSet;
		private int ticksAlive;

		public DelayedTileCollide(float clearanceDistance, int fallbackTicks = 30)
		{
			this.clearanceDistance = clearanceDistance;
			this.fallbackTicks = fallbackTicks;
		}

		public void Update(Projectile projectile)
		{
			ticksAlive++;

			if (!spawnCenterSet)
			{
				spawnCenter = projectile.Center;
				spawnCenterSet = true;
			}

			if (!projectile.tileCollide
				&& (Vector2.Distance(projectile.Center, spawnCenter) >= clearanceDistance || ticksAlive >= fallbackTicks))
			{
				projectile.tileCollide = true;
			}
		}
	}
}
