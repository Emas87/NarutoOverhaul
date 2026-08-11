using System.IO;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Players
{
	// Stamina powers Taijutsu, independent of Chakra (NinjutsuOverhaul's other resource, see
	// ChakraPlayer) - tuned to feel physical rather than mystical: faster regen, shorter
	// regen-delay-after-spend than Chakra.
	public class StaminaPlayer : ModPlayer
	{
		public static ModKeybind SprintKeybind;

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

		// Fixed rate - deliberately does NOT scale with MaxStamina. A base ~100 Stamina character
		// empties this in ~5 seconds of continuous running; a Taijutsu-geared character with a much
		// bigger MaxStamina pool (see TaijutsuBodyItem/WeightedLegWarmersItem/Stamina Scrolls) runs
		// proportionally longer at this same rate - "almost free" running falls out of the existing
		// Stamina-growth systems with no class check needed here.
		private const float RunStaminaDrainPerTick = 100f / (5f * 60f);

		private int regenDelayCounter;
		private bool isRunning;
		private int potionSicknessStacks;
		private int potionSicknessTimer;

		public override void Load()
		{
			SprintKeybind = KeybindLoader.RegisterKeybind(Mod, "Sprint", "LeftShift");
		}

		public override void Initialize()
		{
			Stamina = BaseMaxStamina;
		}

		// Called by StaminaPotionItem BEFORE applying its restore, so the first potion in a fresh
		// sequence is always full strength and each subsequent one (while still "sick") is
		// progressively weaker.
		public float GetPotionEffectivenessMultiplier()
		{
			return System.Math.Max(0f, 1f - (potionSicknessStacks * PotionSicknessConstants.StackPenalty));
		}

		// Called by StaminaPotionItem AFTER applying its restore - refreshes the timer and adds a
		// stack for the next potion to be weaker against.
		public void RegisterPotionUse()
		{
			potionSicknessStacks = System.Math.Min(PotionSicknessConstants.MaxStacks, potionSicknessStacks + 1);
			potionSicknessTimer = PotionSicknessConstants.Duration;
		}

		public override void ResetEffects()
		{
			MaxStamina = BaseMaxStamina;
			StaminaRegenRate = BaseStaminaRegenRate;

			if (Player.HasBuff(ModContent.BuffType<StaminaRegenBuff>()))
			{
				StaminaRegenRate += 2f;
			}

			// Running: hold Sprint + a direction for +50% move speed, gated purely on having
			// Stamina left - not on any class/mount check beyond excluding mounts (which have their
			// own speed system). Set here (not PreUpdateMovement) per TransformationForm's
			// convention that moveSpeed has to be staged before vanilla's movement code reads it.
			isRunning = SprintKeybind.Current && (Player.controlLeft || Player.controlRight) && !Player.mount.Active && Stamina > 0f;

			if (isRunning)
			{
				Player.moveSpeed += 0.5f;
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

			if (isRunning)
			{
				// Not TrySpendStamina - that's all-or-nothing (correct for one-shot ability costs),
				// but a continuous per-tick drain needs to deplete-and-stop at exactly 0 instead of
				// getting stuck on a residual smaller than one tick's drain amount forever.
				Stamina = System.Math.Max(0f, Stamina - RunStaminaDrainPerTick);
				regenDelayCounter = RegenDelayTicks;
			}

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

			// See ChakraPlayer.LoadData for why this reconstructs from the scroll counter instead
			// of flattening to the bare default on a legacy/partially-corrupt save.
			if (BaseMaxStamina <= 0f)
			{
				BaseMaxStamina = 100f + (ConsumedStaminaScrolls * NumberedStaminaScrollItemIncreasePerScroll);
			}
		}

		public override void OnRespawn()
		{
			Stamina = MaxStamina;
		}

		private const float NumberedStaminaScrollItemIncreasePerScroll = 20f;

		// No server-side Stamina drain exists yet (unlike Pain's Chakra drain), but this keeps
		// Stamina symmetric with Chakra's correction-packet mechanism for whenever one is added.
		public static void SendCorrection(Player player)
		{
			ResourceCorrectionPacket.Send(NetMessageType.SyncStaminaCorrection, player, player.GetModPlayer<StaminaPlayer>().Stamina);
		}

		public static void HandleCorrectionPacket(BinaryReader reader)
		{
			ResourceCorrectionPacket.Receive(reader, out byte playerIndex, out float stamina);
			Main.player[playerIndex].GetModPlayer<StaminaPlayer>().Stamina = stamina;
		}
	}
}
