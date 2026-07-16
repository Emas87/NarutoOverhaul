using System.IO;
using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using NarutoOverhaul.Content.Items.Consumables;
using NarutoOverhaul.Content.Items.Materials;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.NPCs.Bosses
{
	// Konoha Crush / Sasuke Retrieval arc final boss. Distinct from Haku (teleport+senbon) and
	// TailedBeastBoss (chase+charge): alternates a sword lunge with 3-way snake-projectile spreads,
	// and "substitutes" (teleports + partially heals, Orochimaru's resilience) once per phase
	// transition instead of just getting faster like the other bosses.
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
			Recover
		}

		private const int LungeTicks = 20;
		private const int RecoverTicks = 45;
		private const float SubstitutionHealFraction = 0.08f;

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
			NPC.width = 44;
			NPC.height = 60;
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

			switch (CurrentAttack)
			{
				case AttackState.Lunge:
					DoLunge(target);
					break;
				case AttackState.SnakeSummon:
					DoSnakeSummon(target);
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
				DoSubstitution(Main.player[NPC.target]);
			}
			else if (lifeRatio <= 0.66f && CurrentPhase == Phase.One)
			{
				CurrentPhase = Phase.Two;
				DoSubstitution(Main.player[NPC.target]);
			}
		}

		private void DoSubstitution(Player target)
		{
			ChakraVFX.SpawnBurst(NPC.Center, DustID.Corruption, 18, 1.4f);
			SoundEngine.PlaySound(SoundID.Item29, NPC.Center);

			Vector2 offset = Main.rand.NextVector2CircularEdge(260f, 260f);
			NPC.Center = target.Center + offset;
			NPC.velocity = Vector2.Zero;
			NPC.life = System.Math.Min(NPC.lifeMax, NPC.life + (int)(NPC.lifeMax * SubstitutionHealFraction));

			ChakraVFX.SpawnBurst(NPC.Center, DustID.Corruption, 18, 1.4f);

			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			SnakesFired = 0f;
		}

		private void DoLunge(Player target)
		{
			Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
			NPC.velocity = toTarget * LungeSpeed;
			NPC.spriteDirection = target.Center.X < NPC.Center.X ? -1 : 1;

			if (StateTimer >= LungeTicks)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoSnakeSummon(Player target)
		{
			NPC.velocity *= 0.9f;

			if (StateTimer == 0)
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
				CurrentAttack = Main.rand.NextBool() ? AttackState.Lunge : AttackState.SnakeSummon;
				StateTimer = 0f;
				SnakesFired = 0f;
			}
		}

		public override void OnKill()
		{
			StoryProgressSystem.DownedOrochimaru = true;
			StoryProgressSystem.SyncToClients();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<CursedSnakeFangItem>(), 1, 3, 5));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<ChakraScroll3Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<StaminaScroll3Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.BossBag(ModContent.ItemType<OrochimaruBossBagItem>()));
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			ChakraVFX.SpawnBurst(NPC.Center, DustID.Corruption, 3, 1f, noGravity: false);
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
