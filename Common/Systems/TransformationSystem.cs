using System.Collections.Generic;
using NarutoOverhaul.Content.Buffs;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Systems
{
	public class TransformationSystem : ModSystem
	{
		public static List<TransformationForm> RegisteredForms { get; private set; } = new List<TransformationForm>();

		// Resolves a form's index by type instead of by hardcoded position - TransformationPlayer
		// needs to single out a few specific forms (Eight Gates, Chakra Control, ...) by index for
		// its own bespoke handling of each, but hardcoding those indices as magic numbers would
		// silently break (rebind the wrong keybind to the wrong form) if this list's registration
		// order below ever changes.
		public static int IndexOfForm<T>() where T : TransformationForm
		{
			return RegisteredForms.FindIndex(form => form is T);
		}

		public override void PostSetupContent()
		{
			RegisteredForms.Clear();
			RegisteredForms.Add(new SageModeForm());
			RegisteredForms.Add(new TailedBeastModeForm());
			RegisteredForms.Add(new SixPathsSageModeForm());
			RegisteredForms.Add(new EightGatesForm());
			RegisteredForms.Add(new ChakraControlForm());
			RegisteredForms.Add(new KamuiPhaseForm());
			RegisteredForms.Add(new ByakuganForm());
			RegisteredForms.Add(new CurseMarkForm());
		}
	}
}
