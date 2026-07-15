using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Items.Accessories;
using NarutoOverhaul.Content.Items.Weapons.Jutsu;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.NPCs.Town
{
	// Taijutsu class specialist vendor. Available from world start like Tenten; the items he
	// sells only appear once Shukaku is down, matching what each item's own CanUseItem already
	// requires - the shop condition isn't a new gate, just making the shop honest about it.
	public class GuySenseiNPC : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 25;
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
			AnimationType = NPCID.Guide;
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

			new NPCShop(Type, "Shop")
				.Add(ModContent.ItemType<GentleFistItem>(), downedShukaku)
				.Add(ModContent.ItemType<LeafHurricaneItem>(), downedShukaku)
				.Add(ModContent.ItemType<IronLegItem>(), downedShukaku)
				.Add(ModContent.ItemType<WeightedLegWarmersItem>(), downedShukaku)
				.Register();
		}
	}
}
