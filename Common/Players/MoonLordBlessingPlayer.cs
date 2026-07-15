using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Players
{
	// Tracks whether this character has ever consumed the Otsutsuki Chakra Fragment - a permanent,
	// one-time blessing rather than an equipped accessory, so it can't be re-applied by re-equipping.
	public class MoonLordBlessingPlayer : ModPlayer
	{
		public bool HasKaguyaBlessing;

		public override void SaveData(TagCompound tag)
		{
			tag["hasKaguyaBlessing"] = HasKaguyaBlessing;
		}

		public override void LoadData(TagCompound tag)
		{
			HasKaguyaBlessing = tag.GetBool("hasKaguyaBlessing");
		}
	}
}
