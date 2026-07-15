using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Minions
{
	// Kakashi's ninja hound pack (Pakkun & friends) - fastest of the 4, low attack cooldown.
	public class NinjaHoundMinionProjectile : AnimalMinionProjectile
	{
		protected override int BuffType => ModContent.BuffType<NinjaHoundMinionBuff>();
		protected override float MoveSpeed => 9f;
		protected override float AttackRange => 550f;

		protected override void SetMinionSize()
		{
			Projectile.width = 26;
			Projectile.height = 20;
		}

		protected override void SetMinionDefaults()
		{
			Projectile.damage = 11;
			Projectile.knockBack = 2f;
		}

		protected override void UpdateVisuals(Player owner, NPC target)
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;
		}
	}
}
