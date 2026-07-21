using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Content.Minions
{
	// Tsunade's Katsuyu - slow and tanky-feeling, and unlike the other 3 pure-damage summons this
	// one also periodically heals the owner a small amount when nearby, matching Katsuyu's canon
	// role as a healer/support summon rather than being a plain damage-dealer clone.
	public class SlugMinionProjectile : AnimalMinionProjectile
	{
		protected override int BuffType => ModContent.BuffType<SlugMinionBuff>();
		protected override float MoveSpeed => 4f;
		protected override float AttackRange => 400f;
		protected override int IdleFrameCount => 4;
		protected override int AttackFrameCount => 12;

		private const int HealIntervalTicks = 300; // 5 seconds
		private const int HealAmount = 15;
		private const float HealRadius = 300f;

		private int healTimer;

		protected override void SetMinionSize()
		{
			Projectile.width = 34;
			Projectile.height = 22;
		}

		protected override void SetMinionDefaults()
		{
			Projectile.damage = 9;
			Projectile.knockBack = 1.5f;
		}

		protected override void UpdateVisuals(Player owner, NPC target)
		{
			healTimer++;

			if (healTimer >= HealIntervalTicks)
			{
				healTimer = 0;

				if (owner.whoAmI == Main.myPlayer && Projectile.Distance(owner.Center) <= HealRadius && owner.statLife < owner.statLifeMax2)
				{
					owner.HealEffect(HealAmount, true);
					owner.statLife = System.Math.Min(owner.statLifeMax2, owner.statLife + HealAmount);
				}
			}
		}
	}
}
