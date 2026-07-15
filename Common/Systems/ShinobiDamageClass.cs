using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Systems
{
	// Jutsu scale off their own damage class instead of borrowing vanilla magic/summon damage,
	// so gear/accessories can target "Shinobi damage" specifically as more jutsu are added.
	public class ShinobiDamageClass : DamageClass
	{
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
		{
			// Generic damage/crit boosts (most accessories) apply to Shinobi damage too.
			if (damageClass == Generic)
			{
				return StatInheritanceData.Full;
			}

			return base.GetModifierInheritance(damageClass);
		}

		public override bool GetEffectInheritance(DamageClass damageClass) => damageClass == Generic;
	}
}
