using System.IO;
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

		// Sheet layout from the nano-banana-generated MadaraBoss.png: idle(5)/melee(4)/cast(4).
		private const int IdleFrameStart = 0;
		private const int IdleFrameCount = 5;
		private const int IdleTicksPerStep = 8;

		private const int MeleeFrameStart = IdleFrameStart + IdleFrameCount;
		private const int MeleeFrameCount = 4;

		private const int CastFrameStart = MeleeFrameStart + MeleeFrameCount;
		private const int CastFrameCount = 4;
		private const int CastTicksPerStep = 6;

		private enum AnimBlock
		{
			Idle,
			Melee,
			Cast
		}

		private AnimBlock currentAnimBlock = AnimBlock.Idle;
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
			Main.npcFrameCount[NPC.type] = IdleFrameCount + MeleeFrameCount + CastFrameCount;
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

			ChakraVFX.SpawnGenjutsuBurst(NPC.Center, 2.5f);
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			ShotsFired = 0f;
		}

		// NPC.scale isn't part of the standard sync packet, and a player joining mid-fight after
		// the Susanoo transition already happened would never run the transition code path at all
		// (it only fires once, off the phase-change check) - without this they'd see a full-size
		// boss forever. Syncing it directly sidesteps both problems.
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(NPC.scale);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			NPC.scale = reader.ReadSingle();
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
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 shotVelocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 8f;
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shotVelocity, ModContent.ProjectileType<ElementalBoltProjectile>(), 18, 1f, ai0: (float)ElementalBoltProjectile.Element.Fire);
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

		public override void FindFrame(int frameCounter)
		{
			int frameHeight = TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type];
			NPC.frame.Width = TextureAssets.Npc[NPC.type].Value.Width;
			NPC.frame.Height = frameHeight;

			AnimBlock targetBlock = CurrentAttack switch
			{
				AttackState.Lunge => AnimBlock.Melee,
				AttackState.RangedBurst => AnimBlock.Cast,
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
				case AnimBlock.Melee:
					// A single lunge-and-recover beat, not a repeating cycle - play it once,
					// synced to how far through the lunge (LungeTicks) we are.
					float progress = MathHelper.Clamp(StateTimer / LungeTicks, 0f, 1f);
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

		// Susanoo transformation already reads via NPC.scale (SetDefaults/TransitionToSusanoo);
		// layering a purple aura tint on top gives it a bit more visual escalation without needing
		// the "separate model" ANIMATION_PIPELINE.md originally called for - reuses the same rig.
		public override Color? GetAlpha(Color drawColor)
		{
			if (CurrentPhase == Phase.Susanoo)
			{
				return Color.Lerp(drawColor, new Color(150, 60, 200), 0.4f);
			}

			return null;
		}

		public override void OnKill()
		{
			StoryProgressSystem.DownedMadara = true;
			StoryProgressSystem.SyncToClients();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<SusanooCoreItem>(), 1, 3, 5));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<ChakraScroll6Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ModContent.ItemType<StaminaScroll6Item>(), 1, 1, 1));
			npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.BossBag(ModContent.ItemType<MadaraBossBagItem>()));
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			ChakraVFX.SpawnGenjutsuBurst(NPC.Center, 0.5f);
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
