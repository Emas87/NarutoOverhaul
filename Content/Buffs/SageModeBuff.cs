using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Mostly a marker + timer via vanilla buff machinery. Stat application, chakra drain, and
	// auto-cancel-on-empty all live on TransformationPlayer, which operates generically over
	// "the currently active form" rather than special-casing this buff.
	public class SageModeBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = false;
		}
	}
}
