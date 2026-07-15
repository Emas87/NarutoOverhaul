using NarutoOverhaul.Content.Items.Accessories;
using NarutoOverhaul.Content.Items.Weapons;
using NarutoOverhaul.Content.Items.Weapons.Jutsu;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.NPCs.Town
{
	// "Tenten" - canonically associated with a ninja tool shop (Higurashi's Ninja Tool Shop) in
	// the source material. Available from world start, no unlock condition - standard vanilla
	// housing/spawn rules only, same as Guide. Reuses Guide's AI/animation entirely (AIType/
	// AnimationType) so this doesn't need a hand-built town-NPC state machine or walk-cycle timing,
	// just a same-shaped placeholder texture.
	public class ShinobiVendorNPC : ModNPC
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
			return "Need kunai or a summoning scroll? I've got what a shinobi needs.";
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
			NPCShop shop = new NPCShop(Type, "Shop")
				.Add(ModContent.ItemType<KunaiItem>())
				.Add(ModContent.ItemType<ToadSummonScrollItem>())
				.Add(ModContent.ItemType<NinjaHoundSummonScrollItem>())
				.Add(ModContent.ItemType<SnakeSummonScrollItem>())
				.Add(ModContent.ItemType<SlugSummonScrollItem>())
				.Add(ModContent.ItemType<MonstrousStrengthGlovesItem>());

			shop.Register();
		}
	}
}
