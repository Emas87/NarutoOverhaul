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
	// Team 7 reunion / early Shippuden arc boss. Cycles through 5 elemental mask phases at HP
	// thresholds (Fire -> Wind -> Lightning -> Earth -> Core), each firing a differently-colored/
	// -behaving ElementalBoltProjectile - a "one boss, five distinct attack identities" fight,
	// distinct from every previous boss's single attack theme.
	public class KakuzuBoss : ModNPC
	{
		private enum AttackState
		{
			RangedBurst,
			Recover
		}

		private const int BurstShots = 3;
		private const int RecoverTicks = 50;

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
			NPC.width = 46;
			NPC.height = 58;
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

			switch (CurrentAttack)
			{
				case AttackState.RangedBurst:
					DoRangedBurst(target);
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
				ChakraVFX.SpawnBurst(NPC.Center, DustID.Torch, 16, 1.3f);
				SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
			}
		}

		private void DoRangedBurst(Player target)
		{
			NPC.velocity *= 0.9f;
			NPC.spriteDirection = target.Center.X < NPC.Center.X ? -1 : 1;

			if (StateTimer % 15 == 0 && ShotsFired < BurstShots)
			{
				Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 7f;
				int type = ModContent.ProjectileType<ElementalBoltProjectile>();
				int index = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, toTarget, type, 15, 1f);

				if (Main.projectile[index].ModProjectile is ElementalBoltProjectile bolt)
				{
					bolt.BoltElement = CurrentElement;
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
			NPC.velocity *= 0.9f;

			Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
			NPC.velocity += toTarget * 0.1f;

			if (StateTimer >= RecoverTicksForElement)
			{
				CurrentAttack = AttackState.RangedBurst;
				StateTimer = 0f;
				ShotsFired = 0f;
			}
		}

		public override void OnKill()
		{
			StoryProgressSystem.DownedKakuzu = true;
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
