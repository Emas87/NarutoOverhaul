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
			ChakraVFX.SpawnBurst(NPC.Center, DustID.IceTorch, 10, 1.3f);

			Vector2 offset = Main.rand.NextVector2CircularEdge(TeleportRadius, TeleportRadius);
			NPC.Center = target.Center + offset;
			NPC.velocity = Vector2.Zero;

			ChakraVFX.SpawnBurst(NPC.Center, DustID.IceTorch, 10, 1.3f);
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
			ChakraVFX.SpawnBurst(NPC.position, DustID.IceTorch, 3, 1f);
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
