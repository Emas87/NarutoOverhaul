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
	// Konoha Crush / Sasuke Retrieval arc final boss. Distinct from Haku (teleport+senbon) and
	// TailedBeastBoss (chase+charge): alternates a sword lunge with 3-way snake-projectile spreads,
	// and "substitutes" (teleports + partially heals, Orochimaru's resilience) once per phase
	// transition instead of just getting faster like the other bosses.
	// [AutoloadBossHead] registers OrochimaruBoss_Head_Boss.png as this boss's health-bar/minimap head icon.
	[AutoloadBossHead]
	public class OrochimaruBoss : ModNPC
	{
		private enum Phase
		{
			One,
			Two,
			Three
		}

		private enum AttackState
		{
			Lunge,
			SnakeSummon,
			Jump,
			Recover
		}

		private const int LungeTicks = 20;
		private const int RecoverTicks = 45;
		private const float SubstitutionHealFraction = 0.08f;

		// DoLunge recomputes a full velocity toward the target from scratch every tick - when a
		// 1-tile obstacle blocks that path, vanilla's tile collision cancels velocity.X for that
		// tick, but the very next tick DoLunge immediately stomps it with the same blocked-direction
		// vector again before any step-climb correction can accumulate, so Orochimaru just sits there
		// pressed against the block ("getting stuck on one tile"). TryHopOverObstacle below is a
		// small additive unstick valve on top of vanilla's normal gravity/tile collision (which stays
		// on, unlike TailedBeastBoss's fully manual noGravity/noTileCollide movement - Orochimaru is
		// humanoid-scale and should still be blocked by real walls, just able to hop 1-2 tile steps),
		// mirroring the same wall-ahead-hop half of the pattern already proven for the ground-walking
		// minions in AnimalMinionProjectile.
		private const int HopCooldownTicks = 15;
		private const float BlockedVelocityThreshold = 1f;
		private const float HopImpulseSmall = -7f;
		private const float HopImpulseLarge = -10f;

		private int hopCooldown;

		// Universal "don't stay wedged/bouncing in place forever" safety net. "Stuck" covers two
		// cases: (1) the hitbox literally overlapping solid tiles (bad teleport/knockback), and
		// (2) repeatedly attempting to move (nonzero velocity) without ever making real net progress.
		// Being stationary while genuinely idle (casting/recovering, near-zero velocity) is NOT stuck
		// and must not trigger this, or a boss calmly casting could suddenly sink through the floor
		// for no reason. Once either condition holds for StuckToleranceTicks (3s) with less than
		// StuckMovementThreshold net displacement, nudge it to a nearby clear-air spot and let
		// gravity/collision (which stay ON throughout - never touching NPC.noTileCollide) settle it
		// onto whatever ground is below, same as any normal fall/landing. An earlier version of this
		// used NPC.noTileCollide to phase through terrain, but that disables collision in EVERY
		// direction including straight down, so a stuck boss fell clean through the floor instead of
		// escaping sideways/upward while still landing on solid ground - this reposition approach
		// can't do that since normal collision never turns off. Reuses the IsAreaClear/
		// FindClearTeleportCenter helpers already below (added for the safe-substitution fix).
		private const int StuckToleranceTicks = 180;
		private const float StuckMovementThreshold = 60f;
		private const float MovingVelocityThreshold = 1.5f;
		private const float EscapeSearchRadius = 150f;
		private int stuckTimer;
		private Vector2 stuckWindowStartPosition;

		// A telegraphed leap attack (EoC-style slam): plant for JumpTelegraphTicks, launch toward the
		// target with a single big velocity impulse, let vanilla's normal gravity/tile collision carry
		// the arc (same IsGrounded() landing test as TryHopOverObstacle above), then a landing burst +
		// knockback flourish - actual damage comes from ordinary contact during the leap/landing, same
		// as every other movement-based attack here (Lunge included), not a separate damage source.
		private const int JumpTelegraphTicks = 20;
		private const float JumpHorizontalSpeed = 9f;
		private const float JumpVerticalImpulse = -20f;
		private const float JumpImpactRadius = 110f;
		private const float JumpImpactKnockback = 8f;

		private bool jumpLaunched;

		// Orochimaru read as too small next to the other bosses. NPC.scale only affects the drawn
		// sprite (Entity.Hitbox uses raw width/height, not scale - see TailedBeastBoss/HakuBoss for
		// the same distinction), so both are scaled together via SetDefaults+PreDraw below to
		// actually grow the hitbox and not just the visual.
		private const float SizeMultiplier = 2f;

		// OrochimaruBoss.png's alpha bounding box sits flush against a consistent 2px bottom margin
		// in every frame (measured directly, same technique as TailedBeastBoss) - used by PreDraw
		// below to anchor the sprite's feet to the hitbox bottom instead of vanilla's default
		// hitbox-center anchor.
		private const float VisualBottomPaddingPx = 2f;

		// Sheet layout from the nano-banana-generated OrochimaruBoss.png: idle(4)/melee(6)/cast(6).
		private const int IdleFrameStart = 0;
		private const int IdleFrameCount = 4;
		private const int IdleTicksPerStep = 8;

		private const int MeleeFrameStart = IdleFrameStart + IdleFrameCount;
		private const int MeleeFrameCount = 6;

		private const int CastFrameStart = MeleeFrameStart + MeleeFrameCount;
		private const int CastFrameCount = 6;
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

		private float SnakesFired
		{
			get => NPC.ai[3];
			set => NPC.ai[3] = value;
		}

		private float LungeSpeed => CurrentPhase switch
		{
			Phase.One => 11f,
			Phase.Two => 14f,
			_ => 17f,
		};

		private int RecoverTicksForPhase => CurrentPhase == Phase.Three ? RecoverTicks / 2 : RecoverTicks;

		public override void SetDefaults()
		{
			// Native (unscaled) size - do NOT pre-multiply by SizeMultiplier here. Vanilla's own
			// NPC.SetDefaults(int) unconditionally does `width = (int)(width * scale); height =
			// (int)(height * scale);` right after this override returns, so pre-multiplying as well
			// would double-apply SizeMultiplier to the hitbox only, leaving it out of sync with the
			// sprite (which is scaled once, by PreDraw). See TailedBeastBoss for the same rule.
			NPC.width = 44;
			NPC.height = 60;
			NPC.scale = SizeMultiplier;
			NPC.damage = 38;
			NPC.defense = 22;
			NPC.lifeMax = 9000;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.knockBackResist = 0.05f;
			NPC.boss = true;
			NPC.npcSlots = 9f;
			NPC.aiStyle = -1;
			NPC.value = Item.buyPrice(gold: 18);
			Main.npcFrameCount[NPC.type] = IdleFrameCount + MeleeFrameCount + CastFrameCount;
		}

		public override void OnSpawn(IEntitySource source)
		{
			CurrentPhase = Phase.One;
			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			SnakesFired = 0f;

			// Soft vanilla-tier scaling, not a hard gate: Orochimaru is meant to loosely track
			// Skeletron as the pre-Hardmode "wall," so fighting him before that point is still
			// allowed but noticeably harder - a nudge toward the intended order, not a block.
			if (ModContent.GetInstance<NarutoOverhaulConfig>().EnableSoftBossScaling && !NPC.downedBoss3)
			{
				NPC.lifeMax = (int)(NPC.lifeMax * 1.5f);
				NPC.life = NPC.lifeMax;
				NPC.damage = (int)(NPC.damage * 1.3f);
				NPC.defense = (int)(NPC.defense * 1.2f);
			}
		}

		// OnSpawn runs server-side only; lifeMax isn't part of the standard NPC sync packet (only
		// life is), so without this a scaled-up Orochimaru shows a client-side HP bar that reads
		// well above 100% (client keeps the unscaled SetDefaults value while receiving the scaled
		// life). Damage/defense are already fine since combat resolution itself is server-authoritative.
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(NPC.lifeMax);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			NPC.lifeMax = reader.ReadInt32();
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

			UpdatePhase();
			UpdateStuckState();

			// Set once per tick regardless of attack state (matching MadaraBoss/PainBoss/KaguyaBoss)
			// instead of only inside DoLunge - previously spriteDirection went stale for the entire
			// Recover/SnakeSummon portion of the cycle while the body kept drifting on decayed lunge
			// velocity, reading as "moving backward" whenever the player crossed to the other side
			// during that window.
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
				case AttackState.SnakeSummon:
					DoSnakeSummon(target);
					break;
				case AttackState.Jump:
					DoJump(target);
					break;
				case AttackState.Recover:
					DoRecover();
					break;
			}

			// Only during Lunge: that's the only state whose purpose is closing distance with the
			// player (SnakeSummon is a stationary caster, Recover is a decaying settle) - gating here
			// avoids fighting either of those.
			if (CurrentAttack == AttackState.Lunge)
			{
				TryHopOverObstacle();
			}

			StateTimer++;
		}

		private void UpdatePhase()
		{
			float lifeRatio = (float)NPC.life / NPC.lifeMax;

			if (lifeRatio <= 0.33f && CurrentPhase != Phase.Three)
			{
				CurrentPhase = Phase.Three;
				DoSubstitution(Main.player[NPC.target]);
			}
			else if (lifeRatio <= 0.66f && CurrentPhase == Phase.One)
			{
				CurrentPhase = Phase.Two;
				DoSubstitution(Main.player[NPC.target]);
			}
		}

		private const float SubstitutionTeleportRadius = 260f;
		private const int TeleportClearAttempts = 8;

		private void DoSubstitution(Player target)
		{
			ChakraVFX.SpawnBurstEffect<CorruptionBurstProjectile>(NPC.Center, 2.5f);
			SoundEngine.PlaySound(SoundID.Item29, NPC.Center);

			NPC.Center = FindClearTeleportCenter(target.Center, SubstitutionTeleportRadius);
			NPC.velocity = Vector2.Zero;
			NPC.life = System.Math.Min(NPC.lifeMax, NPC.life + (int)(NPC.lifeMax * SubstitutionHealFraction));

			ChakraVFX.SpawnBurstEffect<CorruptionBurstProjectile>(NPC.Center, 2.5f);

			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			SnakesFired = 0f;
		}

		// The old code teleported straight to target.Center + a random offset with no check that the
		// landing spot was actually open - a substitution at a bad HP-threshold moment (player fighting
		// in a cave, tunnel, etc.) could land Orochimaru's hitbox partway inside solid terrain, where
		// he'd stay wedged permanently (no state ever un-embeds him - Recover/SnakeSummon never move
		// him, and TryHopOverObstacle only fires during Lunge and only helps when blocked ahead while
		// already grounded, not when already overlapping solid tiles). Retrying several random offsets
		// and only committing to one with a fully clear hitbox footprint fixes this at the source;
		// falling back to straight above the target (virtually always open air mid-fight) if every
		// attempt is blocked, rather than ever risking a blind teleport into terrain.
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
				NPC.Center = FindClearTeleportCenter(NPC.Center, EscapeSearchRadius);
				NPC.velocity = Vector2.Zero;
				stuckTimer = 0;
			}
		}

		private Vector2 FindClearTeleportCenter(Vector2 targetCenter, float radius)
		{
			for (int i = 0; i < TeleportClearAttempts; i++)
			{
				Vector2 candidateCenter = targetCenter + Main.rand.NextVector2CircularEdge(radius, radius);
				Vector2 candidateTopLeft = candidateCenter - new Vector2(NPC.width / 2f, NPC.height / 2f);

				if (IsAreaClear(candidateTopLeft, NPC.width, NPC.height))
				{
					return candidateCenter;
				}
			}

			return targetCenter - new Vector2(0f, 150f);
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

		// Same test AnimalMinionProjectile uses - scale-independent, no adjustment needed for the
		// doubled hitbox.
		private bool IsGrounded() => NPC.velocity.Y == 0f;

		// Edge-relative (NPC.width/height, which already reflect the 2x hitbox) rather than a flat
		// pixel offset, so this stays correct if the size ever changes again.
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

			// velocity.X should be near LungeSpeed while actively lunging - if it's been cancelled
			// down near zero, tile collision just blocked this tick's movement.
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

		private void DoSnakeSummon(Player target)
		{
			NPC.velocity *= 0.9f;

			// AI() increments StateTimer unconditionally every tick, including the tick DoRecover
			// resets it to 0 when transitioning into this state - by the time this method actually
			// runs for the first time in a fresh cycle, StateTimer is already 1, never 0 (same bug
			// found in PainBoss.DoAnimal - see there for the full trace).
			if (StateTimer == 1)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 baseDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
					float[] spreadAngles = { -0.35f, 0f, 0.35f };

					foreach (float angle in spreadAngles)
					{
						Vector2 shotVelocity = baseDirection.RotatedBy(angle) * 8f;
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shotVelocity, ModContent.ProjectileType<SnakeProjectile>(), 16, 1f);
					}
				}

				SnakesFired++;
			}

			if (StateTimer >= 30)
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
				CurrentAttack = Main.rand.Next(3) switch
				{
					0 => AttackState.Lunge,
					1 => AttackState.SnakeSummon,
					_ => AttackState.Jump,
				};
				StateTimer = 0f;
				SnakesFired = 0f;
			}
		}

		private void DoJump(Player target)
		{
			if (!jumpLaunched)
			{
				if (StateTimer < JumpTelegraphTicks)
				{
					NPC.velocity.X *= 0.8f;
					return;
				}

				float directionX = target.Center.X > NPC.Center.X ? 1f : -1f;
				NPC.velocity.X = directionX * JumpHorizontalSpeed;
				NPC.velocity.Y = JumpVerticalImpulse;
				jumpLaunched = true;
				SoundEngine.PlaySound(SoundID.Item29, NPC.Center);
				return;
			}

			if (StateTimer > JumpTelegraphTicks && IsGrounded())
			{
				OnJumpLanding();
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
				jumpLaunched = false;
			}
		}

		private void OnJumpLanding()
		{
			ChakraVFX.SpawnBurstEffect<CorruptionBurstProjectile>(NPC.Center, 2f);
			SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];

				if (!player.active || player.dead)
				{
					continue;
				}

				if (Vector2.Distance(player.Center, NPC.Center) <= JumpImpactRadius)
				{
					Vector2 push = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * JumpImpactKnockback;
					player.velocity += push;
				}
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
				AttackState.SnakeSummon => AnimBlock.Cast,
				AttackState.Jump => AnimBlock.Melee,
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
		// the frame size - once the hitbox stops matching the frame (see SizeMultiplier/PreDraw
		// above), that mismatch reads as the sprite floating away from its own hitbox. Anchoring to
		// the frame's own (measured) bottom padding and drawing at NPC.Bottom instead keeps the feet
		// planted on the hitbox's bottom edge regardless of scale. Same pattern as TailedBeastBoss.
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

		// Phase escalation = increasing sickly-purple aura intensity in post, per
		// ANIMATION_PIPELINE.md, rather than new geometry per phase.
		public override Color? GetAlpha(Color drawColor)
		{
			float intensity = CurrentPhase switch
			{
				Phase.Two => 0.2f,
				Phase.Three => 0.4f,
				_ => 0f,
			};

			return intensity <= 0f ? null : Color.Lerp(drawColor, new Color(140, 70, 170), intensity);
		}

		public override void OnKill()
		{
			StoryProgressSystem.DownedOrochimaru = true;
			StoryProgressSystem.SyncToClients();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<CursedSnakeFangItem>(), 1, 3, 5));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<CursedSealFragmentItem>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<ChakraScroll3Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<StaminaScroll3Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.BossBag(ModContent.ItemType<OrochimaruBossBagItem>()));
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			ChakraVFX.SpawnBurstEffect<CorruptionBurstProjectile>(NPC.Center, 0.5f);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				new FlavorTextBestiaryInfoElement("A rogue sannin obsessed with forbidden jutsu, always slipping away from death's grasp.")
			});
		}
	}
}
