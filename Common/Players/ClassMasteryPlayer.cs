using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Players
{
	// Permanent, one-time class damage boosts unlocked by Mastery Scroll consumables (see
	// Content/Items/Consumables/*MasteryScrollItem.cs) - unlike accessories, these survive
	// unequipping since they're driven by a persisted flag rather than worn gear.
	public class ClassMasteryPlayer : ModPlayer
	{
		public const float MasteryDamageBonus = 0.1f;

		public bool HasTaijutsuMastery;
		public bool HasNinjutsuMastery;
		public bool HasGenjutsuMastery;

		public override void ResetEffects()
		{
			if (HasTaijutsuMastery)
			{
				Player.GetDamage(ModContent.GetInstance<TaijutsuDamageClass>()) += MasteryDamageBonus;
			}

			if (HasNinjutsuMastery)
			{
				Player.GetDamage(ModContent.GetInstance<NinjutsuDamageClass>()) += MasteryDamageBonus;
			}

			if (HasGenjutsuMastery)
			{
				Player.GetDamage(ModContent.GetInstance<GenjutsuDamageClass>()) += MasteryDamageBonus;
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["hasTaijutsuMastery"] = HasTaijutsuMastery;
			tag["hasNinjutsuMastery"] = HasNinjutsuMastery;
			tag["hasGenjutsuMastery"] = HasGenjutsuMastery;
		}

		public override void LoadData(TagCompound tag)
		{
			HasTaijutsuMastery = tag.GetBool("hasTaijutsuMastery");
			HasNinjutsuMastery = tag.GetBool("hasNinjutsuMastery");
			HasGenjutsuMastery = tag.GetBool("hasGenjutsuMastery");
		}
	}
}
