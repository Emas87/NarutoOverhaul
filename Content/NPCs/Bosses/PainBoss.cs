using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Players;
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
	// Pain's Assault arc. Rather than literally spawning all Six Paths as separate simultaneous
	// NPCs (a much bigger "boss group" architecture nothing else here uses), this cycles through
	// all 6 Paths as HP-threshold phases on a single NPC - same pattern as Kakuzu's 5 elements,
	// but each Path gets a genuinely distinct mechanic rather than just a different projectile:
	// Deva repels the player, Asura rapid-fires, Human lifesteals on contact, Animal summons a
	// projectile spread, Preta drains the player's chakra directly, Naraka self-heals over time.
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
			NPC.width = 44;
			NPC.height = 62;
			NPC.damage = 40;
			NPC.defense = 24;
			NPC.lifeMax = 13000;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.knockBackResist = 0.05f;
			NPC.boss = true;
			NPC.npcSlots = 10f;
			NPC.aiStyle = -1;
			NPC.value = Item.buyPrice(gold: 28);
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
			NPC.spriteDirection = aimCenter.X < NPC.Center.X ? -1 : 1;

			switch (CurrentAttack)
			{
				case AttackState.Attack:
					DoAttack(target, townTarget, aimCenter);
					break;
				case AttackState.Recover:
					DoRecover();
					break;
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
				ChakraVFX.SpawnBurst(NPC.Center, DustID.PurpleTorch, 18, 1.4f);
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

				ChakraVFX.SpawnDirectionalBurst(NPC.Center, pushDirection, DustID.PurpleTorch, 16, 6f);
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
				Vector2 shotVelocity = (aimCenter - NPC.Center).SafeNormalize(Vector2.UnitY) * 9f;
				int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shotVelocity, ModContent.ProjectileType<ElementalBoltProjectile>(), 15, 1f);

				if (Main.projectile[index].ModProjectile is ElementalBoltProjectile bolt)
				{
					bolt.BoltElement = ElementalBoltProjectile.Element.Core;
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

		private void DoAnimal(Vector2 aimCenter)
		{
			NPC.velocity *= 0.9f;

			if (StateTimer == 0)
			{
				Vector2 baseDirection = (aimCenter - NPC.Center).SafeNormalize(Vector2.UnitY);
				float[] spreadAngles = { -0.4f, 0f, 0.4f };

				foreach (float angle in spreadAngles)
				{
					Vector2 shotVelocity = baseDirection.RotatedBy(angle) * 7f;
					int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shotVelocity, ModContent.ProjectileType<ElementalBoltProjectile>(), 14, 1f);

					if (Main.projectile[index].ModProjectile is ElementalBoltProjectile bolt)
					{
						bolt.BoltElement = ElementalBoltProjectile.Element.Wind;
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
				target.GetModPlayer<ChakraPlayer>().TrySpendChakra(8f);
				ChakraVFX.SpawnBurst(target.Center, DustID.PurpleTorch, 3, 0.8f);
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
				ChakraVFX.SpawnBurst(NPC.Center, DustID.HealingPlus, 12, 1.2f);
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

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			if (CurrentPath == Path.Human)
			{
				NPC.life = System.Math.Min(NPC.lifeMax, NPC.life + hurtInfo.Damage / 2);
				ChakraVFX.SpawnBurst(NPC.Center, DustID.HealingPlus, 6, 1f);
			}
		}

		public override void OnKill()
		{
			StoryProgressSystem.DownedPain = true;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<RinneganFragmentItem>(), 1, 3, 5));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<ChakraScroll5Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<StaminaScroll5Item>(), 1, 1, 1));
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			ChakraVFX.SpawnBurst(NPC.Center, DustID.PurpleTorch, 3, 1f, noGravity: false);
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
