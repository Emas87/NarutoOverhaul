using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Marker/timer only - the actual regen-rate bonus is applied in ChakraPlayer.ResetEffects,
	// right after ChakraRegenRate is staged from BaseChakraRegenRate each frame.
	public class ChakraRegenBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = false;
		}
	}
}
