using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// The target flees directly away from whoever cast it, rather than being puppeted toward
	// the caster's own input like GenjutsuControlDebuff.
	public class GenjutsuFearDebuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true;
		}
	}
}
