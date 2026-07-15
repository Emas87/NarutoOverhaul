using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Marker/timer for "this NPC is puppeted by a genjutsu illusion." The actual movement
	// takeover lives in Common/GlobalNPCs/GenjutsuGlobalNPC.cs.
	public class GenjutsuControlDebuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true;
		}
	}
}
