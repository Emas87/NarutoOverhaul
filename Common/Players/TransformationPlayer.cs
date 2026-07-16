using System.IO;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	public class TransformationPlayer : ModPlayer
	{
		public static ModKeybind ToggleSageModeKeybind;
		public static ModKeybind ToggleTailedBeastModeKeybind;
		public static ModKeybind ToggleSixPathsSageModeKeybind;
		public static ModKeybind ToggleEightGatesKeybind;
		public static ModKeybind ToggleChakraControlKeybind;

		private const int EightGatesFormIndex = 3;
		private const int ChakraControlFormIndex = 4;
		private const float EightGatesAdvanceStaminaCost = 10f;
		private const int EightGatesWindupTicks = 180; // ~3 seconds at 60 ticks/sec

		// -1 = no active form. Only one form active at a time for now; the registry design in
		// TransformationSystem means supporting simultaneous/stacked forms later is additive, not a rewrite.
		public int ActiveFormIndex = -1;

		// 0 = gates closed, 1-8 = current gate. Only meaningful while ActiveFormIndex == EightGatesFormIndex.
		public int EightGatesLevel;

		// -1 = not dying. Once gate 8 opens this counts up to EightGatesWindupTicks, then kills the player.
		// There is deliberately no cancel/safety-net once this starts - reaching gate 8 is meant to be fatal.
		private int eightGatesWindupTimer = -1;

		public override void Load()
		{
			ToggleSageModeKeybind = KeybindLoader.RegisterKeybind(Mod, "Toggle Sage Mode", "OemPeriod");
			ToggleTailedBeastModeKeybind = KeybindLoader.RegisterKeybind(Mod, "Toggle Tailed Beast Mode", "OemQuestion");
			ToggleSixPathsSageModeKeybind = KeybindLoader.RegisterKeybind(Mod, "Toggle Six Paths Sage Mode", "OemComma");
			ToggleEightGatesKeybind = KeybindLoader.RegisterKeybind(Mod, "Toggle Eight Gates", "OemOpenBrackets");
			ToggleChakraControlKeybind = KeybindLoader.RegisterKeybind(Mod, "Toggle Chakra Control", "OemCloseBrackets");
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (ToggleSageModeKeybind.JustPressed)
			{
				ToggleForm(0);
			}

			if (ToggleTailedBeastModeKeybind.JustPressed)
			{
				ToggleForm(1);
			}

			if (ToggleSixPathsSageModeKeybind.JustPressed)
			{
				ToggleForm(2);
			}

			if (ToggleEightGatesKeybind.JustPressed)
			{
				HandleEightGatesInput();
			}

			if (ToggleChakraControlKeybind.JustPressed)
			{
				ToggleForm(ChakraControlFormIndex);
			}
		}

		private void ToggleForm(int formIndex)
		{
			if (formIndex < 0 || formIndex >= TransformationSystem.RegisteredForms.Count)
			{
				return;
			}

			TransformationForm form = TransformationSystem.RegisteredForms[formIndex];

			if (ActiveFormIndex == formIndex)
			{
				DeactivateForm(form);
				return;
			}

			if (ActiveFormIndex != -1 || !form.IsUnlocked)
			{
				return;
			}

			ChakraPlayer chakraPlayer = Player.GetModPlayer<ChakraPlayer>();

			if (!chakraPlayer.TrySpendChakra(form.ActivationCost))
			{
				return;
			}

			Player.AddBuff(form.BuffType, 60 * 60 * 10); // long duration; real cutoff is the chakra drain below
			ActiveFormIndex = formIndex;
		}

		// Eight Gates isn't a simple on/off toggle: the same keybind activates it at Gate 1, then
		// each further press opens the next gate (up to 8) instead of deactivating - reaching 8 is
		// a deliberate, repeated choice, not an accident. It's Taijutsu, so it costs Stamina, not
		// Chakra like the other 3 forms.
		private void HandleEightGatesInput()
		{
			if (eightGatesWindupTimer >= 0)
			{
				return; // already committed to dying - input no longer does anything
			}

			TransformationForm form = TransformationSystem.RegisteredForms[EightGatesFormIndex];

			if (ActiveFormIndex != EightGatesFormIndex)
			{
				if (ActiveFormIndex != -1 || !form.IsUnlocked)
				{
					return;
				}

				StaminaPlayer staminaPlayer = Player.GetModPlayer<StaminaPlayer>();

				if (!staminaPlayer.TrySpendStamina(form.ActivationCost))
				{
					return;
				}

				Player.AddBuff(form.BuffType, 60 * 60 * 10);
				ActiveFormIndex = EightGatesFormIndex;
				EightGatesLevel = 1;
				return;
			}

			if (EightGatesLevel >= 8)
			{
				return;
			}

			StaminaPlayer stamina = Player.GetModPlayer<StaminaPlayer>();

			if (!stamina.TrySpendStamina(EightGatesAdvanceStaminaCost))
			{
				return;
			}

			EightGatesLevel++;

			if (EightGatesLevel >= 8)
			{
				eightGatesWindupTimer = 0;
			}
		}

		private void DeactivateForm(TransformationForm form)
		{
			Player.DelBuff(Player.FindBuffIndex(form.BuffType));

			if (ActiveFormIndex == EightGatesFormIndex)
			{
				EightGatesLevel = 0;
			}

			ActiveFormIndex = -1;
		}

		public override void ResetEffects()
		{
			if (ActiveFormIndex == -1)
			{
				return;
			}

			TransformationForm form = TransformationSystem.RegisteredForms[ActiveFormIndex];

			if (!Player.HasBuff(form.BuffType))
			{
				ActiveFormIndex = -1;
				return;
			}

			form.ApplyStatBoosts(Player);
		}

		public override void PreUpdateMovement()
		{
			if (ActiveFormIndex == -1)
			{
				return;
			}

			TransformationSystem.RegisteredForms[ActiveFormIndex].PreUpdateMovement(Player);
		}

		public override void PostUpdateMiscEffects()
		{
			if (eightGatesWindupTimer >= 0)
			{
				UpdateEightGatesWindup();
				return;
			}

			if (ActiveFormIndex == -1)
			{
				return;
			}

			TransformationForm form = TransformationSystem.RegisteredForms[ActiveFormIndex];

			// Eight Gates costs 0 chakra per tick (it costs Stamina to open/advance instead) - skip
			// the chakra spend entirely rather than calling TrySpendChakra(0), which would otherwise
			// keep resetting Chakra's regen-delay every tick for a form that never touches chakra.
			if (form.ChakraDrainPerTick > 0f)
			{
				ChakraPlayer chakraPlayer = Player.GetModPlayer<ChakraPlayer>();

				if (!chakraPlayer.TrySpendChakra(form.ChakraDrainPerTick))
				{
					DeactivateForm(form);
					return;
				}
			}

			// Forms like Eight Gates (gates 1-7) cost life instead of/alongside chakra - capped so
			// they can't kill the player. Gate 8 bypasses this entirely via the windup above.
			float lifeDrain = form.LifeDrainPerTick(Player);

			if (lifeDrain > 0f)
			{
				if (Player.statLife <= 1)
				{
					DeactivateForm(form);
					return;
				}

				Player.statLife = System.Math.Max(1, Player.statLife - (int)lifeDrain);
			}
		}

		private void UpdateEightGatesWindup()
		{
			eightGatesWindupTimer++;

			if (eightGatesWindupTimer % 10 == 0)
			{
				ChakraVFX.SpawnBurst(Player.Center, DustID.Torch, 8, 1.6f);
			}

			if (eightGatesWindupTimer < EightGatesWindupTicks)
			{
				return;
			}

			TransformationForm form = TransformationSystem.RegisteredForms[EightGatesFormIndex];
			Player.DelBuff(Player.FindBuffIndex(form.BuffType));
			ActiveFormIndex = -1;
			EightGatesLevel = 0;
			eightGatesWindupTimer = -1;

			Player.KillMe(PlayerDeathReason.ByCustomReason(Terraria.Localization.NetworkText.FromLiteral($"{Player.name} pushed the Eighth Gate too far")), 99999, 0, false);
		}

		// Without sync, ActiveFormIndex/EightGatesLevel only exist on the owning client - the
		// SageModeDrawLayer aura (which reads ActiveFormIndex off the *drawn* player, not just the
		// local one) never shows for anyone else, and other players can't tell someone's mid-Eight-
		// Gates. Both are small, rarely-changing ints, so the standard diff-on-tick
		// SendClientChanges pattern (cheap here, unlike a constantly-regenerating float) is a good fit.
		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)NetMessageType.SyncTransformation);
			packet.Write((byte)Player.whoAmI);
			packet.Write((sbyte)ActiveFormIndex);
			packet.Write((byte)EightGatesLevel);
			packet.Send(toWho, fromWho);
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			var clone = (TransformationPlayer)clientPlayer;

			if (clone.ActiveFormIndex != ActiveFormIndex || clone.EightGatesLevel != EightGatesLevel)
			{
				SyncPlayer(-1, Player.whoAmI, false);
			}
		}

		public override void CopyClientState(ModPlayer targetCopy)
		{
			var clone = (TransformationPlayer)targetCopy;
			clone.ActiveFormIndex = ActiveFormIndex;
			clone.EightGatesLevel = EightGatesLevel;
		}

		public static void HandlePacket(BinaryReader reader, int whoAmI)
		{
			byte playerIndex = reader.ReadByte();
			int activeFormIndex = reader.ReadSByte();
			int eightGatesLevel = reader.ReadByte();

			TransformationPlayer transformationPlayer = Main.player[playerIndex].GetModPlayer<TransformationPlayer>();
			transformationPlayer.ActiveFormIndex = activeFormIndex;
			transformationPlayer.EightGatesLevel = eightGatesLevel;

			if (Main.netMode == NetmodeID.Server)
			{
				ModPacket relay = ModContent.GetInstance<NarutoOverhaul>().GetPacket();
				relay.Write((byte)NetMessageType.SyncTransformation);
				relay.Write(playerIndex);
				relay.Write((sbyte)activeFormIndex);
				relay.Write((byte)eightGatesLevel);
				relay.Send(-1, whoAmI);
			}
		}
	}
}
