using System.Collections.Generic;
using NarutoOverhaul.Common.GlobalNPCs;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Items.Weapons.Jutsu
{
	// A short-range AoE fear illusion - melee-hitbox so it naturally catches multiple enemies in
	// one swing (like Leaf Hurricane), applying GenjutsuFearDebuff instead of dealing real damage.
	// noUseGraphic + Thrust (not Swing) so it reads as a hand-seal pulse instead of a sword slash -
	// a genjutsu should never look like it's swinging a blade.
	public class GenjutsuNightmareItem : ModItem
	{
		public const float ChakraCost = 20f;
		public const int FearDuration = 180; // 3 seconds

		// UseAnimation can't abort the swing outright (unlike Shoot returning false), so the debuff
		// application in OnHitNPC is gated on this instead - keeps a failed spend from applying the
		// effect for free.
		private bool chakraSpent;

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.damage = 6;
			Item.DamageType = ModContent.GetInstance<GenjutsuDamageClass>();
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.noUseGraphic = true;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.knockBack = 1f;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item43;
		}

		public override bool CanUseItem(Player player)
		{
			return StoryProgressSystem.DownedHaku && player.GetModPlayer<ChakraPlayer>().Chakra >= ChakraCost;
		}

		public override void UseAnimation(Player player)
		{
			chakraSpent = player.GetModPlayer<ChakraPlayer>().TrySpendChakra(ChakraCost);
			ChakraVFX.SpawnGenjutsuBurst(player.Center, 1.2f);
		}

		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!chakraSpent)
			{
				return;
			}

			int duration = FearDuration + player.GetModPlayer<ChakraPlayer>().GenjutsuControlDurationBonus;

			target.AddBuff(ModContent.BuffType<GenjutsuFearDebuff>(), duration);
			target.GetGlobalNPC<GenjutsuGlobalNPC>().ControllingPlayerIndex = player.whoAmI;
			target.netUpdate = true; // force an immediate sync so the server/other clients learn who's controlling this flee
			ChakraVFX.SpawnGenjutsuBurst(target.Center, 1.5f);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "ChakraCost", $"Uses {ChakraCost} Chakra"));
			tooltips.Add(new TooltipLine(Mod, "StoryRequirement", "Requires Haku defeated"));
			tooltips.Add(new TooltipLine(Mod, "Fear", "Terrifies nearby enemies into fleeing"));
		}
	}
}
