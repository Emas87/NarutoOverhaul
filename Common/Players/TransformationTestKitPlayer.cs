using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Items.Weapons;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// TEMPORARY testing aid: force-unlocks every TransformationForm's IsUnlocked gate (see
	// Common/Systems/TransformationForm.cs and each Content/Buffs/*Form.cs) so every form can be
	// toggled immediately without playing through the story - Byakugan/SixPathsSageMode/
	// TailedBeastMode/EightGates/SageMode all read StoryProgressSystem's Downed* flags,
	// CurseMarkForm reads CurseMarkPlayer.HasCurseMark instead. ChakraControlForm needs nothing
	// extra (always unlocked).
	//
	// KamuiPhaseForm.IsUnlocked checks Main.hardMode directly, not a per-player/per-world flag -
	// the only way to satisfy that is the real WorldGen.StartHardmode() conversion (same call
	// vanilla makes when you break a Demon/Crimson altar post-Wall of Flesh: hardmode ore,
	// V/S/H biome spread, tougher enemies). Explicitly opted into for this world - not something
	// to do by default in a debug tool, but requested here specifically to unblock testing Kamui
	// Phase. Runs once (guarded on !Main.hardMode), not every tick.
	//
	// Forms themselves are toggled purely by the ModKeybinds registered in TransformationPlayer
	// (defaults: see that file's RegisterKeybind calls), not consumables - but Hiraishin Warp
	// (also on TransformationPlayer, its own keybind) needs at least 2 planted HiraishinSealTiles to
	// warp between, so a stack of the thrown Minato Kunai is handed out here too (throw it at a
	// wall to plant a seal - see MinatoKunaiProjectile.OnTileCollide). Remove once transformation
	// testing is done - this is not meant to ship.
	//
	// NOTE: this used to also force Chakra/Stamina to full every tick as a "can't afford to
	// activate" safety net. Removed - it masked the actual per-tick drain entirely (Chakra looked
	// like it "regenerated immediately after every use" because it was being stomped back to max
	// the same tick), which broke testing the drain/deactivate-on-empty behavior itself. The real
	// cause of the original "can't toggle any form" report turned out to be unbound keybinds (see
	// ShinobiHandbookItem), not low Chakra - full pools at world entry (ChakraPlayer.Initialize
	// already starts you at MaxChakra) are enough headroom to activate any form.
	public class TransformationTestKitPlayer : ModPlayer
	{
		public override void OnEnterWorld()
		{
			StoryProgressSystem.DownedShukaku = true;
			StoryProgressSystem.DownedPain = true;
			StoryProgressSystem.DownedMadara = true;
			StoryProgressSystem.DownedKaguya = true;

			Player.GetModPlayer<CurseMarkPlayer>().HasCurseMark = true;

			if (!Main.hardMode)
			{
				WorldGen.StartHardmode();
			}

			int kunaiType = ModContent.ItemType<MinatoKunaiItem>();
			if (!Player.HasItem(kunaiType))
			{
				Player.QuickSpawnItem(Player.GetSource_Misc("TransformationTestKit"), kunaiType, 10);
			}
		}
	}
}
