using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace NarutoOverhaul.Common.Systems
{
	// Server-side: OnSpawn only runs on whichever machine calls NPC.NewNPC (the server in
	// dedicated/host-and-play multiplayer), so EnableSoftBossScaling is inherently one shared,
	// session-wide decision baked into boss stats at spawn - not a genuine per-client toggle
	// (2026-09-24 /fix-unfixes decision: matches what the code already does).
	public class NarutoOverhaulConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// Tooltip text lives in Localization/en-US_Mods.NarutoOverhaul.hjson under Configs -
		// TooltipAttribute is obsolete in favor of the auto-generated localization key.
		[Header("Difficulty")]
		[DefaultValue(true)]
		public bool EnableSoftBossScaling = true;

		[Header("HUD")]
		[DefaultValue(true)]
		public bool ShowActiveFormIndicator = true;

		[DefaultValue(true)]
		public bool ShowResourceBars = true;
	}
}
