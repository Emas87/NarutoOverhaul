using Microsoft.Xna.Framework;
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
	// Ice Mirror mechanic: rather than a chase/thrust pattern (TailedBeastBoss), Haku teleports
	// between "mirror" positions around the target and alternates a melee lunge with senbon
	// volleys - reads as a distinct fight instead of a reskin of the existing boss.
	public class HakuBoss : ModNPC
	{
		private enum Phase
		{
			One,
			Two
		}

		private enum AttackState
		{
			Teleport,
			Strike,
			SenbonVolley,
			Recover
		}

		private const int StrikeTicks = 24;
		private const int RecoverTicks = 40;
		private const float TeleportRadius = 220f;

		// Sheet layout from the nano-banana-generated HakuBoss.png (idle/melee/cast rows, each a
		// real posed sequence rather than one hero frame repeated): 10 idle frames, then 7 melee
		// frames, then 8 cast frames, stacked in that order. Teleport reuses the idle block per
		// ANIMATION_PIPELINE.md ("Teleport = reuse Idle, add a fade/mist effect") rather than
		// getting its own pose.
		private const int IdleFrameStart = 0;
		private const int IdleFrameCount = 10;
		private const int IdleTicksPerStep = 8;

		private const int MeleeFrameStart = IdleFrameStart + IdleFrameCount;
		private const int MeleeFrameCount = 7;

		private const int CastFrameStart = MeleeFrameStart + MeleeFrameCount;
		private const int CastFrameCount = 8;
		private const int CastTicksPerStep = 6;

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

		private float VolleyShotsFired
		{
			get => NPC.ai[3];
			set => NPC.ai[3] = value;
		}

		private int VolleyShotCount => CurrentPhase == Phase.One ? 3 : 5;
		private float StrikeSpeed => CurrentPhase == Phase.One ? 14f : 20f;
		private int RecoverTicksForPhase => CurrentPhase == Phase.One ? RecoverTicks : RecoverTicks / 2;

		public override void SetDefaults()
		{
			NPC.width = 40;
			NPC.height = 56;
			NPC.damage = 32;
			NPC.defense = 14;
			NPC.lifeMax = 6000;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0.1f;
			NPC.boss = true;
			NPC.npcSlots = 8f;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.value = Item.buyPrice(gold: 12);
			Main.npcFrameCount[NPC.type] = IdleFrameCount + MeleeFrameCount + CastFrameCount;
		}

		public override void OnSpawn(IEntitySource source)
		{
			CurrentPhase = Phase.One;
			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			VolleyShotsFired = 0f;
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

			if (CurrentPhase == Phase.One && (float)NPC.life / NPC.lifeMax <= 0.5f)
			{
				CurrentPhase = Phase.Two;
			}

			switch (CurrentAttack)
			{
				case AttackState.Teleport:
					DoTeleport(target);
					break;
				case AttackState.Strike:
					DoStrike(target);
					break;
				case AttackState.SenbonVolley:
					DoSenbonVolley(target);
					break;
				case AttackState.Recover:
					DoRecover();
					break;
			}

			StateTimer++;
		}

		private void DoTeleport(Player target)
		{
			ChakraVFX.SpawnIceBurst(NPC.Center, 1.6f);

			Vector2 offset = Main.rand.NextVector2CircularEdge(TeleportRadius, TeleportRadius);
			NPC.Center = target.Center + offset;
			NPC.velocity = Vector2.Zero;

			ChakraVFX.SpawnIceBurst(NPC.Center, 1.6f);
			SoundEngine.PlaySound(SoundID.Item28, NPC.Center);

			CurrentAttack = Main.rand.NextBool() ? AttackState.Strike : AttackState.SenbonVolley;
			StateTimer = 0f;
			VolleyShotsFired = 0f;
		}

		private void DoStrike(Player target)
		{
			Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
			NPC.velocity = toTarget * StrikeSpeed;
			NPC.spriteDirection = target.Center.X < NPC.Center.X ? -1 : 1;

			if (StateTimer >= StrikeTicks)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoSenbonVolley(Player target)
		{
			NPC.velocity *= 0.9f;

			if (StateTimer % 10 == 0 && VolleyShotsFired < VolleyShotCount)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 9f;
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, toTarget, ModContent.ProjectileType<SenbonProjectile>(), 14, 1f);
				}

				VolleyShotsFired++;
			}

			if (VolleyShotsFired >= VolleyShotCount)
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
				CurrentAttack = AttackState.Teleport;
				StateTimer = 0f;
			}
		}

		private enum AnimBlock
		{
			Idle,
			Melee,
			Cast
		}

		private AnimBlock currentAnimBlock = AnimBlock.Idle;

		public override void FindFrame(int frameCounter)
		{
			int frameHeight = TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type];
			NPC.frame.Width = TextureAssets.Npc[NPC.type].Value.Width;
			NPC.frame.Height = frameHeight;

			AnimBlock targetBlock = CurrentAttack switch
			{
				AttackState.Strike => AnimBlock.Melee,
				AttackState.SenbonVolley => AnimBlock.Cast,
				_ => AnimBlock.Idle, // Teleport, Recover -> idle
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
					// The melee row is a single lunge-and-recover beat, not a repeating cycle -
					// play it once, synced to how far through the lunge (StrikeTicks) we are,
					// rather than looping it freely like idle/cast.
					float progress = MathHelper.Clamp(StateTimer / StrikeTicks, 0f, 1f);
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

		// Phase 2 = brighter icy-blue tint, per ANIMATION_PIPELINE.md's "vary palette/aura tint
		// instead of a new pose" guidance for Haku's HP phases.
		public override Color? GetAlpha(Color drawColor)
		{
			if (CurrentPhase == Phase.Two)
			{
				return Color.Lerp(drawColor, new Color(150, 220, 255), 0.35f);
			}

			return null;
		}

		public override void OnKill()
		{
			StoryProgressSystem.DownedHaku = true;
			StoryProgressSystem.SyncToClients();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<IceMirrorShardItem>(), 1, 3, 5));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<ChakraScroll1Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<StaminaScroll1Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.BossBag(ModContent.ItemType<HakuBossBagItem>()));
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			ChakraVFX.SpawnIceBurst(NPC.position, 0.5f);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				new FlavorTextBestiaryInfoElement("A shinobi who fights to protect her precious person, moving between mirrors of ice faster than the eye can follow.")
			});
		}
	}
}
