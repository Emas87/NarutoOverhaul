using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Players
{
	// Stamina powers Taijutsu, independent of Chakra (NinjutsuOverhaul's other resource, see
	// ChakraPlayer) - tuned to feel physical rather than mystical: faster regen, shorter
	// regen-delay-after-spend than Chakra.
	public class StaminaPlayer : ModPlayer
	{
		public const int RegenDelayTicks = 15;

		public float Stamina;
		public float MaxStamina;

		public float BaseMaxStamina = 100f;

		// Tracks progress through the 7 story-order Stamina Scrolls (one per boss - see
		// NumberedStaminaScrollItem). Doubles as the sequencing gate: scroll N requires this to
		// equal N-1, so scrolls can only ever be consumed in order.
		public const int MaxStaminaScrolls = 7;
		public int ConsumedStaminaScrolls;

		public const float BaseStaminaRegenRate = 1.2f;
		public float StaminaRegenRate;

		private int regenDelayCounter;

		public override void Initialize()
		{
			Stamina = BaseMaxStamina;
		}

		public override void ResetEffects()
		{
			MaxStamina = BaseMaxStamina;
			StaminaRegenRate = BaseStaminaRegenRate;

			if (Player.HasBuff(ModContent.BuffType<StaminaRegenBuff>()))
			{
				StaminaRegenRate += 2f;
			}
		}

		public override void PostUpdateMiscEffects()
		{
			if (Stamina > MaxStamina)
			{
				Stamina = MaxStamina;
			}

			if (regenDelayCounter > 0)
			{
				regenDelayCounter--;
				return;
			}

			if (Stamina < MaxStamina)
			{
				Stamina = System.Math.Min(MaxStamina, Stamina + StaminaRegenRate);
			}
		}

		public bool TrySpendStamina(float amount)
		{
			if (Stamina < amount)
			{
				return false;
			}

			Stamina -= amount;
			regenDelayCounter = RegenDelayTicks;
			return true;
		}

		public void IncreaseBaseMaxStamina(float amount)
		{
			BaseMaxStamina += amount;
			Stamina += amount;
		}

		public override void SaveData(TagCompound tag)
		{
			tag["baseMaxStamina"] = BaseMaxStamina;
			tag["consumedStaminaScrolls"] = ConsumedStaminaScrolls;
		}

		public override void LoadData(TagCompound tag)
		{
			BaseMaxStamina = tag.GetFloat("baseMaxStamina");
			ConsumedStaminaScrolls = tag.GetInt("consumedStaminaScrolls");

			if (BaseMaxStamina <= 0f)
			{
				BaseMaxStamina = 100f;
			}
		}

		public override void OnRespawn()
		{
			Stamina = MaxStamina;
		}
	}
}
