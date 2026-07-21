using Microsoft.Xna.Framework;
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

		private Vector2 chargeDirection;

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

		public override void SetDefaults()
		{
			NPC.width = 100;
			NPC.height = 100;
			NPC.damage = 40;
			NPC.defense = 20;
			NPC.lifeMax = 12000;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0f;
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
				NPC.velocity.Y -= 0.2f;
				NPC.EncourageDespawn(10);
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
				case AttackState.Recover:
					DoRecover();
					break;
			}

			StateTimer++;
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

		private void DoChase(Player target)
		{
			Vector2 toTarget = target.Center - NPC.Center;
			NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget.SafeNormalize(Vector2.Zero) * ChaseSpeed, 0.05f);
			NPC.spriteDirection = target.Center.X < NPC.Center.X ? -1 : 1;

			if (StateTimer >= ChaseTicks)
			{
				chargeDirection = toTarget.SafeNormalize(Vector2.UnitX);
				CurrentAttack = AttackState.Charge;
				StateTimer = 0f;
			}
		}

		private void DoCharge()
		{
			NPC.velocity = chargeDirection * ChargeSpeed;

			if (StateTimer >= ChargeTicks)
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
