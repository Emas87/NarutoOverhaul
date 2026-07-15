using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Systems
{
	// Physical combat techniques (Gentle Fist, Eight Gates) - one of the mod's 3 core classes,
	// alongside Ninjutsu and Genjutsu, replacing the old placeholder ShinobiDamageClass.
	public class TaijutsuDamageClass : DamageClass
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
