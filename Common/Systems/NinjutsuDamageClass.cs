using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Systems
{
	// Chakra-molded ranged techniques (Rasengan, Fireball) - one of the mod's 3 core classes,
	// alongside Taijutsu and Genjutsu, replacing the old placeholder ShinobiDamageClass.
	public class NinjutsuDamageClass : DamageClass
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
