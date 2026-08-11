using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Systems
{
	// Shared base for the mod's 3 core damage classes (Ninjutsu/Taijutsu/Genjutsu) - each inherits
	// modifiers/effects from Generic only, and previously copy-pasted this same override into all
	// 3 files identically.
	public abstract class CombatStyleDamageClass : DamageClass
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
