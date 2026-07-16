using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace NarutoOverhaul.Common.Systems
{
	// Client-side only - nothing here affects gameplay balance in a way that needs to be
	// server-authoritative (the boss scaling toggle only affects the reading client's own local
	// experience of an already-existing, purely additive difficulty nudge).
	public class NarutoOverhaulConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// Tooltip text lives in Localization/en-US_Mods.NarutoOverhaul.hjson under Configs -
		// TooltipAttribute is obsolete in favor of the auto-generated localization key.
		[Header("Difficulty")]
		[DefaultValue(true)]
		public bool EnableSoftBossScaling = true;

		[Header("HUD")]
		[DefaultValue(true)]
		public bool ShowActiveFormIndicator = true;
	}
}
