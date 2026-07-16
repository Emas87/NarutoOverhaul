using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Projectiles
{
	// Kakashi's signature lightning-charged thrust - same player-anchored windup/active/retract
	// state machine as RasenganProjectile, just faster and shorter-ranged (a stabbing lunge rather
	// than a held sphere).
	public class ChidoriProjectile : ModProjectile
	{
		private enum ThrustState
		{
			Windup,
			Active,
			Retract
		}

		private const int WindupTicks = 6;
		private const int ActiveTicks = 10;
		private const int RetractTicks = 6;

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
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = ModContent.GetInstance<NinjutsuDamageClass>();
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

			float thrustDistance = State switch
			{
				ThrustState.Windup => MathHelper.Lerp(10f, 40f, StateTimer / WindupTicks),
				ThrustState.Active => 40f,
				_ => MathHelper.Lerp(40f, 10f, StateTimer / RetractTicks),
			};

			var direction = new Vector2(owner.direction, 0f);
			Projectile.Center = owner.Center + direction * thrustDistance;
			Projectile.spriteDirection = owner.direction;
			Projectile.rotation = owner.direction > 0 ? 0f : MathHelper.Pi;

			if (Main.rand.NextBool(2))
			{
				Common.VFX.ChakraVFX.SpawnBurst(Projectile.Center, DustID.Electric, 2, 1f);
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
			Common.VFX.ChakraVFX.SpawnBurst(target.Center, DustID.Electric, 10, 1.4f);
		}
	}
}
