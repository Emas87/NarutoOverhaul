using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Players;
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

namespace NarutoOverhaul.Content.NPCs.Bosses
{
	// Pain's Assault arc. Rather than literally spawning all Six Paths as separate simultaneous
	// NPCs (a much bigger "boss group" architecture nothing else here uses), this cycles through
	// all 6 Paths as HP-threshold phases on a single NPC - same pattern as Kakuzu's 5 elements,
	// but each Path gets a genuinely distinct mechanic rather than just a different projectile:
	// Deva repels the player, Asura rapid-fires, Human lifesteals on contact, Animal summons a
	// projectile spread, Preta drains the player's chakra directly, Naraka self-heals over time.
	// [AutoloadBossHead] registers PainBoss_Head_Boss.png as this boss's health-bar/minimap head icon.
	[AutoloadBossHead]
	public class PainBoss : ModNPC
	{
		private enum Path
		{
			Deva,
			Asura,
			Human,
			Animal,
			Preta,
			Naraka
		}

		private enum AttackState
		{
			Attack,
			Recover
		}

		private const int RecoverTicks = 45;
		private const float DevaRepelRadius = 220f;
		private const float PretaDrainRadius = 260f;
		public const float TownNpcDetectionRadius = 2500f;

		// Deva's actual signature ability - Bansho Ten'in (pull) into Shinra Tensei (push) plus a
		// temporary gravity flip - "attract us to him, repel us from him, and change gravity at his
		// will". The pull/gravity-flip specifically target the player (not whichever entity the
		// existing push targets - townTarget when protecting an NPC), matching "attract US to him".
		private const int DevaPullTicks = 20;
		private const float DevaPullRadius = 500f;
		private const float DevaPullSpeed = 9f;
		private const float DevaPullStrength = 0.12f;
		// Gravitation is the same buff vanilla's own Gravitation Potion grants - reusing it means all
		// of vanilla's existing safe-reversion/UI-icon handling for an inverted-gravity player comes
		// for free instead of hand-rolling Player.gravDir management (and its edge cases around
		// ceiling clearance, camera, etc.).
		private const int DevaGravityFlipDuration = 180;

		// DoHuman recomputes a full velocity toward the target from scratch every tick - when a
		// 1-tile obstacle blocks that path, vanilla's tile collision cancels velocity.X for that
		// tick, but the very next tick DoHuman immediately stomps it with the same blocked-direction
		// vector again before any step-climb correction can accumulate, so Pain just sits there
		// pressed against the block. TryHopOverObstacle is a small additive unstick valve on top of
		// vanilla's normal gravity/tile collision (which stays on), mirroring the fix already applied
		// to OrochimaruBoss/MadaraBoss (see OrochimaruBoss for the full writeup) and the wall-hop half
		// of the pattern proven for ground-walking minions in AnimalMinionProjectile.
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

		// While actively going after a Town NPC ("Pain levels the Hidden Leaf Village"), he reads as
		// almost invincible - a big defense spike, not literal invulnerability - so the player has a
		// clear, legible signal that he's mid-way through trying to kill that NPC specifically,
		// rather than it just being another indistinguishable phase of the fight.
		private const int BaseDefense = 24;
		private const int TownProtectDefenseBonus = 250;

		private bool isProtectingTownNpc;
		private bool wasProtectingTownNpc;

		// Pain read as too small next to the other bosses. NPC.scale only affects the drawn sprite
		// (Entity.Hitbox uses raw width/height, not scale - see OrochimaruBoss/KakuzuBoss for the
		// same distinction), so both are scaled together via SetDefaults+PreDraw below to actually
		// grow the hitbox and not just the visual.
		private const float SizeMultiplier = 2f;

		// PainBoss.png's alpha bounding box sits flush against a consistent 2px bottom margin in
		// every frame (measured directly, same technique as OrochimaruBoss/KakuzuBoss/TailedBeastBoss)
		// - used by PreDraw below to anchor the sprite's feet to the hitbox bottom instead of
		// vanilla's default hitbox-center anchor.
		private const float VisualBottomPaddingPx = 2f;

		// Sheet layout from the nano-banana-generated PainBoss.png: idle(4)/attack(6). Reused as-is
		// for all 6 Paths per ANIMATION_PIPELINE.md - they're differentiated by projectile VFX, not pose.
		private const int IdleFrameStart = 0;
		private const int IdleFrameCount = 4;
		private const int IdleTicksPerStep = 8;

		private const int AttackFrameStart = IdleFrameStart + IdleFrameCount;
		private const int AttackFrameCount = 6;
		private const int AttackTicksPerStep = 6;

		private bool inAttackBlock;
		private int animFrame;
		private int animTicks;

		private Path CurrentPath
		{
			get => (Path)NPC.ai[0];
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

		private float SubCounter
		{
			get => NPC.ai[3];
			set => NPC.ai[3] = value;
		}

		public override void SetDefaults()
		{
			// Native (unscaled) size - do NOT pre-multiply by SizeMultiplier here. Vanilla's own
			// NPC.SetDefaults(int) unconditionally does `width = (int)(width * scale); height =
			// (int)(height * scale);` right after this override returns, so pre-multiplying as well
			// would double-apply SizeMultiplier to the hitbox only. See OrochimaruBoss/KakuzuBoss for
			// the same rule.
			NPC.width = 44;
			NPC.height = 62;
			NPC.scale = SizeMultiplier;
			NPC.damage = 40;
			NPC.defense = BaseDefense;
			NPC.lifeMax = 13000;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.knockBackResist = 0.05f;
			NPC.boss = true;
			NPC.npcSlots = 10f;
			NPC.aiStyle = -1;
			NPC.value = Item.buyPrice(gold: 28);
			Main.npcFrameCount[NPC.type] = IdleFrameCount + AttackFrameCount;
		}

		public override void OnSpawn(IEntitySource source)
		{
			CurrentPath = Path.Deva;
			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			SubCounter = 0f;
		}

		// "Pain levels the Hidden Leaf Village" - prefers hunting the nearest Town NPC over the
		// player whenever one is in range, matching that beat instead of just being a player-only
		// fight. Falls back to the player once no Town NPC remains nearby.
		public static NPC FindNearestTownNPC(Vector2 position, float radius)
		{
			NPC closest = null;
			float closestDistance = radius;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];

				if (!npc.active || !npc.townNPC)
				{
					continue;
				}

				float distance = Vector2.Distance(npc.Center, position);

				if (distance <= closestDistance)
				{
					closest = npc;
					closestDistance = distance;
				}
			}

			return closest;
		}

		public static bool IsNearTownNPC(Vector2 position, float radius)
		{
			return FindNearestTownNPC(position, radius) != null;
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

			NPC townTarget = FindNearestTownNPC(NPC.Center, TownNpcDetectionRadius);
			Vector2 aimCenter = townTarget?.Center ?? target.Center;

			UpdatePathPhase();
			UpdateStuckState();

			// Only during Deva - his actual active assault-on-the-NPC phase - not just "some Town NPC
			// exists somewhere within TownNpcDetectionRadius (2500px)" for the whole fight, which
			// previously made him almost permanently armored any time the fight happens near town
			// (as it usually does), stalling the whole fight in the very first, non-shooting Deva
			// phase since players couldn't out-damage the bonus defense to ever progress past it.
			isProtectingTownNpc = townTarget != null && CurrentPath == Path.Deva;
			NPC.defense = isProtectingTownNpc ? BaseDefense + TownProtectDefenseBonus : BaseDefense;

			if (isProtectingTownNpc && !wasProtectingTownNpc)
			{
				ChakraVFX.SpawnGenjutsuBurst(NPC.Center, 3f);
				SoundEngine.PlaySound(SoundID.Item29, NPC.Center);
			}

			wasProtectingTownNpc = isProtectingTownNpc;

			NPC.spriteDirection = aimCenter.X < NPC.Center.X ? -1 : 1;

			if (hopCooldown > 0)
			{
				hopCooldown--;
			}

			switch (CurrentAttack)
			{
				case AttackState.Attack:
					DoAttack(target, townTarget, aimCenter);
					break;
				case AttackState.Recover:
					DoRecover();
					break;
			}

			// Only Human is a dash toward the target - the other 5 Paths never recompute a full
			// velocity toward it, so there's nothing to unstick there.
			if (CurrentAttack == AttackState.Attack && CurrentPath == Path.Human)
			{
				TryHopOverObstacle();
			}

			StateTimer++;
		}

		private void UpdatePathPhase()
		{
			float lifeRatio = (float)NPC.life / NPC.lifeMax;

			Path expected = lifeRatio switch
			{
				> 0.83f => Path.Deva,
				> 0.66f => Path.Asura,
				> 0.50f => Path.Human,
				> 0.33f => Path.Animal,
				> 0.16f => Path.Preta,
				_ => Path.Naraka,
			};

			if (expected != CurrentPath)
			{
				CurrentPath = expected;
				NPC.velocity = Vector2.Zero;
				SubCounter = 0f;
				ChakraVFX.SpawnGenjutsuBurst(NPC.Center, 2.5f);
				SoundEngine.PlaySound(SoundID.Item29, NPC.Center);
			}
		}

		private void DoAttack(Player target, NPC townTarget, Vector2 aimCenter)
		{
			switch (CurrentPath)
			{
				case Path.Deva:
					DoDeva(target, townTarget, aimCenter);
					break;
				case Path.Asura:
					DoAsura(aimCenter);
					break;
				case Path.Human:
					DoHuman(aimCenter);
					break;
				case Path.Animal:
					DoAnimal(aimCenter);
					break;
				case Path.Preta:
					DoPreta(target);
					break;
				case Path.Naraka:
					DoNaraka();
					break;
			}
		}

		// townTarget is null when there's no Town NPC in range - the repel then hits the player,
		// same as before this change.
		private void DoDeva(Player target, NPC townTarget, Vector2 aimCenter)
		{
			NPC.velocity *= 0.9f;

			// AI() increments StateTimer unconditionally every tick, so this state's first tick sees
			// StateTimer == 1, never 0 (see PainBoss.DoAnimal for the full trace) - telegraph cue on
			// the first tick rather than an unreachable StateTimer == 0.
			if (StateTimer == 1)
			{
				ChakraVFX.SpawnGenjutsuBurst(NPC.Center, 1.5f);
				SoundEngine.PlaySound(SoundID.Item29, NPC.Center);
			}

			// Bansho Ten'in - pulls the player (specifically, not whichever entity the repel below
			// targets) toward Pain for the first stretch of the attack.
			if (StateTimer < DevaPullTicks && NPC.Distance(target.Center) <= DevaPullRadius)
			{
				Vector2 pullDirection = (NPC.Center - target.Center).SafeNormalize(Vector2.Zero);
				target.velocity = Vector2.Lerp(target.velocity, pullDirection * DevaPullSpeed, DevaPullStrength);
			}

			if (StateTimer == 20 && NPC.Distance(aimCenter) <= DevaRepelRadius)
			{
				Vector2 pushDirection = (aimCenter - NPC.Center).SafeNormalize(Vector2.UnitX);
				Vector2 push = pushDirection * 14f;

				if (townTarget != null)
				{
					townTarget.velocity += push;
				}
				else
				{
					target.velocity += push;
				}

				// Shinra Tensei also flips the player's own gravity for a few seconds - reuses
				// vanilla's own Gravitation buff (the same one the Gravitation Potion grants) rather
				// than hand-rolling Player.gravDir management, so vanilla's existing safe-reversion
				// and buff-icon handling carries over for free.
				target.AddBuff(BuffID.Gravitation, DevaGravityFlipDuration);

				ChakraVFX.SpawnGenjutsuBurst(NPC.Center, 2.4f, pushDirection.ToRotation());
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
			}

			if (StateTimer >= 40)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoAsura(Vector2 aimCenter)
		{
			NPC.velocity *= 0.9f;

			if (StateTimer % 8 == 0 && SubCounter < 4)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 shotVelocity = (aimCenter - NPC.Center).SafeNormalize(Vector2.UnitY) * 9f;
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shotVelocity, ModContent.ProjectileType<PainOrbProjectile>(), 15, 1f, ai0: (float)ElementalBoltProjectile.Element.Core);
				}

				SubCounter++;
			}

			if (SubCounter >= 4)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoHuman(Vector2 aimCenter)
		{
			Vector2 toTarget = (aimCenter - NPC.Center).SafeNormalize(Vector2.UnitX);
			NPC.velocity = toTarget * 13f;

			if (StateTimer >= 24)
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

		private void DoAnimal(Vector2 aimCenter)
		{
			NPC.velocity *= 0.9f;

			// AI() increments StateTimer unconditionally every tick, including the tick DoRecover
			// resets it to 0 when transitioning into this state - by the time this method actually
			// runs for the first time in a fresh Attack cycle, StateTimer is already 1, never 0. This
			// silently skipped the whole projectile spread every cycle (animation played, nothing
			// spawned) until fixed.
			if (StateTimer == 1)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 baseDirection = (aimCenter - NPC.Center).SafeNormalize(Vector2.UnitY);
					float[] spreadAngles = { -0.4f, 0f, 0.4f };

					foreach (float angle in spreadAngles)
					{
						Vector2 shotVelocity = baseDirection.RotatedBy(angle) * 7f;
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shotVelocity, ModContent.ProjectileType<PainOrbProjectile>(), 14, 1f, ai0: (float)ElementalBoltProjectile.Element.Wind);
					}
				}
			}

			if (StateTimer >= 30)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoPreta(Player target)
		{
			NPC.velocity *= 0.9f;

			if (NPC.Distance(target.Center) <= PretaDrainRadius && StateTimer % 10 == 0)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					target.GetModPlayer<ChakraPlayer>().TrySpendChakra(8f);
					ChakraPlayer.SendCorrection(target); // server-only drain - tell the owning client
				}
				ChakraVFX.SpawnGenjutsuBurst(target.Center, 0.5f);
			}

			if (StateTimer >= 60)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoNaraka()
		{
			NPC.velocity *= 0.9f;

			if (StateTimer == 20)
			{
				NPC.life = System.Math.Min(NPC.lifeMax, NPC.life + (int)(NPC.lifeMax * 0.05f));
				ChakraVFX.SpawnHealingBurst(NPC.Center, 1.8f);
			}

			if (StateTimer >= 50)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoRecover()
		{
			NPC.velocity *= 0.9f;

			if (StateTimer >= RecoverTicks)
			{
				CurrentAttack = AttackState.Attack;
				StateTimer = 0f;
				SubCounter = 0f;
			}
		}

		public override void FindFrame(int frameCounter)
		{
			int frameHeight = TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type];
			NPC.frame.Width = TextureAssets.Npc[NPC.type].Value.Width;
			NPC.frame.Height = frameHeight;

			bool targetAttack = CurrentAttack == AttackState.Attack;
			if (targetAttack != inAttackBlock)
			{
				inAttackBlock = targetAttack;
				animFrame = 0;
				animTicks = 0;
			}

			int frameStart = inAttackBlock ? AttackFrameStart : IdleFrameStart;
			int frameCount = inAttackBlock ? AttackFrameCount : IdleFrameCount;
			int ticksPerStep = inAttackBlock ? AttackTicksPerStep : IdleTicksPerStep;

			animTicks++;
			if (animTicks >= ticksPerStep)
			{
				animTicks = 0;
				animFrame = (animFrame + 1) % frameCount;
			}

			NPC.frame.Y = (frameStart + animFrame) * frameHeight;
		}

		// Vanilla's default NPC draw anchors the sprite's origin at half the HITBOX size, not half
		// the frame size - once the hitbox stops matching the frame (see SizeMultiplier above), that
		// mismatch reads as the sprite floating away from its own hitbox. Anchoring to the frame's own
		// (measured) bottom padding and drawing at NPC.Bottom instead keeps the feet planted on the
		// hitbox's bottom edge regardless of scale. Same pattern as OrochimaruBoss/KakuzuBoss.
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

		// Sustained gold tint for the whole duration he's protecting a Town NPC - the defense spike
		// itself isn't otherwise visible to the player, so this is what actually communicates
		// "almost invincible right now" rather than just being a stat change under the hood.
		public override Color? GetAlpha(Color drawColor)
		{
			return isProtectingTownNpc ? Color.Lerp(drawColor, new Color(255, 230, 120), 0.55f) : null;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			if (CurrentPath == Path.Human)
			{
				NPC.life = System.Math.Min(NPC.lifeMax, NPC.life + hurtInfo.Damage / 2);
				ChakraVFX.SpawnHealingBurst(NPC.Center, 0.75f);
			}
		}

		public override void OnKill()
		{
			StoryProgressSystem.DownedPain = true;
			StoryProgressSystem.SyncToClients();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<RinneganFragmentItem>(), 1, 3, 5));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<ChakraScroll5Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<StaminaScroll5Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.BossBag(ModContent.ItemType<PainBossBagItem>()));
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			ChakraVFX.SpawnGenjutsuBurst(NPC.Center, 0.5f);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				new FlavorTextBestiaryInfoElement("A rinnegan-eyed shinobi wielding six paths of power, each body a different facet of the same pain.")
			});
		}
	}
}
