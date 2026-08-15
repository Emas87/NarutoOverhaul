using NarutoOverhaul.Content.Buffs;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Minions
{
	// Second of Kakashi's ninja hound pack - stockier bulldog build, trades speed for a harder hit.
	public class BulldogHoundMinionProjectile : AnimalMinionProjectile
	{
		protected override int BuffType => ModContent.BuffType<BulldogHoundMinionBuff>();
		protected override float MoveSpeed => 7f;
		protected override float AttackRange => 450f;
		protected override int IdleFrameCount => 6;
		protected override int AttackFrameCount => 9;

		protected override void SetMinionSize()
		{
			Projectile.width = 26;
			Projectile.height = 20;
		}

		protected override void SetMinionDefaults()
		{
			Projectile.damage = 14;
			Projectile.knockBack = 3f;
		}
	}
}
