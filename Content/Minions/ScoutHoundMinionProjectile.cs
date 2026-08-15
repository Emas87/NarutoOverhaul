using NarutoOverhaul.Content.Buffs;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Minions
{
	// Third of Kakashi's ninja hound pack - the sunglasses-and-vest tracker (Pakkun-styled), sniffs
	// out targets from much further away than the other two but hits the softest.
	public class ScoutHoundMinionProjectile : AnimalMinionProjectile
	{
		protected override int BuffType => ModContent.BuffType<ScoutHoundMinionBuff>();
		protected override float MoveSpeed => 8f;
		protected override float AttackRange => 700f;
		protected override int IdleFrameCount => 6;
		protected override int AttackFrameCount => 9;

		protected override void SetMinionSize()
		{
			Projectile.width = 26;
			Projectile.height = 20;
		}

		protected override void SetMinionDefaults()
		{
			Projectile.damage = 10;
			Projectile.knockBack = 1.5f;
		}
	}
}
