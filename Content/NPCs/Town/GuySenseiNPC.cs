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
	public class GuySenseiNPC : ModNPC
	{
		// Sheet layout from the nano-banana-generated GuySenseiNPC.png: idle(4)/walking(6). Custom
		// FindFrame() instead of AnimationType = NPCID.Guide since our sheet is a 2-row idle/walk
		// pair, not vanilla's 25-slot Guide frame layout.
		private const int IdleFrameStart = 0;
		private const int IdleFrameCount = 4;
		private const int IdleTicksPerStep = 8;

		private const int WalkFrameStart = IdleFrameStart + IdleFrameCount;
		private const int WalkFrameCount = 6;
		private const int WalkTicksPerStep = 6;

		private bool inWalkBlock;
		private int animFrame;
		private int animTicks;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = IdleFrameCount + WalkFrameCount;
		}

		public override void SetDefaults()
		{
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;
		}

		public override void FindFrame(int frameCounter)
		{
			int frameHeight = TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type];
			NPC.frame.Width = TextureAssets.Npc[NPC.type].Value.Width;
			NPC.frame.Height = frameHeight;

			bool walking = NPC.velocity.X != 0f;
			if (walking != inWalkBlock)
			{
				inWalkBlock = walking;
				animFrame = 0;
				animTicks = 0;
			}

			int frameStart = inWalkBlock ? WalkFrameStart : IdleFrameStart;
			int frameCount = inWalkBlock ? WalkFrameCount : IdleFrameCount;
			int ticksPerStep = inWalkBlock ? WalkTicksPerStep : IdleTicksPerStep;

			animTicks++;
			if (animTicks >= ticksPerStep)
			{
				animTicks = 0;
				animFrame = (animFrame + 1) % frameCount;
			}

			NPC.frame.Y = (frameStart + animFrame) * frameHeight;
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
