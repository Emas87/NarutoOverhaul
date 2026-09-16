using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using Terraria.ModLoader.IO;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Content.NPCs.Bosses
{
	// Endgame final boss. Escalates across 3 phases, remixing mechanics proven in earlier bosses
	// (Haku's teleport, Kakuzu/Madara's elemental bolts, Orochimaru/Pain's self-heal) plus one new
	// signature attack: telegraphed "dimension portals" that open around the target and burst
	// with projectiles after a delay, rather than firing directly at the player.
	// [AutoloadBossHead] registers KaguyaBoss_Head_Boss.png as this boss's health-bar/minimap head icon.
	[AutoloadBossHead]
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
			DimensionShift,
			Recover
		}

		private const int RecoverTicks = 40;
		private const float TeleportRadius = 260f;
		private const int TeleportClearAttempts = 8;
		private const int PortalCount = 3;
		private const int PortalTelegraphTicks = 40;
		private const int PortalDetonateTicks = 60;

		// Universal "don't stay wedged/bouncing in place forever" safety net. "Stuck" covers two
		// cases: (1) the hitbox literally overlapping solid tiles (bad teleport/knockback), and
		// (2) repeatedly attempting to move (nonzero velocity - e.g. a Jump attack landing) without
		// ever making real net progress (e.g. bouncing between the same spot against a wall it can't
		// clear) - being stationary while genuinely idle (casting/recovering, near-zero velocity) is
		// NOT stuck and must not trigger this, or a boss calmly casting a spell could suddenly sink
		// through the floor for no reason. Once either condition holds for StuckToleranceTicks (3s)
		// with less than StuckMovementThreshold net displacement, nudge it to a nearby clear-air spot
		// and let gravity/collision (which stay ON throughout - never touching NPC.noTileCollide)
		// settle it onto whatever ground is below, same as any normal fall/landing. An earlier version
		// used NPC.noTileCollide to phase through terrain, but that disables collision in EVERY
		// direction including straight down, so a stuck boss fell clean through the floor instead of
		// escaping sideways/upward while still landing on solid ground - this reposition approach
		// can't do that since normal collision never turns off. Reuses the IsAreaClear/
		// FindClearTeleportCenter helpers already below (added for the safe-teleport fix).
		private const int StuckToleranceTicks = 180;
		private const float StuckMovementThreshold = 60f;
		private const float MovingVelocityThreshold = 1.5f;
		private const float EscapeSearchRadius = 150f;
		private int stuckTimer;
		private Vector2 stuckWindowStartPosition;

		// "Banish to another dimension" - her canon ability. Rather than a one-sided banishment
		// (player flung away, has to fight back), both Kaguya and the player relocate together in the
		// same instant, so it reads as the whole scene/dimension changing around them. Biome
		// locations are found via a one-time world tile scan (see EnsureBiomeColumnsScanned below),
		// covering Desert/Jungle/Snow/Corruption/Crimson/Hallow/Dungeon/Ocean.
		private const int LandingScanRangeTiles = 400;

		// Kaguya read as too small next to the other bosses (and being the endgame final boss, should
		// read as the biggest non-tailed-beast fight). NPC.scale only affects the drawn sprite
		// (Entity.Hitbox uses raw width/height, not scale - see OrochimaruBoss/KakuzuBoss for the same
		// distinction), so both are scaled together via SetDefaults+PreDraw below to actually grow the
		// hitbox and not just the visual. No runtime scale changes elsewhere (unlike MadaraBoss's
		// Susanoo transition), so this is a straight application of the same pattern.
		private const float SizeMultiplier = 3f;

		// KaguyaBoss.png's alpha bounding box sits flush against a consistent 2px bottom margin in
		// every frame (measured directly, same technique as OrochimaruBoss/KakuzuBoss/TailedBeastBoss)
		// - used by PreDraw below to anchor the sprite's feet to the hitbox bottom instead of
		// vanilla's default hitbox-center anchor.
		private const float VisualBottomPaddingPx = 2f;

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
			// Native (unscaled) size - do NOT pre-multiply by SizeMultiplier here. Vanilla's own
			// NPC.SetDefaults(int) unconditionally does `width = (int)(width * scale); height =
			// (int)(height * scale);` right after this override returns, so pre-multiplying as well
			// would double-apply SizeMultiplier to the hitbox only. See OrochimaruBoss/KakuzuBoss for
			// the same rule.
			NPC.width = 46;
			NPC.height = 68;
			NPC.scale = SizeMultiplier;
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
		//
		// portalPositions is rolled with Main.rand, which isn't synced across peers - without also
		// sending it here, each client would pick its own portal spots while only the server's copy
		// spawns the real hitboxes, so the telegraph would visually land in the wrong place.
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(NPC.lifeMax);

			for (int i = 0; i < PortalCount; i++)
			{
				writer.WriteVector2(portalPositions[i]);
			}
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			NPC.lifeMax = reader.ReadInt32();

			for (int i = 0; i < PortalCount; i++)
			{
				portalPositions[i] = reader.ReadVector2();
			}
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
			UpdateStuckState();
			ApplyFinalPhaseGravity(target);
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
				case AttackState.DimensionShift:
					DoDimensionShift(target);
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
			ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(NPC.Center, 2.5f);
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

			if (CurrentPhase == Phase.Final)
			{
				NPC.life = System.Math.Min(NPC.lifeMax, NPC.life + (int)(NPC.lifeMax * 0.1f));

				// One last, one-way dimension shift back to the origin (where the player first
				// appeared in the world) instead of another random biome - from here on,
				// ChooseNextAttack excludes DimensionShift entirely for the rest of the fight, so the
				// gravity hazard (ApplyFinalPhaseGravity) becomes the phase's signature mechanic in a
				// single fixed arena instead of the fight continuing to wander between biomes.
				ReturnToOrigin(Main.player[NPC.target]);

				// Second burst at the new (origin) position - the first one above marked departure
				// from wherever the fight had wandered to, this one marks arrival back at the origin.
				ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(NPC.Center, 4f);
				SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
			}

			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
			SubCounter = 0f;
		}

		// "The origin" = the world's spawn point, where the player first appeared in this world
		// (Main.spawnTileX/spawnTileY). Reuses FindSurfaceLandingSpot to land on solid ground exactly
		// there in case terrain has changed since spawn was set (e.g. the player dug it out), falling
		// back to the raw spawn tile position if nothing solid is found. Same "both relocate together"
		// pattern as DoDimensionShift.
		private void ReturnToOrigin(Player target)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			Vector2 originSpot = FindSurfaceLandingSpot(Main.spawnTileX, target.height)
				?? new Vector2(Main.spawnTileX * 16f + 8f, Main.spawnTileY * 16f - target.height / 2f);

			target.Teleport(originSpot);
			target.velocity = Vector2.Zero;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, target.whoAmI);
			}

			NPC.Center = FindClearTeleportCenter(originSpot, TeleportRadius);
			NPC.velocity = Vector2.Zero;
		}

		// "Messing with gravity" as a last-stand hazard - her control over gravity (already the
		// theme of the dimension shift) starts visibly breaking down near death, dragging both her
		// and the player down harder. The first version of this only added a small nudge on top of
		// EXISTING downward velocity - invisible almost all the time, since a grounded player's
		// velocity.Y gets reset to 0 by tile collision every single tick they're standing still, so
		// the nudge had nothing to build on for most of a normal fight. Replaced with a periodic,
		// unmissable "gravity pulse": every FinalPhaseGravityPulseTicks, forcibly SET (not add to)
		// both entities' downward velocity to a real jolt, regardless of whether they're grounded or
		// airborne, plus VFX/sound at both positions so it's felt AND seen/heard - a continuous
		// passive trickle can't compete with the second-to-second combat noise of a real fight.
		private const int FinalPhaseGravityPulseTicks = 50;
		private const float FinalPhaseGravityPulseVelocity = 11f;
		private const float FinalPhasePlayerPulseVelocity = 8f;

		private int finalPhaseGravityPulseTimer;

		private void ApplyFinalPhaseGravity(Player target)
		{
			if (CurrentPhase != Phase.Final)
			{
				finalPhaseGravityPulseTimer = 0;
				return;
			}

			finalPhaseGravityPulseTimer++;

			if (finalPhaseGravityPulseTimer < FinalPhaseGravityPulseTicks)
			{
				return;
			}

			finalPhaseGravityPulseTimer = 0;

			NPC.velocity.Y = FinalPhaseGravityPulseVelocity;
			target.velocity.Y = FinalPhasePlayerPulseVelocity;

			ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(NPC.Center, 1.5f);
			ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(target.Center, 1.5f);
			SoundEngine.PlaySound(SoundID.Item14, target.Center);
		}

		// The old code teleported straight to target.Center + a random offset with no check that the
		// landing spot was actually open - could land Kaguya's hitbox partway inside solid terrain,
		// where she'd stay wedged permanently. Retrying several random offsets and only committing to
		// one with a fully clear hitbox footprint fixes this at the source; falling back to straight
		// above the target if every attempt is blocked. Same pattern as
		// OrochimaruBoss.FindClearTeleportCenter.
		private bool IsAreaClear(Vector2 topLeft, int width, int height)
		{
			int tileX1 = (int)(topLeft.X / 16f);
			int tileY1 = (int)(topLeft.Y / 16f);
			int tileX2 = (int)((topLeft.X + width) / 16f);
			int tileY2 = (int)((topLeft.Y + height) / 16f);

			for (int x = tileX1; x <= tileX2; x++)
			{
				for (int y = tileY1; y <= tileY2; y++)
				{
					if (WorldGen.SolidTile(x, y))
					{
						return false;
					}
				}
			}

			return true;
		}

		private Vector2 FindClearTeleportCenter(Vector2 targetCenter, float radius)
		{
			for (int i = 0; i < TeleportClearAttempts; i++)
			{
				Vector2 candidateCenter = targetCenter + Main.rand.NextVector2CircularEdge(radius, radius);
				Vector2 candidateTopLeft = candidateCenter - new Vector2(NPC.width / 2f, NPC.height / 2f);

				if (IsAreaClear(candidateTopLeft, NPC.width, NPC.height))
				{
					return candidateCenter;
				}
			}

			return targetCenter - new Vector2(0f, 150f);
		}

		private void DoTeleport(Player target)
		{
			ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(NPC.Center, 1.6f);

			NPC.Center = FindClearTeleportCenter(target.Center, TeleportRadius);
			NPC.velocity = Vector2.Zero;

			ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(NPC.Center, 1.6f);
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

					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shotVelocity, ModContent.ProjectileType<KaguyaAshBoneProjectile>(), 20, 1f);
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

			// AI() increments StateTimer unconditionally every tick, including the tick DoRecover
			// resets it to 0 when transitioning into this state - by the time this method actually
			// runs for the first time in a fresh cycle, StateTimer is already 1, never 0 (same bug
			// found in PainBoss.DoAnimal/OrochimaruBoss.DoSnakeSummon - see PainBoss for the full
			// trace). This previously left portalPositions permanently at its default Vector2.Zero
			// for all 3 slots, so the telegraph tick below fired its projectiles at world origin
			// instead of near the player - invisible, not just "no projectiles".
			if (StateTimer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				Player target = Main.player[NPC.target];

				for (int i = 0; i < PortalCount; i++)
				{
					portalPositions[i] = target.Center + Main.rand.NextVector2CircularEdge(180f, 180f);
					ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(portalPositions[i], 0.9f);
				}

				NPC.netUpdate = true; // push the freshly rolled portalPositions through SendExtraAI
			}

			if (StateTimer == PortalTelegraphTicks)
			{
				foreach (Vector2 portal in portalPositions)
				{
					ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(portal, 2.5f);
					SoundEngine.PlaySound(SoundID.Item14, portal);

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						for (int i = 0; i < 4; i++)
						{
							Vector2 shotVelocity = new Vector2(0f, -1f).RotatedBy(MathHelper.PiOver2 * i) * 6f;
							Projectile.NewProjectile(NPC.GetSource_FromAI(), portal, shotVelocity, ModContent.ProjectileType<KaguyaAshBoneProjectile>(), 18, 1f);
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

			if (CurrentPhase == Phase.Final)
			{
				// DimensionShift is excluded from here on - she already made her one-way return to
				// the origin (see OnPhaseTransition/ReturnToOrigin), and the fight stays fixed there
				// for the rest of Final while the gravity hazard takes over as the signature mechanic.
				int finalRoll = Main.rand.Next(3);
				return finalRoll switch
				{
					0 => AttackState.Teleport,
					1 => AttackState.ElementalBurst,
					_ => AttackState.DimensionPortals,
				};
			}

			int roll = Main.rand.Next(4);
			return roll switch
			{
				0 => AttackState.Teleport,
				1 => AttackState.ElementalBurst,
				2 => AttackState.DimensionPortals,
				_ => AttackState.DimensionShift,
			};
		}

		private enum BiomeTarget
		{
			Desert,
			Jungle,
			Snow,
			Corruption,
			Crimson,
			Hallow,
			Dungeon,
			Ocean,
			Underworld
		}

		// Underworld doesn't need surface-tile classification like the biomes above - it's Hell,
		// present at every X column at the bottom of the map, not a localized region - so its
		// candidates are just spread-out X positions rather than scan results.
		private const int UnderworldCandidateCount = 10;

		// How far apart (in tiles) sampled columns are during the one-time world scan below - dense
		// enough to reliably find every biome present, sparse enough that the scan (run once, not
		// per-tick) stays cheap.
		private const int BiomeScanStepTiles = 10;

		private readonly Dictionary<BiomeTarget, List<int>> biomeColumns = new();
		private readonly List<BiomeTarget> biomeCycleQueue = new();
		private bool biomeColumnsScanned;

		// GenVars.jungleMinX/jungleMaxX/snowMinX/snowMaxX (the previous approach) are world-
		// GENERATION scratch state (Terraria.WorldBuilding.GenVars) - only populated inside
		// WorldGen.GenerateWorld's own generation pass, never persisted to the .wld file and never
		// repopulated on a normal world LOAD. Reading them during actual gameplay on a loaded world
		// hits their default (null for the arrays), which is exactly what crashed here - confirmed via
		// the client.log stack trace (NullReferenceException in this method, snowMinX being null).
		// Real tile scanning is the only safe way to locate biomes at runtime. Classifies each
		// sampled column's topmost solid tile against confirmed vanilla TileID constants (verified via
		// decompile) instead.
		private static BiomeTarget? ClassifyBiomeTile(ushort tileType)
		{
			return tileType switch
			{
				TileID.Sand or TileID.Sandstone or TileID.HardenedSand => BiomeTarget.Desert,
				TileID.JungleGrass or TileID.Mud => BiomeTarget.Jungle,
				TileID.SnowBlock or TileID.IceBlock => BiomeTarget.Snow,
				TileID.CorruptGrass or TileID.Ebonstone => BiomeTarget.Corruption,
				TileID.CrimsonGrass or TileID.Crimstone => BiomeTarget.Crimson,
				TileID.HallowedGrass or TileID.Pearlstone => BiomeTarget.Hallow,
				_ => null,
			};
		}

		private void AddBiomeColumn(BiomeTarget biome, int tileX)
		{
			if (!biomeColumns.TryGetValue(biome, out List<int> columns))
			{
				columns = new List<int>();
				biomeColumns[biome] = columns;
			}

			columns.Add(tileX);
		}

		// Run once (lazily, on the first DimensionShift trigger) rather than per-tick - a full-map
		// scan at BiomeScanStepTiles density is a one-time cost, not something to repeat every attack.
		private void EnsureBiomeColumnsScanned()
		{
			if (biomeColumnsScanned)
			{
				return;
			}

			biomeColumnsScanned = true;

			for (int tileX = 20; tileX < Main.maxTilesX - 20; tileX += BiomeScanStepTiles)
			{
				for (int tileY = 0; tileY < LandingScanRangeTiles; tileY++)
				{
					if (!WorldGen.SolidTile(tileX, tileY))
					{
						continue;
					}

					BiomeTarget? biome = ClassifyBiomeTile(Main.tile[tileX, tileY].TileType);

					if (biome.HasValue)
					{
						AddBiomeColumn(biome.Value, tileX);
					}

					break;
				}
			}

			// Dungeon/Ocean aren't identified by a surface tile type, so they don't come from the
			// scan above - added directly as their own candidates instead.
			if (Main.dungeonX > 0)
			{
				AddBiomeColumn(BiomeTarget.Dungeon, Main.dungeonX);
			}

			AddBiomeColumn(BiomeTarget.Ocean, WorldGen.beachDistance / 2);
			AddBiomeColumn(BiomeTarget.Ocean, Main.maxTilesX - WorldGen.beachDistance / 2);

			for (int i = 0; i < UnderworldCandidateCount; i++)
			{
				AddBiomeColumn(BiomeTarget.Underworld, Main.rand.Next(40, Main.maxTilesX - 40));
			}
		}

		// Cycles through every biome actually found in this world instead of rolling independently
		// each time (which could repeat the same biome many times before ever visiting another) -
		// pops one per trigger, only reshuffling/refilling once the queue empties, guaranteeing every
		// discovered biome is visited once before any repeat.
		private bool TryPickBiomeX(out int tileX, out BiomeTarget biome)
		{
			EnsureBiomeColumnsScanned();

			if (biomeCycleQueue.Count == 0)
			{
				biomeCycleQueue.AddRange(biomeColumns.Keys);

				for (int i = biomeCycleQueue.Count - 1; i > 0; i--)
				{
					int j = Main.rand.Next(i + 1);
					(biomeCycleQueue[i], biomeCycleQueue[j]) = (biomeCycleQueue[j], biomeCycleQueue[i]);
				}
			}

			while (biomeCycleQueue.Count > 0)
			{
				BiomeTarget candidate = biomeCycleQueue[^1];
				biomeCycleQueue.RemoveAt(biomeCycleQueue.Count - 1);

				if (biomeColumns.TryGetValue(candidate, out List<int> columns) && columns.Count > 0)
				{
					tileX = columns[Main.rand.Next(columns.Count)];
					biome = candidate;
					return true;
				}
			}

			tileX = 0;
			biome = default;
			return false;
		}

		// Scans downward from the world surface at tileX for the first solid tile (same technique as
		// TailedBeastBoss.ApplyGroundedMovement, rewritten against a plain column instead of an NPC's
		// own transform) and returns a safe center position for an entity of the given height resting
		// on it - or null if nothing solid is found in range, or the spot directly above it is lava.
		private Vector2? FindSurfaceLandingSpot(int tileX, int entityHeight)
		{
			tileX = System.Math.Clamp(tileX, 10, Main.maxTilesX - 10);

			for (int tileY = 0; tileY < LandingScanRangeTiles; tileY++)
			{
				if (!WorldGen.SolidTile(tileX, tileY))
				{
					continue;
				}

				Tile aboveTile = Main.tile[tileX, tileY - 1];

				if (aboveTile.LiquidAmount > 0 && aboveTile.LiquidType == LiquidID.Lava)
				{
					return null;
				}

				float groundSurfaceY = tileY * 16f;
				return new Vector2(tileX * 16f + 8f, groundSurfaceY - entityHeight / 2f);
			}

			return null;
		}

		// The Underworld is almost entirely lava, so unlike FindSurfaceLandingSpot this doesn't
		// abort outright on finding a lava-topped solid tile - it keeps scanning further down for
		// the next one instead, since giving up on the first lava pocket would fail most attempts.
		// Scans within the Underworld's own Y range (Main.UnderworldLayer downward) rather than from
		// the world top, since that would just find the normal overworld surface first.
		private const int UnderworldScanRangeTiles = 200;

		private Vector2? FindUnderworldLandingSpot(int tileX, int entityHeight)
		{
			tileX = System.Math.Clamp(tileX, 10, Main.maxTilesX - 10);

			int startY = (int)Main.UnderworldLayer;
			int endY = System.Math.Min(Main.maxTilesY - 10, startY + UnderworldScanRangeTiles);

			for (int tileY = startY; tileY < endY; tileY++)
			{
				if (!WorldGen.SolidTile(tileX, tileY))
				{
					continue;
				}

				Tile aboveTile = Main.tile[tileX, tileY - 1];

				if (aboveTile.LiquidAmount > 0 && aboveTile.LiquidType == LiquidID.Lava)
				{
					continue;
				}

				float groundSurfaceY = tileY * 16f;
				return new Vector2(tileX * 16f + 8f, groundSurfaceY - entityHeight / 2f);
			}

			return null;
		}

		// "Banish to another dimension" - both Kaguya and the player relocate together in the same
		// instant (Kaguya via the existing FindClearTeleportCenter, landing near the player's new
		// spot) rather than the player being flung away alone, so it reads as the whole scene
		// changing around them instead of a one-sided banishment.
		private void DoDimensionShift(Player target)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				CurrentAttack = AttackState.Recover;
				StateTimer = 0f;
				return;
			}

			Vector2? landingSpot = null;

			if (TryPickBiomeX(out int tileX, out BiomeTarget biome))
			{
				landingSpot = biome == BiomeTarget.Underworld
					? FindUnderworldLandingSpot(tileX, target.height)
					: FindSurfaceLandingSpot(tileX, target.height);
			}

			if (landingSpot.HasValue)
			{
				ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(NPC.Center, 2.5f);
				SoundEngine.PlaySound(SoundID.Item28, NPC.Center);

				target.Teleport(landingSpot.Value);
				target.velocity = Vector2.Zero;

				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, target.whoAmI);
				}

				NPC.Center = FindClearTeleportCenter(landingSpot.Value, TeleportRadius);
				NPC.velocity = Vector2.Zero;

				ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(NPC.Center, 2.5f);
				SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
			}

			CurrentAttack = AttackState.Recover;
			StateTimer = 0f;
		}

		private void UpdateStuckState()
		{
			bool embedded = !IsAreaClear(NPC.position, NPC.width, NPC.height);
			bool attemptingMovement = NPC.velocity.LengthSquared() > MovingVelocityThreshold * MovingVelocityThreshold;

			if (!embedded && !attemptingMovement)
			{
				stuckTimer = 0;
				return;
			}

			if (stuckTimer == 0)
			{
				stuckWindowStartPosition = NPC.Center;
			}

			stuckTimer++;

			if (Vector2.Distance(NPC.Center, stuckWindowStartPosition) >= StuckMovementThreshold)
			{
				stuckTimer = 0;
				return;
			}

			if (embedded || stuckTimer >= StuckToleranceTicks)
			{
				NPC.Center = FindClearTeleportCenter(NPC.Center, EscapeSearchRadius);
				NPC.velocity = Vector2.Zero;
				stuckTimer = 0;
			}
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
				AttackState.DimensionShift => AnimBlock.Telegraph,
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

		// Vanilla's default NPC draw anchors the sprite's origin at half the HITBOX size, not half
		// the frame size - once the hitbox stops matching the frame (see SizeMultiplier above), that
		// mismatch reads as the sprite floating away from its own hitbox. Anchoring to the frame's own
		// (measured) bottom padding and drawing at NPC.Bottom instead keeps the feet planted on the
		// hitbox's bottom edge regardless of scale. Same pattern as OrochimaruBoss/KakuzuBoss.
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = TextureAssets.Npc[NPC.type].Value;
			Rectangle frame = NPC.frame;
			Vector2 origin = new(frame.Width / 2f, frame.Height - VisualBottomPaddingPx);
			SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Vector2 drawPosition = NPC.Bottom - screenPos + new Vector2(0f, NPC.gfxOffY);

			spriteBatch.Draw(texture, drawPosition, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, effects, 0f);

			return false;
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
			ChakraVFX.SpawnBurstEffect<BoneBurstProjectile>(NPC.Center, 0.5f);
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
