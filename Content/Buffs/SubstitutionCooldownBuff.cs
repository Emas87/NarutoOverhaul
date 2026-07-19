using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Cooldown marker for SubstitutionPlayer - the buff's own remaining time IS the cooldown.
	public class SubstitutionCooldownBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}
	}
}
