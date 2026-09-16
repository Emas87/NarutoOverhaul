using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using NarutoOverhaul.Content.Items.Consumables;
using NarutoOverhaul.Content.Items.Materials;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Content.NPCs.Bosses
{
	// Fourth Shinobi War arc. Two-act fight: a normal humanoid phase, then a one-time transition
	// into a "Susanoo" avatar at 50% HP - represented via NPC.scale + a stat/attack upgrade rather
	// than a second sprite, so the giant-avatar feel doesn't depend on new art existing yet.
	// [AutoloadBossHead] registers MadaraBoss_Head_Boss.png as this boss's health-bar/minimap head icon.
	[AutoloadBossHead]
	public class MadaraBoss : ModNPC
	{
		private enum Phase
		{
			Base,
			Susanoo
		}

		private enum AttackState
		{
			Lunge,
			RangedBurst,
			Recover
		}

		private const int LungeTicks = 22;
		private const int RecoverTicks = 40;
		// Was 1.8f - the transition previously only read as "he got a bit bigger" rather than a real
		// transformation. Bumped for a much more dramatic jump, alongside the stronger tint/burst/
		// recurring-pulse changes below (see TransitionToSusanoo/GetAlpha/AI()).
		private const float SusanooScale = 2.2f;
		// Was 90 (1.5s) at scale 1.2f - the tint itself is persistent for the whole Susanoo phase,
		// but the pulse burst is a short-lived particle effect that fades within about a second by
		// nature, so a 1.5s gap of relative quiet between pulses read as occasional flashes rather
		// than a sustained transformation. Much more frequent + smaller scale reads as a continuous
		// ambient aura instead.
		private const int SusanooPulseTicks = 12;

		// Madara read as too small next to the other bosses. NPC.scale only affects the drawn sprite
		// (Entity.Hitbox uses raw width/height, not scale - see OrochimaruBoss/KakuzuBoss for the
		// same distinction), so both are scaled together via SetDefaults+PreDraw below to actually
		// grow the hitbox and not just the visual. Composes with the existing Susanoo transition
		// (see OnSpawn/TransitionToSusanoo) rather than being overridden by it - see those methods.
		private const float SizeMultiplier = 2f;

		// MadaraBoss.png's alpha bounding box sits flush against a consistent 2px bottom margin in
		// every frame (measured directly, same technique as OrochimaruBoss/KakuzuBoss/TailedBeastBoss)
		// - used by PreDraw below to anchor the sprite's feet to the hitbox bottom instead of
		// vanilla's default hitbox-center anchor.
		private const float VisualBottomPaddingPx = 2f;

		// DoLunge recomputes a full velocity toward the target from scratch every tick - when a
		// 1-tile obstacle blocks that path, vanilla's tile collision cancels velocity.X for that
		// tick, but the very next tick DoLunge immediately stomps it with the same blocked-direction
		// vector again before any step-climb correction can accumulate, so Madara just sits there
		// pressed against the block. TryHopOverObstacle is a small additive unstick valve on top of
		// vanilla's normal gravity/tile collision (which stays on), mirroring the fix already applied
		// to OrochimaruBoss (see there for the full writeup) and the wall-hop half of the pattern
		// proven for ground-walking minions in AnimalMinionProjectile.
		private const int HopCooldownTicks = 15;
		private const float BlockedVelocityThreshold = 1f;
		private const float HopImpulseSmall = -7f;
		private const float HopImpulseLarge = -10f;

		private int hopCooldown;
		private int susanooPulseTimer;

		// Universal "don't stay wedged/bouncing in place forever" safety net. "Stuck" covers two
		// cases: (1) the hitbox literally overlapping solid tiles (bad teleport/knockback), and
		// (2) repeatedly attempting to move (nonzero velocity) without ever making real net progress.
		// Being stationary while genuinely idle (casting/recovering, near-zero velocity) is NOT stuck
		// and must not trigger this, or a boss calmly casting could suddenly sink through the floor
		// for no reason. Once either condition holds for StuckToleranceTicks (3s) with less than
		// StuckMovementThreshold net displacement, nudge it to a nearby clear-air spot and let
		// gravity/collision (which stay ON throughout - never touching NPC.noTileCollide) settle it
		// onto whatever ground is below, same as any normal fall/landing. An earlier version used
		// NPC.noTileCollide to phase through terrain, but that disables collision in EVERY direction
		// including straight down, so a stuck boss fell clean through the floor instead of escaping
		// sideways/upward while still landing on solid ground - this reposition approach can't do
		// that since normal collision never turns off.
		private const int StuckToleranceTicks = 180;
		private const float StuckMovementThreshold = 60f;
		private const float MovingVelocityThreshold = 1.5f;
		private const float EscapeSearchRadius = 150f;
		private const int EscapeSearchAttempts = 8;
		private int stuckTimer;
		private Vector2 stuckWindowStartPosition;

		// Sheet layout from the nano-banana-generated MadaraBoss.png: idle(5)/melee(4)/cast(4).
		private const int IdleFrameStart = 0;
		private const int IdleFrameCount = 5;
		private const int IdleTicksPerStep = 8;

		private const int MeleeFrameStart = IdleFrameStart + IdleFrameCount;
		private const int MeleeFrameCount = 4;

		private const int CastFrameStart = MeleeFrameStart + MeleeFrameCount;
		private const int CastFrameCount = 4;
		private const int CastTicksPerStep = 6;

		private enum AnimBlock
		{
			Idle,
			Melee,
			Cast
		}

		private AnimBlock currentAnimBlock = AnimBlock.Idle;
		private int animFrame;
		private int animTicks;

		private Phase CurrentPhase
		{
			get => (Phase)NPC.ai[0];
			set => NPC.ai[0] = (float)value;
		}

		private AttackState CurrentAttack
		{
			get => (AttackState)NPC.ai[1];
			set => NPC.ai[1] = (float)value;
		}

		private float StateTimer
		{
			get => NPC.ai[2];
			set => NPC.ai[2] = value;
		}

		private float ShotsFired
		{
			get => NPC.ai[3];
			set => NPC.ai[3] = value;
		}

		private float LungeSpeed => CurrentPhase == Phase.Susanoo ? 18f : 13f;
		private int BurstShots => CurrentPhase == Phase.Susanoo ? 5 : 3;
		private int RecoverTicksForPhase => CurrentPhase == Phase.Susanoo ? RecoverTicks / 2 : RecoverTicks;

		public override void SetDefaults()
		{
			// Native (unscaled) size - do NOT pre-multiply by SizeMultiplier here. Vanilla's own
			// NPC.SetDefaults(int) unconditionally does `width = (int)(width * scale); height =
			// (int)(height * scale);` right after this override returns, so pre-multiplying as well
			// would double-apply SizeMultiplier to the hitbox only. See OrochimaruBoss/KakuzuBoss for
			// the same rule. NPC.scale itself is NOT set here - OnSpawn sets it (Susanoo transition
			// needs to multiply relative to whatever OnSpawn establishes as the Base-phase scale).
			NPC.width = 48;
			NPC.height = 64;
			NPC.damage = 42;
			NPC.defense = 28;
			NPC.lifeMax = 15000;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.knockBackResist = 0.03f;
			NPC.boss = true;
			NPC.npcSlots = 10f;
			NPC.aiStyle = -1;
			NPC.value = Item.buyPrice(gold: 32);
			Main.npcFrameCount[NPC.type] = IdleFrameCount + MeleeFrameCount + CastFrameCount;
		}

		public override void OnSpawn(IEntitySource source)
		{
			CurrentPhase = Phase.Base;
			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			ShotsFired = 0f;
			NPC.scale = SizeMultiplier;
		}

		public override void AI()
		{
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
			{
				NPC.TargetClosest();
			}

			Player target = Main.player[NPC.target];

			if (!target.active || target.dead)
			{
				NPC.velocity.Y -= 0.2f;
				NPC.EncourageDespawn(10);
				return;
			}

			UpdateStuckState();

			if (CurrentPhase == Phase.Base && (float)NPC.life / NPC.lifeMax <= 0.5f)
			{
				TransitionToSusanoo();
			}

			// The transition itself only fires once and its burst is a single-tick flash - easy to
			// miss entirely, after which nothing kept reminding the player Susanoo was still active
			// beyond the (previously quite subtle) tint. A small recurring pulse for the rest of the
			// fight keeps the transformation visually present instead of a one-frame blip. Uses its
			// own counter rather than StateTimer, which resets constantly on every attack-state
			// transition (every ~20-40 ticks) and would otherwise fire far more often than intended.
			if (CurrentPhase == Phase.Susanoo)
			{
				susanooPulseTimer++;

				if (susanooPulseTimer >= SusanooPulseTicks)
				{
					susanooPulseTimer = 0;
					ChakraVFX.SpawnBurstEffect<GenjutsuBurstProjectile>(NPC.Center, 0.5f);
				}
			}

			NPC.spriteDirection = target.Center.X < NPC.Center.X ? -1 : 1;

			if (hopCooldown > 0)
			{
				hopCooldown--;
			}

			switch (CurrentAttack)
			{
				case AttackState.Lunge:
					DoLunge(target);
					break;
				case AttackState.RangedBurst:
					DoRangedBurst(target);
					break;
				case AttackState.Recover:
					DoRecover();
					break;
			}

			if (CurrentAttack == AttackState.Lunge)
			{
				TryHopOverObstacle();
			}

			StateTimer++;
		}

		private void TransitionToSusanoo()
		{
			CurrentPhase = Phase.Susanoo;
			NPC.velocity = Vector2.Zero;
			// Composes with the SizeMultiplier baseline (2x) rather than overriding it, preserving the
			// relative SusanooScale jump on top of Madara's base size instead of dropping back down to
			// a flat SusanooScale.
			NPC.scale = SizeMultiplier * SusanooScale;

			// No position/hitbox recentering here (there used to be one) - NPC.width/height (the
			// actual hitbox) never change in this method, only the drawn scale, and PreDraw (see
			// below) already anchors the sprite at NPC.Bottom regardless of NPC.scale, so the visual
			// growth already happens outward from the fixed feet position with nothing to compensate
			// for. The old recentering line shoved the HITBOX itself up into the air by a large,
			// unjustified offset every transition (there being no matching hitbox growth to justify
			// it) - Madara would visually go airborne and have to fall back down, reading as
			// "still flies" after transforming.

			ChakraVFX.SpawnBurstEffect<GenjutsuBurstProjectile>(NPC.Center, 4f);
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			ShotsFired = 0f;
		}

		// NPC.scale isn't part of the standard sync packet, and a player joining mid-fight after
		// the Susanoo transition already happened would never run the transition code path at all
		// (it only fires once, off the phase-change check) - without this they'd see a full-size
		// boss forever. Syncing it directly sidesteps both problems.
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(NPC.scale);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			NPC.scale = reader.ReadSingle();
		}

		private void DoLunge(Player target)
		{
			Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
			NPC.velocity = toTarget * LungeSpeed;

			if (StateTimer >= LungeTicks)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private bool IsGrounded() => NPC.velocity.Y == 0f;

		private bool IsAreaClear(Vector2 topLeft, int width, int height)
		{
			int tileX1 = (int)(topLeft.X / 16f);
			int tileY1 = (int)(topLeft.Y / 16f);
			int tileX2 = (int)((topLeft.X + width) / 16f);
			int tileY2 = (int)((topLeft.Y + height) / 16f);

			for (int x = tileX1; x <= tileX2; x++)
			{
				for (int y = tileY1; y <= tileY2; y++)
				{
					if (WorldGen.SolidTile(x, y))
					{
						return false;
					}
				}
			}

			return true;
		}

		private Vector2 FindEscapeSpot()
		{
			for (int i = 0; i < EscapeSearchAttempts; i++)
			{
				Vector2 candidateCenter = NPC.Center + Main.rand.NextVector2CircularEdge(EscapeSearchRadius, EscapeSearchRadius);
				Vector2 candidateTopLeft = candidateCenter - new Vector2(NPC.width / 2f, NPC.height / 2f);

				if (IsAreaClear(candidateTopLeft, NPC.width, NPC.height))
				{
					return candidateCenter;
				}
			}

			return NPC.Center - new Vector2(0f, 150f);
		}

		private void UpdateStuckState()
		{
			bool embedded = !IsAreaClear(NPC.position, NPC.width, NPC.height);
			bool attemptingMovement = NPC.velocity.LengthSquared() > MovingVelocityThreshold * MovingVelocityThreshold;

			if (!embedded && !attemptingMovement)
			{
				stuckTimer = 0;
				return;
			}

			if (stuckTimer == 0)
			{
				stuckWindowStartPosition = NPC.Center;
			}

			stuckTimer++;

			if (Vector2.Distance(NPC.Center, stuckWindowStartPosition) >= StuckMovementThreshold)
			{
				stuckTimer = 0;
				return;
			}

			if (embedded || stuckTimer >= StuckToleranceTicks)
			{
				NPC.Center = FindEscapeSpot();
				NPC.velocity = Vector2.Zero;
				stuckTimer = 0;
			}
		}

		private bool IsWallAhead(int direction)
		{
			float aheadX = direction > 0 ? NPC.position.X + NPC.width + 4f : NPC.position.X - 4f;
			int tileX = (int)(aheadX / 16f);
			int footTileY = (int)((NPC.position.Y + NPC.height - 4f) / 16f);

			return WorldGen.SolidTile(tileX, footTileY);
		}

		private void TryHopOverObstacle()
		{
			if (hopCooldown > 0 || !IsGrounded())
			{
				return;
			}

			int direction = NPC.spriteDirection;

			if (System.Math.Abs(NPC.velocity.X) >= BlockedVelocityThreshold || !IsWallAhead(direction))
			{
				return;
			}

			int tileX = (int)((NPC.Center.X + direction * (NPC.width / 2f + 4f)) / 16f);
			int footTileY = (int)((NPC.position.Y + NPC.height - 4f) / 16f);
			bool clearAbove = !WorldGen.SolidTile(tileX, footTileY - 2);

			NPC.velocity.Y = clearAbove ? HopImpulseSmall : HopImpulseLarge;
			hopCooldown = HopCooldownTicks;
		}

		private void DoRangedBurst(Player target)
		{
			NPC.velocity *= 0.9f;

			if (StateTimer % 12 == 0 && ShotsFired < BurstShots)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 shotVelocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 8f;
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shotVelocity, ModContent.ProjectileType<MadaraFireballProjectile>(), 18, 1f);
				}

				ShotsFired++;
			}

			if (ShotsFired >= BurstShots)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoRecover()
		{
			NPC.velocity *= 0.9f;

			if (StateTimer >= RecoverTicksForPhase)
			{
				CurrentAttack = Main.rand.NextBool() ? AttackState.Lunge : AttackState.RangedBurst;
				StateTimer = 0f;
				ShotsFired = 0f;
			}
		}

		public override void FindFrame(int frameCounter)
		{
			int frameHeight = TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type];
			NPC.frame.Width = TextureAssets.Npc[NPC.type].Value.Width;
			NPC.frame.Height = frameHeight;

			AnimBlock targetBlock = CurrentAttack switch
			{
				AttackState.Lunge => AnimBlock.Melee,
				AttackState.RangedBurst => AnimBlock.Cast,
				_ => AnimBlock.Idle,
			};

			if (targetBlock != currentAnimBlock)
			{
				currentAnimBlock = targetBlock;
				animFrame = 0;
				animTicks = 0;
			}

			int frameStart;
			int frameIndex;

			switch (currentAnimBlock)
			{
				case AnimBlock.Melee:
					// A single lunge-and-recover beat, not a repeating cycle - play it once,
					// synced to how far through the lunge (LungeTicks) we are.
					float progress = MathHelper.Clamp(StateTimer / LungeTicks, 0f, 1f);
					frameStart = MeleeFrameStart;
					frameIndex = (int)(progress * (MeleeFrameCount - 1));
					break;
				case AnimBlock.Cast:
					frameStart = CastFrameStart;
					animTicks++;
					if (animTicks >= CastTicksPerStep)
					{
						animTicks = 0;
						animFrame = (animFrame + 1) % CastFrameCount;
					}
					frameIndex = animFrame;
					break;
				default:
					frameStart = IdleFrameStart;
					animTicks++;
					if (animTicks >= IdleTicksPerStep)
					{
						animTicks = 0;
						animFrame = (animFrame + 1) % IdleFrameCount;
					}
					frameIndex = animFrame;
					break;
			}

			NPC.frame.Y = (frameStart + frameIndex) * frameHeight;
		}

		// Vanilla's default NPC draw anchors the sprite's origin at half the HITBOX size, not half
		// the frame size - once the hitbox stops matching the frame (see SizeMultiplier above), that
		// mismatch reads as the sprite floating away from its own hitbox. Anchoring to the frame's own
		// (measured) bottom padding and drawing at NPC.Bottom instead keeps the feet planted on the
		// hitbox's bottom edge regardless of scale (including through the Susanoo scale jump, since
		// that only ever changes NPC.scale, not NPC.frame). Same pattern as OrochimaruBoss/KakuzuBoss.
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = TextureAssets.Npc[NPC.type].Value;
			Rectangle frame = NPC.frame;
			Vector2 origin = new(frame.Width / 2f, frame.Height - VisualBottomPaddingPx);
			SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Vector2 drawPosition = NPC.Bottom - screenPos + new Vector2(0f, NPC.gfxOffY);

			spriteBatch.Draw(texture, drawPosition, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, effects, 0f);

			return false;
		}

		// Susanoo transformation already reads via NPC.scale (SetDefaults/TransitionToSusanoo);
		// layering a purple aura tint on top gives it a bit more visual escalation without needing
		// the "separate model" ANIMATION_PIPELINE.md originally called for - reuses the same rig.
		public override Color? GetAlpha(Color drawColor)
		{
			if (CurrentPhase == Phase.Susanoo)
			{
				// Was 0.4f - too subtle to register as a real transformation on top of the scale
				// change alone (see the recurring pulse in AI() for the other half of this fix).
				return Color.Lerp(drawColor, new Color(150, 60, 200), 0.65f);
			}

			return null;
		}

		public override void OnKill()
		{
			StoryProgressSystem.DownedMadara = true;
			StoryProgressSystem.SyncToClients();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<SusanooCoreItem>(), 1, 3, 5));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<ChakraScroll6Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<StaminaScroll6Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.BossBag(ModContent.ItemType<MadaraBossBagItem>()));
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			ChakraVFX.SpawnBurstEffect<GenjutsuBurstProjectile>(NPC.Center, 0.5f);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				new FlavorTextBestiaryInfoElement("A reanimated legend who once manifested a titanic guardian of chakra around himself.")
			});
		}
	}
}
