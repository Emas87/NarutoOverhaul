using System.Collections.Generic;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// Neji's fighting style, unlocked from the Chunin Exams arc (Shukaku fight). A direct melee
	// strike (not a projectile jutsu like Rasengan) that ignores a chunk of the target's defense -
	// chakra-guided precision strikes to the target's tenketsu points rather than raw damage.
	public class GentleFistItem : ModItem
	{
		public const float ChakraCost = 8f;
		public const float ArmorPenetration = 40f;

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.damage = 22;
			Item.DamageType = ModContent.GetInstance<ShinobiDamageClass>();
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 18;
			Item.useTime = 18;
			Item.autoReuse = true;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedShukaku && player.GetModPlayer<ChakraPlayer>().Chakra >= ChakraCost;
		}

		public override void UseAnimation(Player player)
		{
			player.GetModPlayer<ChakraPlayer>().TrySpendChakra(ChakraCost);
		}

		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.ArmorPenetration += ArmorPenetration;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "ChakraCost", $"Uses {ChakraCost} Chakra"));
			tooltips.Add(new TooltipLine(Mod, "ArmorPen", $"Strikes ignore {ArmorPenetration} defense"));
		}
	}
}
