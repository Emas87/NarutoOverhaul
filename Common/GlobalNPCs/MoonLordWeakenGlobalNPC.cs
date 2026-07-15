using NarutoOverhaul.Common.Players;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.GlobalNPCs
{
	// Kaguya's signature drop reaches into vanilla's own endgame: any character who has ever
	// consumed the Otsutsuki Chakra Fragment (a permanent, one-time blessing - see
	// MoonLordBlessingPlayer) faces a visibly weaker Moon Lord, framing her as stronger than he
	// is rather than gated behind him.
	public class MoonLordWeakenGlobalNPC : GlobalNPC
	{
		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			if (npc.type != NPCID.MoonLordHead && npc.type != NPCID.MoonLordHand && npc.type != NPCID.MoonLordCore)
			{
				return;
			}

			if (!AnyPlayerHasBlessing())
			{
				return;
			}

			npc.damage = (int)(npc.damage * 0.6f);
			npc.defense = (int)(npc.defense * 0.7f);
		}

		private static bool AnyPlayerHasBlessing()
		{
			for (int p = 0; p < Main.maxPlayers; p++)
			{
				Player player = Main.player[p];

				if (player.active && player.GetModPlayer<MoonLordBlessingPlayer>().HasKaguyaBlessing)
				{
					return true;
				}
			}

			return false;
		}
	}
}
