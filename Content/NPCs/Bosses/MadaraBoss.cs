using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using NarutoOverhaul.Content.Items.Materials;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.NPCs.Bosses
{
	// Fourth Shinobi War arc. Two-act fight: a normal humanoid phase, then a one-time transition
	// into a "Susanoo" avatar at 50% HP - represented via NPC.scale + a stat/attack upgrade rather
	// than a second sprite, so the giant-avatar feel doesn't depend on new art existing yet.
	public class MadaraBoss : ModNPC
	{
		private enum Phase
		{
			Base,
			Susanoo
		}

		private enum AttackState
		{
			Lunge,
			RangedBurst,
			Recover
		}

		private const int LungeTicks = 22;
		private const int RecoverTicks = 40;
		private const float SusanooScale = 1.8f;

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

		private float ShotsFired
		{
			get => NPC.ai[3];
			set => NPC.ai[3] = value;
		}

		private float LungeSpeed => CurrentPhase == Phase.Susanoo ? 18f : 13f;
		private int BurstShots => CurrentPhase == Phase.Susanoo ? 5 : 3;
		private int RecoverTicksForPhase => CurrentPhase == Phase.Susanoo ? RecoverTicks / 2 : RecoverTicks;

		public override void SetDefaults()
		{
			NPC.width = 48;
			NPC.height = 64;
			NPC.damage = 42;
			NPC.defense = 28;
			NPC.lifeMax = 15000;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.knockBackResist = 0.03f;
			NPC.boss = true;
			NPC.npcSlots = 10f;
			NPC.aiStyle = -1;
			NPC.value = Item.buyPrice(gold: 32);
		}

		public override void OnSpawn(IEntitySource source)
		{
			CurrentPhase = Phase.Base;
			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			ShotsFired = 0f;
			NPC.scale = 1f;
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

			if (CurrentPhase == Phase.Base && (float)NPC.life / NPC.lifeMax <= 0.5f)
			{
				TransitionToSusanoo();
			}

			NPC.spriteDirection = target.Center.X < NPC.Center.X ? -1 : 1;

			switch (CurrentAttack)
			{
				case AttackState.Lunge:
					DoLunge(target);
					break;
				case AttackState.RangedBurst:
					DoRangedBurst(target);
					break;
				case AttackState.Recover:
					DoRecover();
					break;
			}

			StateTimer++;
		}

		private void TransitionToSusanoo()
		{
			CurrentPhase = Phase.Susanoo;
			NPC.velocity = Vector2.Zero;
			NPC.scale = SusanooScale;

			// re-center hitbox growth so the boss doesn't visually jump position when it scales up
			NPC.position -= new Vector2(NPC.width * (SusanooScale - 1f) / 2f, NPC.height * (SusanooScale - 1f) / 2f);

			ChakraVFX.SpawnBurst(NPC.Center, DustID.PurpleTorch, 30, 1.8f);
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			ShotsFired = 0f;
		}

		private void DoLunge(Player target)
		{
			Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
			NPC.velocity = toTarget * LungeSpeed;

			if (StateTimer >= LungeTicks)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
			}
		}

		private void DoRangedBurst(Player target)
		{
			NPC.velocity *= 0.9f;

			if (StateTimer % 12 == 0 && ShotsFired < BurstShots)
			{
				Vector2 shotVelocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 8f;
				int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shotVelocity, ModContent.ProjectileType<ElementalBoltProjectile>(), 18, 1f);

				if (Main.projectile[index].ModProjectile is ElementalBoltProjectile bolt)
				{
					bolt.BoltElement = ElementalBoltProjectile.Element.Fire;
				}

				ShotsFired++;
			}

			if (ShotsFired >= BurstShots)
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
				CurrentAttack = Main.rand.NextBool() ? AttackState.Lunge : AttackState.RangedBurst;
				StateTimer = 0f;
				ShotsFired = 0f;
			}
		}

		public override void OnKill()
		{
			StoryProgressSystem.DownedMadara = true;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<SusanooCoreItem>(), 1, 3, 5));
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			ChakraVFX.SpawnBurst(NPC.Center, DustID.PurpleTorch, 3, 1f, noGravity: false);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				new FlavorTextBestiaryInfoElement("A reanimated legend who once manifested a titanic guardian of chakra around himself.")
			});
		}
	}
}
