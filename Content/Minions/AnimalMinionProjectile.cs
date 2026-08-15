using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Minions
{
	// Shared minion AI for all 6 animal summoning scrolls (idle-follow-owner vs. acquire-and-attack
	// the nearest enemy near the owner) - kept as an abstract base with thin subclasses rather than
	// one shared class + a "kind" parameter (like ElementalBoltProjectile) because each minion needs
	// its own independently-tracked buff for vanilla's "already have this summon out" replacement
	// logic to work correctly per animal.
	//
	// Ground-based like vanilla's own walking pets/minions (Parrot, Pygmy - both driven by
	// Projectile.AI_026) instead of hovering: tileCollide=true + Main.projPet=true is what makes a
	// projectile skip the "die on any tile touch" behavior normal projectiles get with
	// tileCollide=true, and manual gravity + a per-tick Collision.StepUp call (same combo AI_026
	// uses) resolves the actual ground/slope collision - StepUp hard-snaps position.Y to the tile
	// surface when the overlap is <=1 tile, easing the visual jolt into Projectile.gfxOffY instead
	// of jumping the hitbox. A wall-ahead look-ahead + hop (also lifted from AI_026) clears 1-2
	// tile steps that StepUp alone can't climb.
	//
	// Two prior versions got this wrong: tileCollide=false + StepUp alone had no real floor
	// collision at all (StepUp only nudges you up a ledge you're already walking into, it doesn't
	// stop you sinking through flat ground), so minions sank through the world and fell out of
	// view a few seconds after spawning while still technically alive (still owned, still
	// refreshing their buff) - looked like a vanish, but the buff icon never cleared. Then
	// tileCollide=true + Main.projPet=true alone (relying only on the engine's automatic
	// post-AI() Collision.SlopeCollision, called with gravity=0) stopped the falling-through-world
	// bug but is a soft resolver, not a hard snap - minions rested visibly half-buried in the tile
	// underneath them, and had no way to climb a small wall in front of them.
	//
	// The remaining sink (still visible even with StepUp hard-snapping the hitbox to the tile
	// surface every tick) turned out to be a draw bug, not a collision bug: with no PreDraw
	// override, vanilla's default multi-frame draw anchors the sprite using half the HITBOX
	// height as its origin, not half the actual frame height (see KunaiProjectile.cs/
	// WaterDragonProjectile.cs for other instances of this same vanilla quirk) - and these sheets'
	// 56px-tall frames are much taller than the ~20px hitboxes set in each subclass's
	// SetMinionSize(). That mismatch put the hitbox's ground line ~34px up from where the
	// character's actual feet are drawn in the frame (confirmed by checking the generated sheets'
	// alpha bounding boxes - every one of them has the character's feet landing 2px from the
	// frame's bottom edge, regardless of animal), i.e. over 2 tiles of the sprite rendered below
	// the point the physics considers "the ground". PreDraw below anchors the frame's actual
	// bottom (feet) to the hitbox's bottom (Projectile.Bottom) instead, which is what fixes it -
	// changing SetMinionSize()'s hitbox to be taller would only ever have been a coincidental
	// partial fix, not really shrinking the mismatch by design.
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

		private const float Gravity = 0.4f;
		private const float MaxFallSpeed = 10f;

		// Every generated sheet (Idle/Attack blocks alike) has the character's feet landing exactly
		// this many pixels above the frame's bottom edge - see the class comment above for how this
		// was measured. Used by PreDraw to anchor feet to the physics ground line instead of vanilla's
		// default hitbox-center anchor.
		private const float VisualBottomPaddingPx = 2f;

		private bool inAttackBlock;
		private int animFrame;
		private int animTicks;
		private float stepSpeed;

		// True on ticks where the minion is resting on solid ground (velocity.Y settled to 0 by
		// Collision.StepUp) - lets subclasses (e.g. Toad's hop) only jump when actually grounded
		// instead of stacking jumps mid-air.
		protected bool Grounded { get; private set; }

		public sealed override void SetStaticDefaults()
		{
			// Marks this as a "pet"-style projectile to the engine: with tileCollide also true,
			// this is what makes Collision.SlopeCollision run automatically after AI() instead of
			// the normal "die on tile contact" collision every other projectile gets.
			Main.projPet[Type] = true;
		}

		public sealed override void SetDefaults()
		{
			SetMinionSize();
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
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

		public sealed override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
			Vector2 origin = new(frame.Width / 2f, frame.Height - VisualBottomPaddingPx);
			Vector2 drawPosition = Projectile.Bottom - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
			SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effects, 0);

			return false;
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];

			if (!owner.active || owner.dead)
			{
				owner.ClearBuff(BuffType);
				Projectile.Kill();
				return;
			}

			if (owner.HasBuff(BuffType))
			{
				Projectile.timeLeft = 2;
			}

			NPC target = FindTarget(owner);

			if (target != null)
			{
				ChaseTarget(target);
			}
			else
			{
				FollowOwner(owner);
			}

			ApplyGravityAndGroundCollision(owner, target);

			if (Projectile.velocity.X != 0f)
			{
				Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;
			}

			UpdateVisuals(owner, target);
			UpdateAnimationFrame(target != null);
		}

		private void ApplyGravityAndGroundCollision(Player owner, NPC target)
		{
			bool wallAhead = IsWallAhead();

			Projectile.velocity.Y += Gravity;
			if (Projectile.velocity.Y > MaxFallSpeed)
			{
				Projectile.velocity.Y = MaxFallSpeed;
			}

			// StepUp hard-snaps position.Y to the tile surface for steps <=1 tile and feeds the
			// visual correction into gfxOffY (eased back to 0 over time by the engine's normal
			// projectile draw code) instead of jumping the hitbox - this is what stops the minion
			// rendering sunk into the ground.
			Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref stepSpeed, ref Projectile.gfxOffY);
			Grounded = Projectile.velocity.Y == 0f;

			if (Grounded && wallAhead)
			{
				// StepUp alone only climbs steps <=1 tile - still blocked after it ran means the
				// obstacle is taller, so hop clear of it instead of walking in place at the wall.
				int footTileX = (int)(Projectile.Center.X / 16f);
				int footTileY = (int)((Projectile.position.Y + Projectile.height) / 16f);

				Projectile.velocity.Y = !WorldGen.SolidTile(footTileX, footTileY - 2)
					? -7f
					: -10f;
			}
			else if (Grounded)
			{
				// The owner (or the target, mid-chase) is well above and blocked by terrain -
				// hop up toward them instead of walking in place at the base of a wall/ledge.
				Vector2 followPoint = target != null ? target.Center : owner.Center;
				float heightGap = Projectile.Center.Y - followPoint.Y;
				float horizontalGap = System.Math.Abs(Projectile.Center.X - followPoint.X);

				if (heightGap > 64f && horizontalGap < 200f)
				{
					Projectile.velocity.Y = -7f;
				}
			}
		}

		// One-tile look-ahead in the direction of horizontal movement, at the minion's foot row -
		// mirrors vanilla's Projectile.AI_026 blocked-ahead check that gates its own StepUp/jump.
		private bool IsWallAhead()
		{
			if (Projectile.velocity.X == 0f)
			{
				return false;
			}

			int direction = System.Math.Sign(Projectile.velocity.X);
			int aheadTileX = (int)((Projectile.Center.X + direction * (Projectile.width / 2 + 4)) / 16f);
			int footTileY = (int)((Projectile.position.Y + Projectile.height - 1) / 16f);

			return WorldGen.SolidTile(aheadTileX, footTileY);
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
			Vector2 idlePoint = owner.Center + new Vector2(-owner.direction * 40f, 0f);
			float distanceX = idlePoint.X - Projectile.Center.X;

			if (System.Math.Abs(owner.Center.Y - Projectile.Center.Y) > 2000f || System.Math.Abs(distanceX) > 2000f)
			{
				// left behind too far (e.g. owner teleported) - snap back instead of a long walk/climb
				Projectile.Center = idlePoint;
				Projectile.velocity = Vector2.Zero;
				return;
			}

			Projectile.velocity.X = System.Math.Abs(distanceX) > 30f
				? System.Math.Sign(distanceX) * MoveSpeed
				: Projectile.velocity.X * 0.8f;
		}

		private void ChaseTarget(NPC target)
		{
			float distanceX = target.Center.X - Projectile.Center.X;
			Projectile.velocity.X = System.Math.Sign(distanceX) * MoveSpeed * 1.5f;
		}

		// Hook for subclasses' attack quirks (lunge/retreat/heal/etc.) - purely cosmetic/behavioral
		// tweaks on top of the horizontal chase velocity ApplyGravityAndGroundCollision already set;
		// vertical movement (gravity, hopping, stepping) is handled by the base class.
		protected virtual void UpdateVisuals(Player owner, NPC target)
		{
		}
	}
}
