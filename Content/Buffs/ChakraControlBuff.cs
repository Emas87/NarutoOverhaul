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
		}
	}
}
