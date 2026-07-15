using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Systems
{
	// World-scoped story flags, same pattern as vanilla's downedBoss1/downedBoss2/etc.
	// Set from each story boss's OnKill() override; read by whatever jutsu/item that boss's
	// defeat is meant to unlock.
	public class StoryProgressSystem : ModSystem
	{
		public static bool DownedHaku;
		public static bool DownedShukaku;
		public static bool DownedOrochimaru;
		public static bool DownedKakuzu;
		public static bool DownedPain;
		public static bool DownedMadara;
		public static bool DownedKaguya;

		public override void SaveWorldData(TagCompound tag)
		{
			tag["downedHaku"] = DownedHaku;
			tag["downedShukaku"] = DownedShukaku;
			tag["downedOrochimaru"] = DownedOrochimaru;
			tag["downedKakuzu"] = DownedKakuzu;
			tag["downedPain"] = DownedPain;
			tag["downedMadara"] = DownedMadara;
			tag["downedKaguya"] = DownedKaguya;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			DownedHaku = tag.GetBool("downedHaku");
			DownedShukaku = tag.GetBool("downedShukaku");
			DownedOrochimaru = tag.GetBool("downedOrochimaru");
			DownedKakuzu = tag.GetBool("downedKakuzu");
			DownedPain = tag.GetBool("downedPain");
			DownedMadara = tag.GetBool("downedMadara");
			DownedKaguya = tag.GetBool("downedKaguya");
		}

		public override void OnWorldLoad()
		{
			DownedHaku = false;
			DownedShukaku = false;
			DownedOrochimaru = false;
			DownedKakuzu = false;
			DownedPain = false;
			DownedMadara = false;
			DownedKaguya = false;
		}
	}
}
