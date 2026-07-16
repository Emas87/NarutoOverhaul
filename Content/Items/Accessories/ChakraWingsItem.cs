using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Accessories
{
	// Six Paths-themed mobility - the mod's first wings equivalent, filling the "no mobility
	// endgame" gap. Gated one tier before the true final boss (Kaguya), same placement logic as
	// the endgame weapons' Madara-tier gates - a reward for reaching the back half of the story
	// rather than "you've basically already won."
	[AutoloadEquip(EquipType.Wings)]
	public class ChakraWingsItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 30);
			Item.rare = ItemRarityID.Red;
		}

		public override void SetStaticDefaults()
		{
			// flyTime (ticks), flySpeedOverride, accelerationMultiplier, hasHoldDownHoverFeatures,
			// hoverFlySpeedOverride, hoverAccelerationMultiplier - mid-tier Hardmode wings, roughly
			// between vanilla's Fairy Wings and Betsy's Wings.
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(200, 8f, 0.2f, true, 4f, 0.1f);
		}

		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
		{
			ascentWhenFalling = 0.85f;
			ascentWhenRising = 0.15f;
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 3f;
			constantAscend = 0.1f;
		}

		public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
		{
			speed = 9f;
			acceleration = 0.16f;
		}
	}
}
