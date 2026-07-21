using Microsoft.Xna.Framework;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Minions
{
	// Orochimaru/Sasuke's snake contract - slithers with a sine-wave wobble, lunges then retreats
	// on the attack pass instead of a straight chase.
	public class SnakeMinionProjectile : AnimalMinionProjectile
	{
		protected override int BuffType => ModContent.BuffType<SnakeMinionBuff>();
		protected override float MoveSpeed => 7f;
		protected override float AttackRange => 500f;
		protected override int IdleFrameCount => 5;
		protected override int AttackFrameCount => 8;

		private float wobbleTimer;
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
			wobbleTimer += 0.2f;

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
					Projectile.velocity = -Projectile.velocity.SafeNormalize(Vector2.Zero) * MoveSpeed * 0.75f;
				}
			}
			else
			{
				lungeTimer = 0;
				retreating = false;
				Projectile.velocity.Y += (float)System.Math.Sin(wobbleTimer) * 0.5f;
			}

			Projectile.rotation = Projectile.velocity.ToRotation();
		}
	}
}
