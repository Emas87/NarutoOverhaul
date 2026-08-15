using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Minions
{
	// Naruto/Jiraiya's toad contract - ground-hops like vanilla's own Vampire Frog pet (aiStyle
	// CommonFollow) instead of flying smoothly.
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
			// Only hop while actually resting on ground - gating on Grounded (set by the base
			// class's gravity/Collision.StepUp step) stops this from stacking extra upward velocity
			// mid-air on top of a landing/step-up hop, which read as an erratic double-jump.
			if (Grounded)
			{
				hopTimer++;

				if (hopTimer >= 25)
				{
					Projectile.velocity.Y = -7f;
					hopTimer = 0;
				}
			}

			Projectile.rotation = Projectile.velocity.X * 0.05f;
		}
	}
}
