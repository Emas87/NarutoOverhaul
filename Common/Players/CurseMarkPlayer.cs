using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Players
{
	// Tracks the one-time Cursed Seal branding (same permanent-consumable pattern as
	// MoonLordBlessingPlayer). Only read client-side by CurseMarkForm.IsUnlocked, so no
	// SyncPlayer needed - form activation itself already syncs via TransformationPlayer.
	public class CurseMarkPlayer : ModPlayer
	{
		public bool HasCurseMark;

		public override void SaveData(TagCompound tag)
		{
			tag["hasCurseMark"] = HasCurseMark;
		}

		public override void LoadData(TagCompound tag)
		{
			HasCurseMark = tag.GetBool("hasCurseMark");
		}
	}
}
