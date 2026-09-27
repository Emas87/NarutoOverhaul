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
		private int retreatDirection;

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
					if (retreating)
					{
						// Pick the retreat direction once, when the phase starts: away from the facing
						// at that moment (-spriteDirection, not -velocity.X, so a retreat that starts
						// while velocity.X is momentarily 0 still gets a direction). Re-deriving it
						// every tick flipped it every tick, so the snake vibrated instead of retreating.
						retreatDirection = Projectile.spriteDirection == 1 ? -1 : 1;
					}
				}

				if (retreating)
				{
					Projectile.velocity.X = retreatDirection * MoveSpeed * 0.75f;
					Projectile.spriteDirection = retreatDirection;
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
