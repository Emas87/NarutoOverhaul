using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Fully custom AI (aiStyle = -1) so this reads as a jutsu rather than a reskinned vanilla projectile:
	// a player-anchored windup -> active thrust -> retract state machine, spinning independent of travel direction.
	public class RasenganProjectile : ModProjectile
	{
		private enum ThrustState
		{
			Windup,
			Active,
			Retract
		}

		private const int WindupTicks = 10;
		private const int ActiveTicks = 20;
		private const int RetractTicks = 8;

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

		// nano-banana-generated 9-frame spin sheet - cycles continuously while held, layered on
		// top of the existing whole-sprite rotation below rather than replacing it.
		private const int FrameCount = 9;
		private const int TicksPerFrame = 4;
		private int animFrame;
		private int animTicks;

		public override void SetDefaults()
		{
			Projectile.width = 28;
			Projectile.height = 28;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<NinjutsuDamageClass>();
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

			Projectile.rotation += 0.4f;

			float thrustDistance = State switch
			{
				ThrustState.Windup => MathHelper.Lerp(20f, 48f, StateTimer / WindupTicks),
				ThrustState.Active => 48f,
				_ => MathHelper.Lerp(48f, 20f, StateTimer / RetractTicks),
			};

			var direction = new Vector2(owner.direction, 0f);
			Projectile.Center = owner.Center + direction * thrustDistance;
			Projectile.spriteDirection = owner.direction;

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
			Common.VFX.ChakraVFX.SpawnChakraBurst(target.Center, 1.2f);
		}
	}
}
