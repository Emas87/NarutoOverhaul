using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Players
{
	// Chakra is a fully independent resource from vanilla mana - not a reskin of Player.statMana.
	public class ChakraPlayer : ModPlayer
	{
		public const int RegenDelayTicks = 30;

		public float Chakra;
		public float MaxChakra;

		// Grows permanently via consumables (e.g. a future "Chakra Scroll" item calls IncreaseBaseMaxChakra).
		// Persisted across sessions; current Chakra is not persisted, matching vanilla mana-on-respawn behavior.
		public float BaseMaxChakra = 100f;

		public const float BaseChakraRegenRate = 0.5f;
		public float ChakraRegenRate;

		// Accessory-driven bonus to Genjutsu control-debuff duration (in ticks) - lives here rather
		// than a dedicated ModPlayer since this class already serves as the mod-wide per-player stat
		// bag. Staged each frame like the fields above.
		public int GenjutsuControlDurationBonus;

		private int regenDelayCounter;

		public override void Initialize()
		{
			Chakra = BaseMaxChakra;
		}

		public override void ResetEffects()
		{
			// Staged each frame (same pattern as MaxChakra) so accessories can add to either via
			// UpdateEquip without the bonus compounding every tick.
			MaxChakra = BaseMaxChakra;
			ChakraRegenRate = BaseChakraRegenRate;
			GenjutsuControlDurationBonus = 0;
		}

		public override void PostUpdateMiscEffects()
		{
			if (Chakra > MaxChakra)
			{
				Chakra = MaxChakra;
			}

			if (regenDelayCounter > 0)
			{
				regenDelayCounter--;
				return;
			}

			if (Chakra < MaxChakra)
			{
				Chakra = System.Math.Min(MaxChakra, Chakra + ChakraRegenRate);
			}
		}

		public bool TrySpendChakra(float amount)
		{
			if (Chakra < amount)
			{
				return false;
			}

			Chakra -= amount;
			regenDelayCounter = RegenDelayTicks;
			return true;
		}

		public void IncreaseBaseMaxChakra(float amount)
		{
			BaseMaxChakra += amount;
			Chakra += amount;
		}

		public override void SaveData(TagCompound tag)
		{
			tag["baseMaxChakra"] = BaseMaxChakra;
		}

		public override void LoadData(TagCompound tag)
		{
			BaseMaxChakra = tag.GetFloat("baseMaxChakra");

			if (BaseMaxChakra <= 0f)
			{
				BaseMaxChakra = 100f;
			}
		}

		public override void OnRespawn()
		{
			Chakra = MaxChakra;
		}
	}
}
