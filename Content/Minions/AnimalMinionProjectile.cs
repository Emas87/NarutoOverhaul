using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Minions
{
	// Shared minion AI for all 4 animal summoning scrolls (idle-follow-owner vs. acquire-and-attack
	// the nearest enemy near the owner) - kept as an abstract base with thin subclasses rather than
	// one shared class + a "kind" parameter (like ElementalBoltProjectile) because each minion needs
	// its own independently-tracked buff for vanilla's "already have this summon out" replacement
	// logic to work correctly per animal.
	public abstract class AnimalMinionProjectile : ModProjectile
	{
		protected abstract int BuffType { get; }
		protected abstract float MoveSpeed { get; }
		protected abstract float AttackRange { get; }

		// nano-banana-generated sheets: idle/follow block, then attack/chase block, stacked in that
		// order. Frame counts differ per animal, so each subclass supplies its own. ModProjectile has
		// no FindFrame hook (unlike ModNPC) - animation is driven by hand from AI() instead.
		protected abstract int IdleFrameCount { get; }
		protected abstract int AttackFrameCount { get; }
		protected virtual int IdleTicksPerStep => 8;
		protected virtual int AttackTicksPerStep => 6;

		private bool inAttackBlock;
		private int animFrame;
		private int animTicks;

		public sealed override void SetDefaults()
		{
			SetMinionSize();
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.netImportant = true;
			Projectile.DamageType = DamageClass.Summon;
			Main.projFrames[Projectile.type] = IdleFrameCount + AttackFrameCount;

			SetMinionDefaults();
		}

		protected abstract void SetMinionSize();

		// Hook for subclasses that need extra SetDefaults values (damage, knockback, etc.)
		protected virtual void SetMinionDefaults()
		{
		}

		public sealed override bool MinionContactDamage() => true;

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];

			if (owner.dead || !owner.active)
			{
				owner.ClearBuff(BuffType);
			}

			if (owner.HasBuff(BuffType))
			{
				Projectile.timeLeft = 2;
			}

			NPC target = FindTarget(owner);

			if (target != null)
			{
				AttackTarget(target);
			}
			else
			{
				FollowOwner(owner);
			}

			if (Projectile.velocity.X != 0f)
			{
				Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;
			}

			UpdateVisuals(owner, target);
			UpdateAnimationFrame(target != null);
		}

		private void UpdateAnimationFrame(bool attacking)
		{
			if (attacking != inAttackBlock)
			{
				inAttackBlock = attacking;
				animFrame = 0;
				animTicks = 0;
			}

			int frameCount = inAttackBlock ? AttackFrameCount : IdleFrameCount;
			int ticksPerStep = inAttackBlock ? AttackTicksPerStep : IdleTicksPerStep;

			animTicks++;
			if (animTicks >= ticksPerStep)
			{
				animTicks = 0;
				animFrame = (animFrame + 1) % frameCount;
			}

			int frameStart = inAttackBlock ? IdleFrameCount : 0;
			Projectile.frame = frameStart + animFrame;
		}

		private NPC FindTarget(Player owner)
		{
			NPC closest = null;
			float closestDistance = AttackRange;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];

				if (!npc.CanBeChasedBy(Projectile, false))
				{
					continue;
				}

				float distance = Vector2.Distance(npc.Center, owner.Center);

				if (distance < closestDistance)
				{
					closest = npc;
					closestDistance = distance;
				}
			}

			return closest;
		}

		private void FollowOwner(Player owner)
		{
			Vector2 idlePoint = owner.Center + new Vector2(-owner.direction * 40f, -10f);
			Vector2 toIdle = idlePoint - Projectile.Center;
			float distance = toIdle.Length();

			if (distance > 800f)
			{
				// left behind too far (e.g. owner teleported) - snap back instead of a long chase
				Projectile.Center = idlePoint;
				Projectile.velocity = Vector2.Zero;
				return;
			}

			Projectile.velocity = distance > 30f
				? toIdle.SafeNormalize(Vector2.Zero) * MoveSpeed
				: Projectile.velocity * 0.9f;
		}

		private void AttackTarget(NPC target)
		{
			Vector2 toTarget = target.Center - Projectile.Center;
			Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * MoveSpeed * 1.5f;
		}

		// Hook for subclasses' movement quirks (hop/slither/etc.) and any support effects.
		protected virtual void UpdateVisuals(Player owner, NPC target)
		{
		}
	}
}
