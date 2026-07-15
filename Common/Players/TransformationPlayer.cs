using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	public class TransformationPlayer : ModPlayer
	{
		public static ModKeybind ToggleSageModeKeybind;
		public static ModKeybind ToggleTailedBeastModeKeybind;
		public static ModKeybind ToggleSixPathsSageModeKeybind;

		// -1 = no active form. Only one form active at a time for now; the registry design in
		// TransformationSystem means supporting simultaneous/stacked forms later is additive, not a rewrite.
		public int ActiveFormIndex = -1;

		public override void Load()
		{
			ToggleSageModeKeybind = KeybindLoader.RegisterKeybind(Mod, "Toggle Sage Mode", "OemPeriod");
			ToggleTailedBeastModeKeybind = KeybindLoader.RegisterKeybind(Mod, "Toggle Tailed Beast Mode", "OemQuestion");
			ToggleSixPathsSageModeKeybind = KeybindLoader.RegisterKeybind(Mod, "Toggle Six Paths Sage Mode", "OemComma");
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

			if (!chakraPlayer.TrySpendChakra(form.ActivationChakraCost))
			{
				return;
			}

			Player.AddBuff(form.BuffType, 60 * 60 * 10); // long duration; real cutoff is the chakra drain below
			ActiveFormIndex = formIndex;
		}

		private void DeactivateForm(TransformationForm form)
		{
			Player.DelBuff(Player.FindBuffIndex(form.BuffType));
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

		public override void PostUpdateMiscEffects()
		{
			if (ActiveFormIndex == -1)
			{
				return;
			}

			TransformationForm form = TransformationSystem.RegisteredForms[ActiveFormIndex];
			ChakraPlayer chakraPlayer = Player.GetModPlayer<ChakraPlayer>();

			if (!chakraPlayer.TrySpendChakra(form.ChakraDrainPerTick))
			{
				DeactivateForm(form);
			}
		}
	}
}
