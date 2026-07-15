using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Short damage-boost window granted on a successful Sharingan dodge - the stat application
	// lives on SharinganPlayer, this is just the timer/marker.
	public class SharinganFocusBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = false;
		}
	}
}
