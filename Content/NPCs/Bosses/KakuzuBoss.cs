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
	// Team 7 reunion / early Shippuden arc boss. Cycles through 5 elemental mask phases at HP
	// thresholds (Fire -> Wind -> Lightning -> Earth -> Core), each firing a differently-colored/
	// -behaving ElementalBoltProjectile - a "one boss, five distinct attack identities" fight,
	// distinct from every previous boss's single attack theme.
	// [AutoloadBossHead] registers KakuzuBoss_Head_Boss.png as this boss's health-bar/minimap head icon.
	[AutoloadBossHead]
	public class KakuzuBoss : ModNPC
	{
		private enum AttackState
		{
			RangedBurst,
			Jump,
			Recover
		}

		private const int BurstShots = 3;
		private const int RecoverTicks = 50;

		// No other state ever moves Kakuzu toward the player (RangedBurst only decays velocity while
		// casting, like every other boss's caster states) - unlike every other boss, which has some
		// discrete approach move (Lunge/DoHuman/Teleport), Kakuzu previously only had a token
		// `+= toTarget * 0.1f` per-tick nudge here in Recover. Against his knockBackResist of 0.05
		// (95% of hit knockback applies), a player fighting him in melee would knock him away faster
		// than that trickle could correct for, so over a real fight he nets away from the player -
		// read as "walks backward". Replaced with an actual Lerp-toward-target chase (same style as
		// TailedBeastBoss.DoChase), only touching velocity.X and leaving Y to gravity/collision as
		// normal, so Recover now closes distance for real instead of just resisting knockback.
		private const float RecoverChaseSpeed = 5f;

		// A telegraphed leap attack (EoC-style slam): plant for JumpTelegraphTicks, launch toward the
		// target with a single big velocity impulse, let vanilla's normal gravity/tile collision carry
		// the arc, then a landing burst + knockback flourish - actual damage comes from ordinary
		// contact during the leap/landing, same as every other movement-based attack in this codebase,
		// not a separate damage source. See OrochimaruBoss.DoJump for the same pattern.
		private const int JumpTelegraphTicks = 22;
		private const float JumpHorizontalSpeed = 8f;
		private const float JumpVerticalImpulse = -18f;
		private const float JumpImpactRadius = 110f;
		private const float JumpImpactKnockback = 8f;

		private bool jumpLaunched;

		// Universal "don't stay wedged/bouncing in place forever" safety net. "Stuck" covers two
		// cases: (1) the hitbox literally overlapping solid tiles (bad teleport/knockback), and
		// (2) repeatedly attempting to move (nonzero velocity - e.g. Jump landing at the same spot
		// against a wall it can't clear) without ever making real net progress. Being stationary
		// while genuinely idle (casting/recovering, near-zero velocity) is NOT stuck and must not
		// trigger this, or a boss calmly casting could suddenly sink through the floor for no reason.
		// Once either condition holds for StuckToleranceTicks (3s) with less than
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

		// Kakuzu read as too small next to the other bosses. NPC.scale only affects the drawn sprite
		// (Entity.Hitbox uses raw width/height, not scale - see OrochimaruBoss/TailedBeastBoss for the
		// same distinction), so both are scaled together via SetDefaults+PreDraw below to actually
		// grow the hitbox and not just the visual.
		private const float SizeMultiplier = 2f;

		// KakuzuBoss.png's alpha bounding box sits flush against a consistent 2px bottom margin in
		// every frame (measured directly, same technique as OrochimaruBoss/TailedBeastBoss) - used by
		// PreDraw below to anchor the sprite's feet to the hitbox bottom instead of vanilla's default
		// hitbox-center anchor.
		private const float VisualBottomPaddingPx = 2f;

		// Sheet layout from the nano-banana-generated KakuzuBoss.png: idle(4)/cast(7).
		private const int IdleFrameStart = 0;
		private const int IdleFrameCount = 4;
		private const int IdleTicksPerStep = 8;

		private const int CastFrameStart = IdleFrameStart + IdleFrameCount;
		private const int CastFrameCount = 7;
		private const int CastTicksPerStep = 6;

		private bool inCastBlock;
		private int animFrame;
		private int animTicks;

		private ElementalBoltProjectile.Element CurrentElement
		{
			get => (ElementalBoltProjectile.Element)NPC.ai[0];
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

		private int RecoverTicksForElement => CurrentElement == ElementalBoltProjectile.Element.Core ? RecoverTicks / 2 : RecoverTicks;

		public override void SetDefaults()
		{
			// Native (unscaled) size - do NOT pre-multiply by SizeMultiplier here. Vanilla's own
			// NPC.SetDefaults(int) unconditionally does `width = (int)(width * scale); height =
			// (int)(height * scale);` right after this override returns, so pre-multiplying as well
			// would double-apply SizeMultiplier to the hitbox only. See OrochimaruBoss/TailedBeastBoss
			// for the same rule.
			NPC.width = 46;
			NPC.height = 58;
			NPC.scale = SizeMultiplier;
			NPC.damage = 36;
			NPC.defense = 26;
			NPC.lifeMax = 11000;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.knockBackResist = 0.05f;
			NPC.boss = true;
			NPC.npcSlots = 9f;
			NPC.aiStyle = -1;
			NPC.value = Item.buyPrice(gold: 22);
			Main.npcFrameCount[NPC.type] = IdleFrameCount + CastFrameCount;
		}

		public override void OnSpawn(IEntitySource source)
		{
			CurrentElement = ElementalBoltProjectile.Element.Fire;
			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			ShotsFired = 0f;
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

			UpdateElementPhase();
			UpdateStuckState();

			// Set once per tick regardless of attack state (matching MadaraBoss/PainBoss/KaguyaBoss)
			// instead of only inside DoRangedBurst - previously spriteDirection went stale during
			// Recover while the body kept getting nudged toward the target on decayed velocity,
			// which could read as facing/drifting the wrong way (see OrochimaruBoss for the same fix
			// and full explanation).
			NPC.spriteDirection = target.Center.X < NPC.Center.X ? -1 : 1;

			switch (CurrentAttack)
			{
				case AttackState.RangedBurst:
					DoRangedBurst(target);
					break;
				case AttackState.Jump:
					DoJump(target);
					break;
				case AttackState.Recover:
					DoRecover(target);
					break;
			}

			StateTimer++;
		}

		private void UpdateElementPhase()
		{
			float lifeRatio = (float)NPC.life / NPC.lifeMax;

			ElementalBoltProjectile.Element expected = lifeRatio switch
			{
				> 0.8f => ElementalBoltProjectile.Element.Fire,
				> 0.6f => ElementalBoltProjectile.Element.Wind,
				> 0.4f => ElementalBoltProjectile.Element.Lightning,
				> 0.2f => ElementalBoltProjectile.Element.Earth,
				_ => ElementalBoltProjectile.Element.Core,
			};

			if (expected != CurrentElement)
			{
				CurrentElement = expected;
				NPC.velocity = Vector2.Zero;

				switch (expected)
				{
					case ElementalBoltProjectile.Element.Fire:
						ChakraVFX.SpawnBurstEffect<FireBurstProjectile>(NPC.Center, 2.5f);
						break;
					case ElementalBoltProjectile.Element.Wind:
						ChakraVFX.SpawnBurstEffect<WindBurstProjectile>(NPC.Center, 2.5f);
						break;
					case ElementalBoltProjectile.Element.Lightning:
						ChakraVFX.SpawnBurstEffect<LightningBurstProjectile>(NPC.Center, 2.5f);
						break;
					case ElementalBoltProjectile.Element.Earth:
						ChakraVFX.SpawnBurstEffect<EarthBurstProjectile>(NPC.Center, 2.5f);
						break;
					default:
						ChakraVFX.SpawnBurstEffect<CoreBurstProjectile>(NPC.Center, 2.5f);
						break;
				}

				SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
			}
		}

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

		private void DoRangedBurst(Player target)
		{
			NPC.velocity *= 0.9f;

			if (StateTimer % 15 == 0 && ShotsFired < BurstShots)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 7f;
					int type = ModContent.ProjectileType<ElementalBoltProjectile>();
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, toTarget, type, 15, 1f, ai0: (float)CurrentElement);
				}

				ShotsFired++;
			}

			if (ShotsFired >= BurstShots)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoRecover(Player target)
		{
			float directionX = target.Center.X > NPC.Center.X ? 1f : -1f;
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, directionX * RecoverChaseSpeed, 0.08f);

			if (StateTimer >= RecoverTicksForElement)
			{
				CurrentAttack = Main.rand.NextBool() ? AttackState.RangedBurst : AttackState.Jump;
				StateTimer = 0f;
				ShotsFired = 0f;
			}
		}

		private bool IsGrounded() => NPC.velocity.Y == 0f;

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
				SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
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

		// Matches the current elemental mask's burst, same as UpdateElementPhase's transition VFX -
		// keeps the "one rig, five identities" theme consistent for every move, not just the phase
		// transition.
		private void OnJumpLanding()
		{
			switch (CurrentElement)
			{
				case ElementalBoltProjectile.Element.Fire:
					ChakraVFX.SpawnBurstEffect<FireBurstProjectile>(NPC.Center, 2f);
					break;
				case ElementalBoltProjectile.Element.Wind:
					ChakraVFX.SpawnBurstEffect<WindBurstProjectile>(NPC.Center, 2f);
					break;
				case ElementalBoltProjectile.Element.Lightning:
					ChakraVFX.SpawnBurstEffect<LightningBurstProjectile>(NPC.Center, 2f);
					break;
				case ElementalBoltProjectile.Element.Earth:
					ChakraVFX.SpawnBurstEffect<EarthBurstProjectile>(NPC.Center, 2f);
					break;
				default:
					ChakraVFX.SpawnBurstEffect<CoreBurstProjectile>(NPC.Center, 2f);
					break;
			}

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

			bool targetCast = CurrentAttack == AttackState.RangedBurst || CurrentAttack == AttackState.Jump;
			if (targetCast != inCastBlock)
			{
				inCastBlock = targetCast;
				animFrame = 0;
				animTicks = 0;
			}

			int frameStart = inCastBlock ? CastFrameStart : IdleFrameStart;
			int frameCount = inCastBlock ? CastFrameCount : IdleFrameCount;
			int ticksPerStep = inCastBlock ? CastTicksPerStep : IdleTicksPerStep;

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
		// hitbox's bottom edge regardless of scale. Same pattern as OrochimaruBoss/TailedBeastBoss.
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

		// One rig, five elemental "masks" - differentiated purely by tint, per
		// ANIMATION_PIPELINE.md, matching the same element->dust colors ElementalBoltProjectile uses.
		public override Color? GetAlpha(Color drawColor)
		{
			Color tint = CurrentElement switch
			{
				ElementalBoltProjectile.Element.Fire => new Color(230, 140, 60),
				ElementalBoltProjectile.Element.Wind => new Color(170, 230, 170),
				ElementalBoltProjectile.Element.Lightning => new Color(130, 200, 255),
				ElementalBoltProjectile.Element.Earth => new Color(150, 110, 70),
				ElementalBoltProjectile.Element.Core => new Color(160, 90, 190),
				_ => Color.White,
			};

			return Color.Lerp(drawColor, tint, 0.35f);
		}

		public override void OnKill()
		{
			StoryProgressSystem.DownedKakuzu = true;
			StoryProgressSystem.SyncToClients();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<KakuzuHeartItem>(), 1, 3, 5));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<ChakraScroll4Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<StaminaScroll4Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.BossBag(ModContent.ItemType<KakuzuBossBagItem>()));
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			ChakraVFX.SpawnBurst(NPC.Center, DustID.Blood, 3, 1f, noGravity: false);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				new FlavorTextBestiaryInfoElement("A bounty-hunting missing-nin bound to five stolen hearts, each one a different elemental mask.")
			});
		}
	}
}
