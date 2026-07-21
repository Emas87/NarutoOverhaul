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
	// Ninjutsu class specialist vendor. Available from world start like Tenten; unlike Guy/Itachi,
	// his stock spans two different unlock tiers (Rasengan needs Orochimaru down; the rest need
	// Kakuzu down), so each shop entry uses its own matching condition instead of one shared one.
	public class KakashiNPC : ModNPC
	{
		// Sheet layout from the nano-banana-generated KakashiNPC.png: idle(6)/walking(7). Custom
		// FindFrame() instead of AnimationType = NPCID.Guide since our sheet is a 2-row idle/walk
		// pair, not vanilla's 25-slot Guide frame layout.
		private const int IdleFrameStart = 0;
		private const int IdleFrameCount = 6;
		private const int IdleTicksPerStep = 8;

		private const int WalkFrameStart = IdleFrameStart + IdleFrameCount;
		private const int WalkFrameCount = 7;
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
			return "I've copied over a thousand jutsu. Let's see which ones suit you.";
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
			var downedOrochimaru = new Condition("NarutoOverhaul.DownedOrochimaru", () => StoryProgressSystem.DownedOrochimaru);
			var downedKakuzu = new Condition("NarutoOverhaul.DownedKakuzu", () => StoryProgressSystem.DownedKakuzu);
			var downedPain = new Condition("NarutoOverhaul.DownedPain", () => StoryProgressSystem.DownedPain);
			var downedKaguya = new Condition("NarutoOverhaul.DownedKaguya", () => StoryProgressSystem.DownedKaguya);

			new NPCShop(Type, "Shop")
				.Add(ModContent.ItemType<ShadowCloneItem>())
				.Add(ModContent.ItemType<RasenganItem>(), downedOrochimaru)
				.Add(ModContent.ItemType<FireballItem>(), downedKakuzu)
				.Add(ModContent.ItemType<GreatBreakthroughItem>(), downedKakuzu)
				.Add(ModContent.ItemType<WaterDragonItem>(), downedKakuzu)
				.Add(ModContent.ItemType<ChakraPaperItem>(), downedKakuzu)
				.Add(ModContent.ItemType<NinjutsuFocusSealItem>(), downedKakuzu)
				.Add(ModContent.ItemType<ChidoriItem>(), downedPain)
				.Add(ModContent.ItemType<AllKillingAshBonesItem>(), downedKaguya)
				.Add(ModContent.ItemType<NinjutsuHelmetItem>(), downedKakuzu)
				.Add(ModContent.ItemType<NinjutsuBodyItem>(), downedKakuzu)
				.Add(ModContent.ItemType<NinjutsuLegsItem>(), downedKakuzu)
				.Register();
		}
	}
}
