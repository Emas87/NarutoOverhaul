using Microsoft.Xna.Framework;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Minions
{
	// Naruto/Jiraiya's toad contract - hops rather than flying smoothly.
	public class ToadMinionProjectile : AnimalMinionProjectile
	{
		protected override int BuffType => ModContent.BuffType<ToadMinionBuff>();
		protected override float MoveSpeed => 6f;
		protected override float AttackRange => 500f;
		protected override int IdleFrameCount => 4;
		protected override int AttackFrameCount => 6;

		private int hopTimer;

		protected override void SetMinionSize()
		{
			Projectile.width = 30;
			Projectile.height = 24;
		}

		protected override void SetMinionDefaults()
		{
			Projectile.damage = 14;
			Projectile.knockBack = 3f;
		}

		protected override void UpdateVisuals(Player owner, NPC target)
		{
			hopTimer++;

			if (hopTimer >= 25)
			{
				Projectile.velocity.Y -= 5f;
				hopTimer = 0;
			}

			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}
	}
}
