using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Items.Accessories;
using NarutoOverhaul.Content.Items.Weapons.Jutsu;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.NPCs.Town
{
	// Genjutsu class specialist vendor. A friendly shop-keeper role is a deliberate canon liberty
	// (he's an antagonist for nearly the entire story) - confirmed with the user, same spirit as
	// the non-literal Madara/Graveyard biome gate from an earlier batch. Available from world
	// start like the other vendors; his stock unlocks once Haku is down, matching the Genjutsu
	// items' own existing CanUseItem gate.
	public class ItachiNPC : ModNPC
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
			return "Genjutsu is the art of showing the enemy a reality of your choosing.";
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
			var downedHaku = new Condition("NarutoOverhaul.DownedHaku", () => StoryProgressSystem.DownedHaku);

			new NPCShop(Type, "Shop")
				.Add(ModContent.ItemType<GenjutsuIllusionItem>(), downedHaku)
				.Add(ModContent.ItemType<GenjutsuSleepItem>(), downedHaku)
				.Add(ModContent.ItemType<GenjutsuNightmareItem>(), downedHaku)
				.Add(ModContent.ItemType<IllusionCharmItem>(), downedHaku)
				.Register();
		}
	}
}
