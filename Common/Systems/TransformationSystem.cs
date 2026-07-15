using System.Collections.Generic;
using NarutoOverhaul.Content.Buffs;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Systems
{
	public class TransformationSystem : ModSystem
	{
		public static List<TransformationForm> RegisteredForms { get; private set; } = new List<TransformationForm>();

		public override void PostSetupContent()
		{
			RegisteredForms.Clear();
			RegisteredForms.Add(new SageModeForm());
			RegisteredForms.Add(new TailedBeastModeForm());
		}
	}
}
