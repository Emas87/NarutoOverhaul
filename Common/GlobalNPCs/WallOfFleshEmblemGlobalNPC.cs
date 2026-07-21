using NarutoOverhaul.Content.Items.Accessories;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.GlobalNPCs
{
	// Wall of Flesh is untouched vanilla in this mod, so there's no ModNPC of our own to hook - a
	// GlobalNPC is the only way to add to its loot table. This is purely additive: it doesn't touch
	// vanilla's own Warrior/Ranger/Sorcerer/Summoner Emblem drop, just adds one more guaranteed
	// random pick from the mod's 3 shinobi-class emblems alongside it.
	public class WallOfFleshEmblemGlobalNPC : GlobalNPC
	{
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			if (npc.type != NPCID.WallofFlesh)
			{
				return;
			}

			npcLoot.Add(ItemDropRule.OneFromOptions(1,
				ModContent.ItemType<TaijutsuEmblemItem>(),
				ModContent.ItemType<NinjutsuEmblemItem>(),
				ModContent.ItemType<GenjutsuEmblemItem>()));
		}
	}
}
