using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Items.Accessories;

namespace NarutoOverhaul.Common.GlobalNPCs
{
	// Kaguya's signature drop reaches into vanilla's own endgame: wearing it into the Moon Lord
	// fight visibly softens him, framing her as stronger than he is rather than gated behind him.
	public class MoonLordWeakenGlobalNPC : GlobalNPC
	{
		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			if (npc.type != NPCID.MoonLordHead && npc.type != NPCID.MoonLordHand && npc.type != NPCID.MoonLordCore)
			{
				return;
			}

			if (!AnyPlayerHasFragmentEquipped())
			{
				return;
			}

			npc.damage = (int)(npc.damage * 0.6f);
			npc.defense = (int)(npc.defense * 0.7f);
		}

		private static bool AnyPlayerHasFragmentEquipped()
		{
			int fragmentType = ModContent.ItemType<OtsutsukiChakraFragmentItem>();

			for (int p = 0; p < Main.maxPlayers; p++)
			{
				Player player = Main.player[p];

				if (!player.active)
				{
					continue;
				}

				for (int i = 3; i < 10; i++)
				{
					if (player.armor[i].type == fragmentType)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
