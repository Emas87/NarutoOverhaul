using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
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
		}

		public override void OnSpawn(IEntitySource source)
		{
			CurrentPhase = Phase.Base;
			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			SubCounter = 0f;
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
			ChakraVFX.SpawnBurst(NPC.Center, DustID.WhiteTorch, 24, 1.6f);
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
			ChakraVFX.SpawnBurst(NPC.Center, DustID.WhiteTorch, 10, 1.3f);

			Vector2 offset = Main.rand.NextVector2CircularEdge(TeleportRadius, TeleportRadius);
			NPC.Center = target.Center + offset;
			NPC.velocity = Vector2.Zero;

			ChakraVFX.SpawnBurst(NPC.Center, DustID.WhiteTorch, 10, 1.3f);
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
				Vector2 shotVelocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 9f;
				int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shotVelocity, ModContent.ProjectileType<ElementalBoltProjectile>(), 20, 1f);

				if (Main.projectile[index].ModProjectile is ElementalBoltProjectile bolt)
				{
					bolt.BoltElement = ElementalBoltProjectile.Element.Core;
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
					ChakraVFX.SpawnBurst(portalPositions[i], DustID.WhiteTorch, 6, 1.2f);
				}
			}

			if (StateTimer == PortalTelegraphTicks)
			{
				foreach (Vector2 portal in portalPositions)
				{
					ChakraVFX.SpawnBurst(portal, DustID.WhiteTorch, 14, 1.6f);
					SoundEngine.PlaySound(SoundID.Item14, portal);

					for (int i = 0; i < 4; i++)
					{
						Vector2 shotVelocity = new Vector2(0f, -1f).RotatedBy(MathHelper.PiOver2 * i) * 6f;
						Projectile.NewProjectile(NPC.GetSource_FromAI(), portal, shotVelocity, ModContent.ProjectileType<ElementalBoltProjectile>(), 18, 1f);
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

		public override void OnKill()
		{
			StoryProgressSystem.DownedKaguya = true;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			ChakraVFX.SpawnBurst(NPC.Center, DustID.WhiteTorch, 3, 1f, noGravity: false);
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
