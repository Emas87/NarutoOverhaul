using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Shared base for Taijutsu's kick weapons (Iron Leg / Leaf Hurricane / Front Lotus) - the melee
	// damage now lives on a player-anchored, invisible projectile instead of the item's own hitbox,
	// same "anchor a projectile to the player and drive the AI off it" pattern ChidoriProjectile/
	// RasenganProjectile already use for Ninjutsu. The reason to bother: this is what actually lets
	// us rotate the player's whole body through the kick's arc (via Player.fullRotation, driven by
	// this projectile's own AI every tick) instead of just swapping a static leg frame - verified
	// against GumGum's (a One Piece mod) AntiMannerKickCourse, which does exactly this for its
	// "Black Leg" fighting style. Player.fullRotation must be reset to 0 on every exit path or the
	// player's sprite is left visibly tilted after the swing ends.
	public abstract class TaijutsuKickProjectile : ModProjectile
	{
		protected abstract float SweepLimitDegrees { get; }
		protected abstract int SweepTicks { get; }

		// The actual damage hitbox (square, centered on the player). Overridable per weapon so a
		// wider/longer kick actually reaches as far as its sweep animation implies - was flat 30 for
		// every kick regardless of reach, which read as "the kick whiffs even when it visibly lands."
		protected virtual int HitboxSize => 30;

		// Forward shove applied to the player on a successful hit - the "dash through the kick"
		// feel. 0 disables it for weapons that shouldn't lunge.
		protected virtual float DashSpeed => 0f;

		public override string Texture => "NarutoOverhaul/Content/Projectiles/TaijutsuKickBlank";

		public sealed override void SetDefaults()
		{
			Projectile.width = HitboxSize;
			Projectile.height = HitboxSize;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<TaijutsuDamageClass>();
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = SweepTicks;
		}

		public sealed override bool PreDraw(ref Color lightColor) => false;

		public sealed override void AI()
		{
			Player owner = Main.player[Projectile.owner];

			if (!owner.active || owner.dead)
			{
				ResetPlayerRotation(owner);
				Projectile.Kill();
				return;
			}

			float progress = 1f - Projectile.timeLeft / (float)SweepTicks;
			float rotation = MathHelper.ToRadians(SweepLimitDegrees) * progress * owner.direction;

			Projectile.Center = owner.Center;
			owner.fullRotationOrigin = owner.Size * 0.5f;
			owner.fullRotation = rotation;
			Projectile.rotation = rotation;

			OnSweep(owner, progress);
		}

		public sealed override void OnKill(int timeLeft)
		{
			ResetPlayerRotation(Main.player[Projectile.owner]);
		}

		private static void ResetPlayerRotation(Player owner)
		{
			owner.fullRotation = 0f;
		}

		public sealed override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			ModifyKickHit(ref modifiers);
		}

		// Override for weapon-specific hit modifiers (armor penetration, extra knockback). No-op by
		// default.
		protected virtual void ModifyKickHit(ref NPC.HitModifiers modifiers)
		{
		}

		public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player owner = Main.player[Projectile.owner];

			// Only the owning client drives its own velocity - other clients pick the movement up
			// through normal position sync, same convention as TaijutsuKickAnimationPlayer's dash.
			if (DashSpeed > 0f && owner.whoAmI == Main.myPlayer)
			{
				owner.velocity.X = owner.direction * DashSpeed;
			}

			OnKickHit(owner, target);
		}

		// Called every AI tick with 0..1 sweep progress - override for mid-swing VFX (e.g. a flash
		// at the peak of the arc). Empty by default.
		protected virtual void OnSweep(Player owner, float progress)
		{
		}

		// Called on a successful hit for weapon-specific impact VFX.
		protected abstract void OnKickHit(Player owner, NPC target);
	}
}
