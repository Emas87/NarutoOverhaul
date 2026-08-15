using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Minions
{
	// Orochimaru/Sasuke's snake contract - crawls the ground like the rest of the pack, lunges then
	// retreats on the attack pass instead of a straight chase.
	public class SnakeMinionProjectile : AnimalMinionProjectile
	{
		protected override int BuffType => ModContent.BuffType<SnakeMinionBuff>();
		protected override float MoveSpeed => 7f;
		protected override float AttackRange => 500f;
		protected override int IdleFrameCount => 5;
		protected override int AttackFrameCount => 8;

		private int lungeTimer;
		private bool retreating;

		protected override void SetMinionSize()
		{
			Projectile.width = 32;
			Projectile.height = 18;
		}

		protected override void SetMinionDefaults()
		{
			Projectile.damage = 16;
			Projectile.knockBack = 3f;
		}

		protected override void UpdateVisuals(Player owner, NPC target)
		{
			if (target != null)
			{
				lungeTimer++;

				if (lungeTimer >= 30)
				{
					retreating = !retreating;
					lungeTimer = 0;
				}

				if (retreating)
				{
					// -spriteDirection (not -velocity.X) so a retreat that starts exactly when
					// velocity.X is momentarily 0 (e.g. just landed a hop) still picks a direction.
					Projectile.velocity.X = -Projectile.spriteDirection * MoveSpeed * 0.75f;
					Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;
				}
			}
			else
			{
				lungeTimer = 0;
				retreating = false;
			}
		}
	}
}
