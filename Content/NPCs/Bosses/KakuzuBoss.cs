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

				switch (expected)
				{
					case ElementalBoltProjectile.Element.Fire:
						ChakraVFX.SpawnFireBurst(NPC.Center, 2.5f);
						break;
					case ElementalBoltProjectile.Element.Wind:
						ChakraVFX.SpawnWindBurst(NPC.Center, 2.5f);
						break;
					case ElementalBoltProjectile.Element.Lightning:
						ChakraVFX.SpawnLightningBurst(NPC.Center, 2.5f);
						break;
					case ElementalBoltProjectile.Element.Earth:
						ChakraVFX.SpawnEarthBurst(NPC.Center, 2.5f);
						break;
					default:
						ChakraVFX.SpawnCoreBurst(NPC.Center, 2.5f);
						break;
				}

				SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
			}
		}

		private void DoRangedBurst(Player target)
		{
			NPC.velocity *= 0.9f;
			NPC.spriteDirection = target.Center.X < NPC.Center.X ? -1 : 1;

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

		public override void FindFrame(int frameCounter)
		{
			int frameHeight = TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type];
			NPC.frame.Width = TextureAssets.Npc[NPC.type].Value.Width;
			NPC.frame.Height = frameHeight;

			bool targetCast = CurrentAttack == AttackState.RangedBurst;
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
