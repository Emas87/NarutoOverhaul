using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Players
{
	// Tracks the one-time Cursed Seal branding (same permanent-consumable pattern as
	// MoonLordBlessingPlayer). CurseMarkForm.IsUnlocked(Player) takes whichever player is being
	// checked, but its only call site (TransformationPlayer.ToggleForm, from client-side
	// ProcessTriggers) always means the local player - so this field is only ever meaningfully
	// read for its own owning client, and doesn't need SyncPlayer for that to stay correct. Form
	// activation itself already syncs via TransformationPlayer if that assumption ever changes.
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
