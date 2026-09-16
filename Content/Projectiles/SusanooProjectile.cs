using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Content.Projectiles
{
	// Madara's avatar technique, scaled down to "one giant reaching strike" rather than a full
	// summoned-entity implementation - same player-anchored thrust state machine as
	// RasenganProjectile/ChidoriProjectile, just far larger and slower to reflect its weight.
	public class SusanooProjectile : ModProjectile
	{
		private enum ThrustState
		{
			Windup,
			Active,
			Retract
		}

		private const int WindupTicks = 16;
		private const int ActiveTicks = 26;
		private const int RetractTicks = 12;

		private ThrustState State
		{
			get => (ThrustState)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}

		private float StateTimer
		{
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		private const int FrameCount = 6;
		private const int TicksPerFrame = 8;
		private int animFrame;
		private int animTicks;

		public override void SetDefaults()
		{
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<GenjutsuDamageClass>();
			Projectile.penetrate = -1;
			Projectile.timeLeft = WindupTicks + ActiveTicks + RetractTicks;
			Projectile.tileCollide = false;
			Main.projFrames[Projectile.type] = FrameCount;
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];

			if (!owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}

			float thrustDistance = State switch
			{
				ThrustState.Windup => MathHelper.Lerp(30f, 90f, StateTimer / WindupTicks),
				ThrustState.Active => 90f,
				_ => MathHelper.Lerp(90f, 30f, StateTimer / RetractTicks),
			};

			// Item.useStyle = Shoot already aims the spawn velocity at the mouse (shootSpeed = 1f,
			// so this is a unit vector) - AI() never touches Projectile.velocity again after spawn,
			// so it stays a stable aim direction for the whole thrust. Previously this was hardcoded
			// to new Vector2(owner.direction, 0f), which is why aiming above/below the player still
			// only ever thrust sideways.
			Vector2 direction = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
			Projectile.Center = owner.Center + direction * thrustDistance;
			Projectile.spriteDirection = direction.X < 0 ? -1 : 1;
			// Flip-safe rotation: the sprite is authored facing right, spriteDirection mirrors it
			// horizontally for leftward aims, so the rotation angle needs a matching 180-degree
			// correction on that side or the avatar would render upside-down when aiming up-left/down-left.
			Projectile.rotation = direction.ToRotation() + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f);

			if (Main.rand.NextBool(2))
			{
				Common.VFX.ChakraVFX.SpawnBurstEffect<GenjutsuBurstProjectile>(Projectile.Center, 0.6f);
			}

			StateTimer++;

			switch (State)
			{
				case ThrustState.Windup when StateTimer >= WindupTicks:
					State = ThrustState.Active;
					StateTimer = 0f;
					break;
				case ThrustState.Active when StateTimer >= ActiveTicks:
					State = ThrustState.Retract;
					StateTimer = 0f;
					break;
				case ThrustState.Retract when StateTimer >= RetractTicks:
					Projectile.Kill();
					break;
			}

			animTicks++;
			if (animTicks >= TicksPerFrame)
			{
				animTicks = 0;
				animFrame = (animFrame + 1) % FrameCount;
			}
			Projectile.frame = animFrame;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Common.VFX.ChakraVFX.SpawnBurstEffect<GenjutsuBurstProjectile>(target.Center, 2.5f);
		}

		// The 170x130 sprite sheet frame is much bigger than the 64x64 hitbox. Terraria's default
		// multi-frame draw anchors the texture using half the HITBOX size as its origin, not half
		// the frame size, so with a mismatch this size it draws the avatar shifted well away from
		// Projectile.Center - combined with the (now-removed) continuous Projectile.rotation spin,
		// this is what read as "rotating around a different pivot than the player." Same fix as
		// WaterDragonProjectile's PreDraw: draw it ourselves with the origin at the frame's own
		// center.
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame(1, FrameCount, 0, Projectile.frame);
			Vector2 origin = frame.Size() / 2f;
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, effects, 0);

			return false;
		}
	}
}
