using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	public class SixPathsSageModeBuff : ModBuff
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
