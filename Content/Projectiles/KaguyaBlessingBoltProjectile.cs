using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Content.Projectiles
{
	// Kaguya's blessing (Otsutsuki Chakra Fragment) manifesting during the Moon Lord fight - a
	// friendly homing bolt spawned by MoonLordAssistGlobalNPC, tracking whichever Moon Lord part's
	// NPC.whoAmI was passed in via ai[0] at spawn (same "pass state through ai0" convention already
	// used by ElementalBoltProjectile's BoltElement). Deals a flat, high, not-player-scaled amount of
	// damage so it reads as a fixed narrative reward rather than a build-dependent power spike.
	//
	// CanHitNPC below is what actually guarantees "only ever damages Moon Lord" - homing keeps it
	// aimed at the tracked part, but the hit-filter is the real enforcement, independent of how well
	// the homing tracks moment to moment.
	public class KaguyaBlessingBoltProjectile : ModProjectile
	{
		// No dedicated art - reuses Kaguya's own AshBoneProjectile texture (same bone/dimensional
		// palette) instead, matching the "reuse existing art via Texture override" convention already
		// used by MadaraFireballProjectile/KaguyaAshBoneProjectile this session. Without this, tML's
		// AutoStaticDefaults looks for a PNG at this class's own implicit path (none exists) and
		// fails to load the whole mod at startup - confirmed via the client.log MissingResourceException.
		public override string Texture => "NarutoOverhaul/Content/Projectiles/AshBoneProjectile";

		private const int TargetNpcIndex = 0; // ai[0]: the NPC.whoAmI this bolt tracks

		private const float HomingSpeed = 14f;
		private const float HomingTurnStrength = 0.12f;

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.ignoreWater = true;
			Projectile.scale = 1.5f;
		}

		public override void AI()
		{
			Projectile.rotation += 0.3f;

			int targetIndex = (int)Projectile.ai[TargetNpcIndex];
			NPC target = Main.npc[targetIndex];

			if (!target.active || !IsMoonLordPart(target.type))
			{
				Projectile.Kill();
				return;
			}

			Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity);
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * HomingSpeed, HomingTurnStrength);

			if (Main.rand.NextBool(2))
			{
				Common.VFX.ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(Projectile.Center, 0.5f);
			}
		}

		private static bool IsMoonLordPart(int npcType)
		{
			return npcType == NPCID.MoonLordHead || npcType == NPCID.MoonLordHand || npcType == NPCID.MoonLordCore;
		}

		// The actual "only ever damages Moon Lord" guarantee - independent of the homing above.
		public override bool? CanHitNPC(NPC target) => IsMoonLordPart(target.type) ? true : false;

		public override bool CanHitPlayer(Player target) => false;

		public override void OnKill(int timeLeft)
		{
			Common.VFX.ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(Projectile.Center, 1.2f);
		}
	}
}
