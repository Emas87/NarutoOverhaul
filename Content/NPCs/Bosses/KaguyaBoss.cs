using System.IO;
using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Items.Consumables;
using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.NPCs.Bosses
{
	// Endgame final boss. Escalates across 3 phases, remixing mechanics proven in earlier bosses
	// (Haku's teleport, Kakuzu/Madara's elemental bolts, Orochimaru/Pain's self-heal) plus one new
	// signature attack: telegraphed "dimension portals" that open around the target and burst
	// with projectiles after a delay, rather than firing directly at the player.
	public class KaguyaBoss : ModNPC
	{
		private enum Phase
		{
			Base,
			Escalate,
			Final
		}

		private enum AttackState
		{
			Teleport,
			ElementalBurst,
			DimensionPortals,
			Recover
		}

		private const int RecoverTicks = 40;
		private const float TeleportRadius = 260f;
		private const int PortalCount = 3;
		private const int PortalTelegraphTicks = 40;
		private const int PortalDetonateTicks = 60;

		// Sheet layout from the nano-banana-generated KaguyaBoss.png: idle(4)/cast(7)/telegraph(4)
		// stacked in that order. Teleport reuses idle per ANIMATION_PIPELINE.md.
		private const int IdleFrameStart = 0;
		private const int IdleFrameCount = 4;
		private const int IdleTicksPerStep = 8;

		private const int CastFrameStart = IdleFrameStart + IdleFrameCount;
		private const int CastFrameCount = 7;
		private const int CastTicksPerStep = 6;

		private const int TelegraphFrameStart = CastFrameStart + CastFrameCount;
		private const int TelegraphFrameCount = 4;
		private const int TelegraphTicksPerStep = 8;

		private enum AnimBlock
		{
			Idle,
			Cast,
			Telegraph
		}

		private AnimBlock currentAnimBlock = AnimBlock.Idle;
		private int animFrame;
		private int animTicks;

		private readonly Vector2[] portalPositions = new Vector2[PortalCount];

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

		private float SubCounter
		{
			get => NPC.ai[3];
			set => NPC.ai[3] = value;
		}

		private int RecoverTicksForPhase => CurrentPhase switch
		{
			Phase.Final => RecoverTicks / 3,
			Phase.Escalate => RecoverTicks / 2,
			_ => RecoverTicks,
		};

		public override void SetDefaults()
		{
			NPC.width = 46;
			NPC.height = 68;
			NPC.damage = 46;
			NPC.defense = 30;
			NPC.lifeMax = 20000;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.knockBackResist = 0.02f;
			NPC.boss = true;
			NPC.npcSlots = 12f;
			NPC.aiStyle = -1;
			NPC.value = Item.buyPrice(gold: 50);
			Main.npcFrameCount[NPC.type] = IdleFrameCount + CastFrameCount + TelegraphFrameCount;
		}

		public override void OnSpawn(IEntitySource source)
		{
			CurrentPhase = Phase.Base;
			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			SubCounter = 0f;

			// Soft vanilla-tier scaling, not a hard gate: Kaguya is meant to be fought before
			// Moon Lord (one tier earlier than Lunatic Cultist, for a bit of buffer) - not
			// literally gated on Moon Lord himself, which would mean "always harder" until the
			// game's basically over. Still fightable early, just noticeably tougher.
			if (ModContent.GetInstance<NarutoOverhaulConfig>().EnableSoftBossScaling && !NPC.downedGolemBoss)
			{
				NPC.lifeMax = (int)(NPC.lifeMax * 1.5f);
				NPC.life = NPC.lifeMax;
				NPC.damage = (int)(NPC.damage * 1.3f);
				NPC.defense = (int)(NPC.defense * 1.2f);
			}
		}

		// See OrochimaruBoss.SendExtraAI - lifeMax isn't part of the standard NPC sync packet, so
		// without this a scaled-up Kaguya shows a client-side HP bar above 100%.
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
			NPC.spriteDirection = target.Center.X < NPC.Center.X ? -1 : 1;

			switch (CurrentAttack)
			{
				case AttackState.Teleport:
					DoTeleport(target);
					break;
				case AttackState.ElementalBurst:
					DoElementalBurst(target);
					break;
				case AttackState.DimensionPortals:
					DoDimensionPortals();
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

			if (lifeRatio <= 0.3f && CurrentPhase != Phase.Final)
			{
				CurrentPhase = Phase.Final;
				OnPhaseTransition();
			}
			else if (lifeRatio <= 0.6f && CurrentPhase == Phase.Base)
			{
				CurrentPhase = Phase.Escalate;
				OnPhaseTransition();
			}
		}

		private void OnPhaseTransition()
		{
			NPC.velocity = Vector2.Zero;
			ChakraVFX.SpawnBoneBurst(NPC.Center, 2.5f);
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

			if (CurrentPhase == Phase.Final)
			{
				NPC.life = System.Math.Min(NPC.lifeMax, NPC.life + (int)(NPC.lifeMax * 0.1f));
			}

			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			SubCounter = 0f;
		}

		private void DoTeleport(Player target)
		{
			ChakraVFX.SpawnBoneBurst(NPC.Center, 1.6f);

			Vector2 offset = Main.rand.NextVector2CircularEdge(TeleportRadius, TeleportRadius);
			NPC.Center = target.Center + offset;
			NPC.velocity = Vector2.Zero;

			ChakraVFX.SpawnBoneBurst(NPC.Center, 1.6f);
			SoundEngine.PlaySound(SoundID.Item28, NPC.Center);

			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
		}

		private void DoElementalBurst(Player target)
		{
			NPC.velocity *= 0.9f;

			int shotsWanted = CurrentPhase == Phase.Final ? 6 : 4;

			if (StateTimer % 8 == 0 && SubCounter < shotsWanted)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 shotVelocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 9f;

					// Element passed via ai0 at spawn time (not set after via a post-spawn cast) so
					// the spawn packet itself carries the right value - clients never see a stale
					// default element like they would from a same-tick-but-after mutation.
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shotVelocity, ModContent.ProjectileType<ElementalBoltProjectile>(), 20, 1f, ai0: (float)ElementalBoltProjectile.Element.Core);
				}

				SubCounter++;
			}

			if (SubCounter >= shotsWanted)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoDimensionPortals()
		{
			NPC.velocity *= 0.9f;

			if (StateTimer == 0)
			{
				Player target = Main.player[NPC.target];

				for (int i = 0; i < PortalCount; i++)
				{
					portalPositions[i] = target.Center + Main.rand.NextVector2CircularEdge(180f, 180f);
					ChakraVFX.SpawnBoneBurst(portalPositions[i], 0.9f);
				}
			}

			if (StateTimer == PortalTelegraphTicks)
			{
				foreach (Vector2 portal in portalPositions)
				{
					ChakraVFX.SpawnBoneBurst(portal, 2.5f);
					SoundEngine.PlaySound(SoundID.Item14, portal);

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 4; i++)
						{
							Vector2 shotVelocity = new Vector2(0f, -1f).RotatedBy(MathHelper.PiOver2 * i) * 6f;
							Projectile.NewProjectile(NPC.GetSource_FromAI(), portal, shotVelocity, ModContent.ProjectileType<ElementalBoltProjectile>(), 18, 1f, ai0: (float)ElementalBoltProjectile.Element.Core);
						}
					}
				}
			}

			if (StateTimer >= PortalDetonateTicks)
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
				CurrentAttack = ChooseNextAttack();
				StateTimer = 0f;
				SubCounter = 0f;
			}
		}

		private AttackState ChooseNextAttack()
		{
			if (CurrentPhase == Phase.Base)
			{
				return Main.rand.NextBool() ? AttackState.Teleport : AttackState.ElementalBurst;
			}

			int roll = Main.rand.Next(3);
			return roll switch
			{
				0 => AttackState.Teleport,
				1 => AttackState.ElementalBurst,
				_ => AttackState.DimensionPortals,
			};
		}

		public override void FindFrame(int frameCounter)
		{
			int frameHeight = TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type];
			NPC.frame.Width = TextureAssets.Npc[NPC.type].Value.Width;
			NPC.frame.Height = frameHeight;

			AnimBlock targetBlock = CurrentAttack switch
			{
				AttackState.ElementalBurst => AnimBlock.Cast,
				AttackState.DimensionPortals => AnimBlock.Telegraph,
				_ => AnimBlock.Idle, // Teleport, Recover -> idle
			};

			if (targetBlock != currentAnimBlock)
			{
				currentAnimBlock = targetBlock;
				animFrame = 0;
				animTicks = 0;
			}

			int frameStart;
			int frameCount;
			int ticksPerStep;

			switch (currentAnimBlock)
			{
				case AnimBlock.Cast:
					frameStart = CastFrameStart;
					frameCount = CastFrameCount;
					ticksPerStep = CastTicksPerStep;
					break;
				case AnimBlock.Telegraph:
					frameStart = TelegraphFrameStart;
					frameCount = TelegraphFrameCount;
					ticksPerStep = TelegraphTicksPerStep;
					break;
				default:
					frameStart = IdleFrameStart;
					frameCount = IdleFrameCount;
					ticksPerStep = IdleTicksPerStep;
					break;
			}

			animTicks++;
			if (animTicks >= ticksPerStep)
			{
				animTicks = 0;
				animFrame = (animFrame + 1) % frameCount;
			}

			NPC.frame.Y = (frameStart + animFrame) * frameHeight;
		}

		// Phase escalation = increasing violet glow intensity in post, per ANIMATION_PIPELINE.md,
		// rather than new geometry per phase.
		public override Color? GetAlpha(Color drawColor)
		{
			float intensity = CurrentPhase switch
			{
				Phase.Escalate => 0.2f,
				Phase.Final => 0.4f,
				_ => 0f,
			};

			return intensity <= 0f ? null : Color.Lerp(drawColor, new Color(190, 130, 230), intensity);
		}

		public override void OnKill()
		{
			StoryProgressSystem.DownedKaguya = true;
			StoryProgressSystem.SyncToClients();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<OtsutsukiChakraFragmentItem>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<ChakraScroll7Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<StaminaScroll7Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.BossBag(ModContent.ItemType<KaguyaBossBagItem>()));
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			ChakraVFX.SpawnBoneBurst(NPC.Center, 0.5f);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				new FlavorTextBestiaryInfoElement("The progenitor of chakra itself, warping the battlefield between dimensions at will.")
			});
		}
	}
}
