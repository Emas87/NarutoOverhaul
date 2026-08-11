using System.IO;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ID;
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

		// Tracks progress through the 7 story-order Chakra Scrolls (one per boss - see
		// NumberedChakraScrollItem). Doubles as the sequencing gate: scroll N requires this to
		// equal N-1, so scrolls can only ever be consumed in order.
		public const int MaxChakraScrolls = 7;
		public int ConsumedChakraScrolls;

		public const float BaseChakraRegenRate = 0.5f;
		public float ChakraRegenRate;

		// Accessory-driven bonus to Genjutsu control-debuff duration (in ticks) - lives here rather
		// than a dedicated ModPlayer since this class already serves as the mod-wide per-player stat
		// bag. Staged each frame like the fields above.
		public int GenjutsuControlDurationBonus;

		// Set by ChakraWingsItem.UpdateEquip while the wings are equipped and the player is
		// airborne (flying OR just falling) - regen otherwise works fine in midair (e.g. jumping
		// around casting jutsu), this is specifically the wings' "must land to refuel" gate so
		// hovering can't passively regenerate the fuel it's burning. Staged each frame like the
		// fields above.
		public bool SuppressRegenAirborne;

		private int regenDelayCounter;
		private int potionSicknessStacks;
		private int potionSicknessTimer;

		public override void Initialize()
		{
			Chakra = BaseMaxChakra;
		}

		// Called by ChakraPotionItem BEFORE applying its restore, so the first potion in a fresh
		// sequence is always full strength and each subsequent one (while still "sick") is
		// progressively weaker.
		public float GetPotionEffectivenessMultiplier()
		{
			return System.Math.Max(0f, 1f - (potionSicknessStacks * PotionSicknessConstants.StackPenalty));
		}

		// Called by ChakraPotionItem AFTER applying its restore - refreshes the timer and adds a
		// stack for the next potion to be weaker against.
		public void RegisterPotionUse()
		{
			potionSicknessStacks = System.Math.Min(PotionSicknessConstants.MaxStacks, potionSicknessStacks + 1);
			potionSicknessTimer = PotionSicknessConstants.Duration;
		}

		public override void ResetEffects()
		{
			// Staged each frame (same pattern as MaxChakra) so accessories can add to either via
			// UpdateEquip without the bonus compounding every tick.
			MaxChakra = BaseMaxChakra;
			ChakraRegenRate = BaseChakraRegenRate;
			GenjutsuControlDurationBonus = 0;
			SuppressRegenAirborne = false;

			if (Player.HasBuff(ModContent.BuffType<ChakraRegenBuff>()))
			{
				ChakraRegenRate += 1f;
			}
		}

		public override void PostUpdateMiscEffects()
		{
			if (potionSicknessTimer > 0)
			{
				potionSicknessTimer--;

				if (potionSicknessTimer == 0)
				{
					potionSicknessStacks = 0;
				}
			}

			if (Chakra > MaxChakra)
			{
				Chakra = MaxChakra;
			}

			if (regenDelayCounter > 0)
			{
				regenDelayCounter--;
				return;
			}

			if (Chakra < MaxChakra && !SuppressRegenAirborne)
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
			tag["consumedChakraScrolls"] = ConsumedChakraScrolls;
		}

		public override void LoadData(TagCompound tag)
		{
			BaseMaxChakra = tag.GetFloat("baseMaxChakra");
			ConsumedChakraScrolls = tag.GetInt("consumedChakraScrolls");

			// A legacy/partially-corrupt save could have baseMaxChakra missing/<=0 while the scroll
			// counter is still intact - reconstruct from the counter instead of flattening to the
			// bare default, or the scroll bonus is lost forever (the sequencing gate blocks
			// re-consuming scrolls already "used" according to the counter).
			if (BaseMaxChakra <= 0f)
			{
				BaseMaxChakra = 100f + (ConsumedChakraScrolls * NumberedChakraScrollItemIncreasePerScroll);
			}
		}

		public override void OnRespawn()
		{
			Chakra = MaxChakra;
		}

		// Named to avoid a circular reference to Content.Items.Consumables.NumberedChakraScrollItem
		// (which itself lives in a different assembly-load-order-sensitive namespace) - kept as a
		// local constant mirroring that item's ChakraIncreasePerScroll instead.
		private const float NumberedChakraScrollItemIncreasePerScroll = 20f;

		// PainBoss's Preta Path drains chakra server-side from AI(), which only ever touches the
		// server's own authoritative copy - without this, the client never learns their chakra was
		// spent. A one-off correction push (not continuous sync, which would spam every regen tick)
		// triggered right after the drain is the minimal correct fix.
		public static void SendCorrection(Player player)
		{
			ResourceCorrectionPacket.Send(NetMessageType.SyncChakraCorrection, player, player.GetModPlayer<ChakraPlayer>().Chakra);
		}

		public static void HandleCorrectionPacket(BinaryReader reader)
		{
			ResourceCorrectionPacket.Receive(reader, out byte playerIndex, out float chakra);
			Main.player[playerIndex].GetModPlayer<ChakraPlayer>().Chakra = chakra;
		}
	}
}
