using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
	// AI as an explicit state machine: NPC.ai[0] = phase, ai[1] = attack state, ai[2] = state timer.
	// HP-threshold phase transitions raise chase/charge speed - the "difficulty ramps per phase" pattern
	// used by overhaul-mod bosses instead of a flat, single-pattern fight.
	// [AutoloadBossHead] registers TailedBeastBoss_Head_Boss.png as this boss's health-bar/minimap
	// head icon.
	[AutoloadBossHead]
	public class TailedBeastBoss : ModNPC
	{
		private enum Phase
		{
			One,
			Two,
			Three
		}

		private enum AttackState
		{
			Chase,
			Charge,
			Jump,
			Recover
		}

		private const int ChaseTicks = 90;
		private const int ChargeTicks = 20;
		private const int RecoverTicks = 45;

		// Sheet layout from the nano-banana-generated TailedBeastBoss.png (hand-directed non-humanoid
		// poses per ANIMATION_PIPELINE.md, not the shared humanoid rig): idle/hover(8)/chase(8)/charge(7).
		private const int IdleFrameStart = 0;
		private const int IdleFrameCount = 8;
		private const int IdleTicksPerStep = 8;

		private const int ChaseFrameStart = IdleFrameStart + IdleFrameCount;
		private const int ChaseFrameCount = 8;
		private const int ChaseTicksPerStep = 6;

		private const int ChargeFrameStart = ChaseFrameStart + ChaseFrameCount;
		private const int ChargeFrameCount = 7;

		private enum AnimBlock
		{
			Idle,
			Chase,
			Charge
		}

		private AnimBlock currentAnimBlock = AnimBlock.Idle;
		private int animFrame;
		private int animTicks;

		private float chargeDirectionX;
		private float jumpDirectionX;
		private bool jumpLaunched;

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

		private float ChaseSpeed => CurrentPhase switch
		{
			Phase.One => 4f,
			Phase.Two => 6f,
			_ => 8f,
		};

		private float ChargeSpeed => CurrentPhase switch
		{
			Phase.One => 10f,
			Phase.Two => 14f,
			_ => 18f,
		};

		// A telegraphed leap attack (EoC-style slam): plant for JumpTelegraphTicks, launch toward the
		// target with a single big velocity impulse, then let ApplyGroundedMovement (called
		// unconditionally every tick, see AI()) carry the actual arc - its gravity-fall math already
		// generalizes to any starting velocity.Y (including a big negative "launch" one), not just
		// falling from rest, so no separate jump-physics path is needed here. Landing is detected the
		// same way ApplyGroundedMovement reports "resting" (velocity.Y snapped to exactly 0f), then a
		// landing burst + knockback flourish - actual damage comes from ordinary contact during the
		// leap/landing, same as every other movement-based attack here, not a separate damage source.
		// See OrochimaruBoss.DoJump for the same pattern applied to a vanilla-gravity boss.
		private const int JumpTelegraphTicks = 25;
		private const float JumpHorizontalSpeed = 9f;
		private const float JumpVerticalImpulse = -22f;
		private const float JumpImpactRadius = 160f;
		private const float JumpImpactKnockback = 10f;

		// Shukaku read as way too small at 1x - a full tailed beast should tower over the player.
		// NPC.scale only affects the drawn sprite (Entity.Hitbox uses raw width/height, not scale -
		// see MadaraBoss's Susanoo transition/HakuBoss's size doubling for the same distinction),
		// so both are scaled together to actually grow the hitbox and not just the visual.
		private const float SizeMultiplier = 4f;

		// Real native frame size from TailedBeastBoss.png (77x1288 = 23 frames of 56px), measured by
		// alpha bounding box - NOT 100x100.
		private const int NativeFrameWidth = 77;
		private const int NativeFrameHeight = 56;

		public override void SetDefaults()
		{
			// Set width/height to the NATIVE (unscaled) frame size here, NOT pre-multiplied by
			// SizeMultiplier. Vanilla's own NPC.SetDefaults(int) unconditionally does
			// `width = (int)(width * scale); height = (int)(height * scale);` right after this
			// override returns - confirmed by decompiling it. Pre-multiplying here as well double
			// applied SizeMultiplier to the hitbox only (77*4=308, then vanilla's auto-multiply made
			// it 308*4=1232) while the drawn sprite only ever gets scaled once by NPC.scale in
			// PreDraw - a hitbox 4x too large in each dimension versus the visible sprite, explaining
			// every "something invisible is hitting me" report this session (confirmed via
			// HitDebugLoggerPlayer's log: reported hitbox was exactly 1232x896, i.e. 16x the native
			// 77x56 frame instead of the intended 4x).
			NPC.width = NativeFrameWidth;
			NPC.height = NativeFrameHeight;
			NPC.scale = SizeMultiplier;
			NPC.damage = 40;
			NPC.defense = 20;
			NPC.lifeMax = 12000;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0f;
			// Shukaku is a ground creature, not a flier like HakuBoss, but vanilla's own
			// gravity/Collision.TileCollision (used when these are left off) is built for
			// few-tile-sized mobs - decompiling NPC.UpdateCollision confirmed it runs the same
			// generic resolver regardless of aiStyle, and that resolver just zeroes out further
			// vertical movement the moment any part of a hitbox this size (19x14 tiles) touches
			// ground, instead of tracking the surface height as it walks. That pinned Shukaku at
			// whatever Y it first landed on regardless of terrain, so as the player moved to a
			// different elevation its real (just wrongly-elevated) hitbox could still reach them
			// while visibly sitting somewhere else - "an invisible thing hitting me". Both flags stay
			// on and ApplyGroundedMovement (see AI()) does its own terrain-height tracking instead,
			// sized appropriately for this NPC instead of vanilla's small-mob-tuned collision.
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.boss = true;
			NPC.npcSlots = 10f;
			NPC.aiStyle = -1;
			NPC.value = Item.buyPrice(gold: 20);
			Main.npcFrameCount[NPC.type] = IdleFrameCount + ChaseFrameCount + ChargeFrameCount;
		}

		public override void OnSpawn(IEntitySource source)
		{
			CurrentPhase = Phase.One;
			CurrentAttack = AttackState.Chase;
			StateTimer = 0f;
		}

		public override void OnKill()
		{
			Common.Systems.StoryProgressSystem.DownedShukaku = true;
			Common.Systems.StoryProgressSystem.SyncToClients();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<SandCoreItem>(), 1, 3, 5));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<ChakraScroll2Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<StaminaScroll2Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.BossBag(ModContent.ItemType<ShukakuBossBagItem>()));
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
				// EncourageDespawn alone doesn't work here: vanilla's own per-tick despawn check
				// resets timeLeft back to full (and clears despawnEncouraged) every tick the NPC's
				// hitbox is still on ANY player's screen - including the player who just died, who
				// is usually still looking right at it. boss=true also exempts it from the normal
				// off-screen despawn entirely. Deactivating directly guarantees it actually leaves,
				// without granting kill credit/loot the way NPC.checkDead() would.
				NPC.active = false;
				NPC.netUpdate = true;
				return;
			}

			UpdatePhase();

			switch (CurrentAttack)
			{
				case AttackState.Chase:
					DoChase(target);
					break;
				case AttackState.Charge:
					DoCharge();
					break;
				case AttackState.Jump:
					DoJump();
					break;
				case AttackState.Recover:
					DoRecover();
					break;
			}

			ApplyGroundedMovement();

			StateTimer++;
		}

		private const float FallGravity = 0.5f;
		private const float MaxFallSpeed = 12f;
		// How far below the current top edge to search for the nearest solid tile, in tiles - generous
		// enough to cover falling from spawn height or off a cliff without scanning the whole world
		// every tick.
		private const int GroundScanRangeTiles = 120;

		// DoChase/DoCharge/DoRecover only ever touch velocity.X - this owns all of Y. Scans downward
		// from the NPC's own top edge (not vanilla's undersized-for-this-hitbox TileCollision) for the
		// nearest solid tile at its horizontal center and either falls toward it or snaps onto it,
		// re-run every tick so it re-tracks the surface height as it walks across hills/valleys instead
		// of getting stuck at whatever level it first landed on.
		private void ApplyGroundedMovement()
		{
			int centerTileX = (int)(NPC.Center.X / 16f);
			int topTileY = (int)(NPC.position.Y / 16f);
			int groundTileY = -1;

			for (int tileY = System.Math.Max(topTileY, 0); tileY < topTileY + GroundScanRangeTiles; tileY++)
			{
				if (WorldGen.SolidTile(centerTileX, tileY))
				{
					groundTileY = tileY;
					break;
				}
			}

			if (groundTileY < 0)
			{
				// No ground found within range (e.g. mid-fall off a cliff) - keep falling.
				NPC.velocity.Y = System.Math.Min(NPC.velocity.Y + FallGravity, MaxFallSpeed);
				NPC.position.Y += NPC.velocity.Y;
				return;
			}

			float groundSurfaceY = groundTileY * 16f;
			float desiredTopY = groundSurfaceY - NPC.height;

			if (NPC.position.Y >= desiredTopY - (NPC.velocity.Y + FallGravity))
			{
				// Already resting on it, or about to reach/pass it this tick - land exactly on it
				// instead of overshooting into the ground.
				NPC.position.Y = desiredTopY;
				NPC.velocity.Y = 0f;
			}
			else
			{
				NPC.velocity.Y = System.Math.Min(NPC.velocity.Y + FallGravity, MaxFallSpeed);
				NPC.position.Y += NPC.velocity.Y;
			}
		}

		private void UpdatePhase()
		{
			float lifeRatio = (float)NPC.life / NPC.lifeMax;

			if (lifeRatio <= 0.33f && CurrentPhase != Phase.Three)
			{
				CurrentPhase = Phase.Three;
				OnPhaseTransition();
			}
			else if (lifeRatio <= 0.66f && CurrentPhase == Phase.One)
			{
				CurrentPhase = Phase.Two;
				OnPhaseTransition();
			}
		}

		private void OnPhaseTransition()
		{
			NPC.velocity = Vector2.Zero;
			Common.VFX.ChakraVFX.SpawnChakraBurst(NPC.Center, 2.5f);
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
		}

		// Native (unflipped, spriteDirection=1) art faces RIGHT - confirmed by rendering the chase
		// frames directly (snout/eye on the right, legs stepping rightward, tail curling up-left
		// behind). A prior comment here claimed the opposite (native pose faces left) and the
		// resulting ternary had Shukaku flipped to face away from its own movement the whole time -
		// read as "walking backwards". SetFacing below is the single source of truth for this now.
		private void SetFacing(float velocityX)
		{
			if (velocityX != 0f)
			{
				NPC.spriteDirection = velocityX < 0 ? -1 : 1;
			}
		}

		private void DoChase(Player target)
		{
			// Ground creature, not a flier: only ever drive velocity.X here - velocity.Y is left
			// entirely to vanilla's own gravity/tile collision (noGravity/noTileCollide are off), so
			// Shukaku falls, lands, and walks on terrain like a normal NPC instead of hovering/flying.
			float directionX = target.Center.X > NPC.Center.X ? 1f : -1f;
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, directionX * ChaseSpeed, 0.05f);
			SetFacing(directionX);

			if (StateTimer >= ChaseTicks)
			{
				if (Main.rand.NextBool())
				{
					chargeDirectionX = directionX;
					CurrentAttack = AttackState.Charge;
				}
				else
				{
					jumpDirectionX = directionX;
					CurrentAttack = AttackState.Jump;
				}

				StateTimer = 0f;
			}
		}

		private void DoCharge()
		{
			NPC.velocity.X = chargeDirectionX * ChargeSpeed;
			SetFacing(NPC.velocity.X);

			if (StateTimer >= ChargeTicks)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoJump()
		{
			if (!jumpLaunched)
			{
				if (StateTimer < JumpTelegraphTicks)
				{
					NPC.velocity.X *= 0.8f;
					SetFacing(jumpDirectionX);
					return;
				}

				NPC.velocity.X = jumpDirectionX * JumpHorizontalSpeed;
				NPC.velocity.Y = JumpVerticalImpulse;
				jumpLaunched = true;
				SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
			}

			SetFacing(NPC.velocity.X);

			if (jumpLaunched && StateTimer > JumpTelegraphTicks && NPC.velocity.Y == 0f)
			{
				OnJumpLanding();
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
				jumpLaunched = false;
			}
		}

		private void OnJumpLanding()
		{
			Common.VFX.ChakraVFX.SpawnChakraBurst(NPC.Center, 2.5f);
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

		private void DoRecover()
		{
			NPC.velocity.X *= 0.9f;
			SetFacing(NPC.velocity.X);

			if (StateTimer >= RecoverTicks)
			{
				CurrentAttack = AttackState.Chase;
				StateTimer = 0f;
			}
		}

		public override void FindFrame(int frameCounter)
		{
			int frameHeight = TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type];
			NPC.frame.Width = TextureAssets.Npc[NPC.type].Value.Width;
			NPC.frame.Height = frameHeight;

			AnimBlock targetBlock = CurrentAttack switch
			{
				AttackState.Chase => AnimBlock.Chase,
				AttackState.Charge => AnimBlock.Charge,
				AttackState.Jump => AnimBlock.Charge,
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
				case AnimBlock.Charge:
					// The rear-back-and-roar sequence is a single telegraph-into-dash beat, not a
					// repeating cycle - play it once, synced to how far through the charge
					// (ChargeTicks) we are.
					float progress = MathHelper.Clamp(StateTimer / ChargeTicks, 0f, 1f);
					frameStart = ChargeFrameStart;
					frameIndex = (int)(progress * (ChargeFrameCount - 1));
					break;
				case AnimBlock.Chase:
					frameStart = ChaseFrameStart;
					animTicks++;
					if (animTicks >= ChaseTicksPerStep)
					{
						animTicks = 0;
						animFrame = (animFrame + 1) % ChaseFrameCount;
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
		// the frame/texture size (same bug pattern already fixed elsewhere in this codebase - see
		// SenbonProjectile/AnimalMinionProjectile). Even now that the hitbox is sized off the real
		// native frame (see NativeFrameWidth/Height above) instead of an invented one, the origin
		// still needs to come from the actual frame rectangle, not vanilla's hitbox-based default,
		// for the draw to land where the collision box actually is.
		//
		// A plain frame-center origin still isn't right though: measuring the sheet's alpha bounding
		// box per frame (same technique used for AnimalMinionProjectile) shows the character's feet
		// sit a consistent 2px above the frame's bottom edge in every frame, but the empty space
		// ABOVE the character varies per pose (0-4px - the charge/roar frames reach higher into the
		// frame). Anchoring at the geometric frame center against that uneven top padding reads as
		// the sprite floating a few pixels above the hitbox and bobbing between poses. Anchoring to
		// the frame's (fixed) bottom padding instead and drawing at NPC.Bottom keeps the feet planted
		// on the hitbox's bottom edge regardless of pose.
		private const float VisualBottomPaddingPx = 2f;

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

		// Phase escalation = increasing blue chakra-cloak intensity in post, matching the BlueTorch
		// dust already used on phase transition, per ANIMATION_PIPELINE.md's tint-not-new-geometry guidance.
		public override Color? GetAlpha(Color drawColor)
		{
			float intensity = CurrentPhase switch
			{
				Phase.Two => 0.15f,
				Phase.Three => 0.3f,
				_ => 0f,
			};

			return intensity <= 0f ? null : Color.Lerp(drawColor, new Color(120, 180, 240), intensity);
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			Common.VFX.ChakraVFX.SpawnBurst(NPC.Center, DustID.Blood, 3, 1f, noGravity: false);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				new FlavorTextBestiaryInfoElement("A tailed beast, sealed away long ago, now unleashed upon the world.")
			});
		}
	}
}
