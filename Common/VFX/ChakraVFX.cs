using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace NarutoOverhaul.Common.VFX
{
	// Shared Dust-based VFX helpers so each new jutsu doesn't hand-roll its own dust loop.
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
	}
}
