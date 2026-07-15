using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Buffs
{
	// Total immobilization (zero velocity, no AI) - shorter duration than the puppet-control
	// debuff since there's no player-facing "fun" being had while it's active, just a hard lock.
	public class GenjutsuSleepDebuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true;
		}
	}
}
