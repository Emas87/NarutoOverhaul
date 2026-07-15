using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Minions;

namespace NarutoOverhaul.Content.Buffs
{
	// Standard tModLoader summon-buff idiom: keeps itself alive exactly as long as the tracked
	// minion projectile is still owned by the player, same pattern for all 4 animal summons.
	public class ToadMinionBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<ToadMinionProjectile>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
