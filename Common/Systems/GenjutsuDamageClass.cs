using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Systems
{
	// Illusion/control techniques - low direct damage by design (see GenjutsuIllusionItem),
	// the debuff/control-focused counterpart to Taijutsu and Ninjutsu.
	public class GenjutsuDamageClass : DamageClass
	{
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
		{
			if (damageClass == Generic)
			{
				return StatInheritanceData.Full;
			}

			return base.GetModifierInheritance(damageClass);
		}

		public override bool GetEffectInheritance(DamageClass damageClass) => damageClass == Generic;
	}
}
