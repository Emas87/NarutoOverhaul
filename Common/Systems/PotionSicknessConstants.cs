namespace NarutoOverhaul.Common.Systems
{
	// Shared by ChakraPlayer and StaminaPlayer, which previously each hardcoded their own identical
	// copy of these three values - a balance tweak to one was easy to forget in the other.
	public static class PotionSicknessConstants
	{
		public const int Duration = 60 * 30; // 30 seconds of not drinking fully clears it
		public const float StackPenalty = 0.2f; // -20% restore per stack
		public const int MaxStacks = 5; // 5th+ stack in a row restores nothing
	}
}
