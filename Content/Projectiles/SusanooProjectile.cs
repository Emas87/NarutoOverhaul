using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];

			if (!owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}

			Projectile.rotation += 0.1f;

			float thrustDistance = State switch
			{
				ThrustState.Windup => MathHelper.Lerp(30f, 90f, StateTimer / WindupTicks),
				ThrustState.Active => 90f,
				_ => MathHelper.Lerp(90f, 30f, StateTimer / RetractTicks),
			};

			var direction = new Vector2(owner.direction, 0f);
			Projectile.Center = owner.Center + direction * thrustDistance;
			Projectile.spriteDirection = owner.direction;

			if (Main.rand.NextBool(2))
			{
				Common.VFX.ChakraVFX.SpawnBurst(Projectile.Center, DustID.PurpleTorch, 3, 1.6f);
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
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Common.VFX.ChakraVFX.SpawnBurst(target.Center, DustID.PurpleTorch, 14, 1.8f);
		}
	}
}
