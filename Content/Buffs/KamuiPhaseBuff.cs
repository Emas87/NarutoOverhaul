using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Marker + timer via vanilla buff machinery, same as ChakraControlBuff - all real behavior
	// (tile-phasing) lives on TransformationPlayer/KamuiPhaseForm.
	public class KamuiPhaseBuff : ModBuff
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
