using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Same role as SageModeBuff: a marker/timer, stat application and chakra drain live on
	// TransformationPlayer.
	public class TailedBeastModeBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = false;
		}
	}
}
