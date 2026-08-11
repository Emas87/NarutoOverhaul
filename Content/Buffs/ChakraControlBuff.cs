using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Marker + timer via vanilla buff machinery, same as SageModeBuff - all real behavior
	// (water walking, wall climbing, chakra drain) lives on TransformationPlayer/ChakraControlForm.
	public class ChakraControlBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = false;
			// The 10-minute AddBuff duration in TransformationPlayer.ToggleForm is a long safety cap,
			// not the real duration (chakra/stamina drain is) - showing a countdown for it is misleading.
			Main.buffNoTimeDisplay[Type] = true;
		}
	}
}
