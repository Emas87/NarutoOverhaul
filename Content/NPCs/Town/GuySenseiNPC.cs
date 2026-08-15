using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Items.Accessories;
using NarutoOverhaul.Content.Items.Armor;
using NarutoOverhaul.Content.Items.Weapons.Jutsu;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.NPCs.Town
{
	// Taijutsu class specialist vendor. Available from world start like Tenten; the items he
	// sells only appear once Shukaku is down, matching what each item's own CanUseItem already
	// requires - the shop condition isn't a new gate, just making the shop honest about it.
	// [AutoloadHead] registers GuySenseiNPC_Head.png (already present on disk) as this NPC's
	// minimap/chat head icon - without it the head texture is never wired up even though the file
	// exists.
	[AutoloadHead]
	public class GuySenseiNPC : ModNPC
	{
		// Sheet layout from the nano-banana-generated GuySenseiNPC.png: idle(4)/walking(6). Custom
		// FindFrame() instead of AnimationType = NPCID.Guide since our sheet is a 2-row idle/walk
		// pair, not vanilla's 25-slot Guide frame layout.
		private const int IdleFrameStart = 0;
		private const int IdleFrameCount = 4;

		private const int WalkFrameStart = IdleFrameStart + IdleFrameCount;
		private const int WalkFrameCount = 6;
		private const int WalkTicksPerStep = 6;

		private const int WalkStateDebounceTicks = 8;

		private bool inWalkBlock;
		private int animFrame;
		private int animTicks;
		private int walkStateTicks;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = IdleFrameCount + WalkFrameCount;
		}

		// NPC.scale only affects the drawn sprite (Entity.Hitbox uses raw width/height, not scale),
		// so both are scaled together to actually grow the hitbox and not just the visual.
		private const float SizeMultiplier = 1.5f;

		public override void SetDefaults()
		{
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.width = (int)(18 * SizeMultiplier);
			NPC.height = (int)(40 * SizeMultiplier);
			NPC.scale = SizeMultiplier;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;
		}

		// Vanilla positions the talk bubble a fixed 26px above NPC.position.Y (the hitbox top), with
		// no awareness of sprite frame height or NPC.scale, so it floats well above our custom
		// sprite's head. Calibrated by pixel-measuring the bubble/head gap on KakashiNPC (same 56px
		// frame/hitbox setup) across two screenshots at different offsets (24px: bubble still ~12px
		// above head; 75px: bubble ~49px below/overlapping the body) and solving the resulting linear
		// fit for a ~0 gap - ~34px, not the 75px a rougher eyeball estimate suggested.
		private const float ChatBubbleYOffset = 34f;

		public override void ChatBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects)
		{
			position.Y += ChatBubbleYOffset;

			// Vanilla's left/right bubble placement assumes spriteDirection == -1 means facing left,
			// but PostAI (below) deliberately inverts that convention for our sheet's facing - so
			// vanilla puts the bubble behind the NPC instead of in front. Mirror the X position around
			// the NPC's screen-space center to put it back on the correct (facing) side, and flip the
			// icon's own SpriteEffects too - otherwise the position is corrected but the bubble's tail
			// still points the vanilla (wrong-for-us) way instead of back toward the NPC.
			float centerXScreen = NPC.Center.X - Main.screenPosition.X;
			float chatWidth = TextureAssets.Chat.Width();
			position.X = 2f * centerXScreen - position.X - chatWidth;
			spriteEffects ^= SpriteEffects.FlipHorizontally;
		}

		public override void FindFrame(int frameCounter)
		{
			int frameHeight = TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type];
			NPC.frame.Width = TextureAssets.Npc[NPC.type].Value.Width;
			NPC.frame.Height = frameHeight;

			// A raw threshold flip on every tick was too noisy: vanilla's passive town AI
			// accelerates/decelerates/turns around constantly, so velocity crosses 0.1f back and
			// forth many times per second. Flipping inWalkBlock on every crossing reset animFrame to
			// 0 each time, so the walk cycle never got past its first pose - looked like the NPC was
			// just snapping into "a walking position" instead of actually walking. Requiring the new
			// state to hold for WalkStateDebounceTicks in a row before switching blocks lets the walk
			// cycle actually play through.
			bool walking = System.Math.Abs(NPC.velocity.X) > 0.1f;
			if (walking != inWalkBlock)
			{
				walkStateTicks++;
				if (walkStateTicks >= WalkStateDebounceTicks)
				{
					inWalkBlock = walking;
					animFrame = 0;
					animTicks = 0;
					walkStateTicks = 0;
				}
			}
			else
			{
				walkStateTicks = 0;
			}

			// Vanilla town NPCs hold a single static pose while standing still and only animate
			// while walking - match that instead of continuously cycling the idle frames.
			if (inWalkBlock)
			{
				animTicks++;
				if (animTicks >= WalkTicksPerStep)
				{
					animTicks = 0;
					animFrame = (animFrame + 1) % WalkFrameCount;
				}
				NPC.frame.Y = (WalkFrameStart + animFrame) * frameHeight;
			}
			else
			{
				NPC.frame.Y = IdleFrameStart * frameHeight;
			}
		}

		// AI_007_TownEntities (vanilla's own town-NPC AI, used by aiStyle Passive) doesn't reliably
		// update NPC.direction/spriteDirection for these test-spawned NPCs (no valid house - see
		// StarterItemPlayer.ForceTownNpcSpawn), so facing stayed frozen. Force it straight from
		// velocity every tick instead of depending on vanilla's internal (house/pathing-dependent)
		// facing logic. NPC.direction keeps the normal 1=right/-1=left convention (other AI code
		// reads it), but the draw-facing flip (spriteDirection) is the opposite sign on this sheet -
		// confirmed empirically: the untouched default (spriteDirection = -1) was already showing
		// them facing right, so -1 = right/no-flip and 1 = left/flipped for this art.
		public override void PostAI()
		{
			if (NPC.velocity.X > 0.1f)
			{
				NPC.direction = 1;
				NPC.spriteDirection = -1;
			}
			else if (NPC.velocity.X < -0.1f)
			{
				NPC.direction = -1;
				NPC.spriteDirection = 1;
			}
		}

		public override string GetChat()
		{
			return "The flames of youth burn brightest in taijutsu! Come, let me equip you properly!";
		}

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = "Shop";
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shopName)
		{
			if (firstButton)
			{
				shopName = "Shop";
			}
		}

		public override void AddShops()
		{
			var downedShukaku = new Condition("NarutoOverhaul.DownedShukaku", () => StoryProgressSystem.DownedShukaku);
			var downedKakuzu = new Condition("NarutoOverhaul.DownedKakuzu", () => StoryProgressSystem.DownedKakuzu);

			new NPCShop(Type, "Shop")
				.Add(ModContent.ItemType<GentleFistItem>(), downedShukaku)
				.Add(ModContent.ItemType<LeafHurricaneItem>(), downedShukaku)
				.Add(ModContent.ItemType<IronLegItem>(), downedShukaku)
				.Add(ModContent.ItemType<WeightedLegWarmersItem>(), downedShukaku)
				.Add(ModContent.ItemType<TaijutsuWrapsItem>(), downedShukaku)
				.Add(ModContent.ItemType<FrontLotusItem>(), downedKakuzu)
				.Add(ModContent.ItemType<TaijutsuHelmetItem>(), downedKakuzu)
				.Add(ModContent.ItemType<TaijutsuBodyItem>(), downedKakuzu)
				.Add(ModContent.ItemType<TaijutsuLegsItem>(), downedKakuzu)
				.Register();
		}
	}
}
