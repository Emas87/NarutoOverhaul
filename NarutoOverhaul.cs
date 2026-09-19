using System.IO;
using NarutoOverhaul.Common.Players;
using NarutoOverhaul.Common.Systems;
using Terraria.ModLoader;

namespace NarutoOverhaul
{
	public class NarutoOverhaul : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			var messageType = (NetMessageType)reader.ReadByte();

			switch (messageType)
			{
				case NetMessageType.SyncMoonLordBlessing:
					MoonLordBlessingPlayer.HandlePacket(reader, whoAmI);
					break;
				case NetMessageType.SyncTransformation:
					TransformationPlayer.HandlePacket(reader, whoAmI);
					break;
				case NetMessageType.SyncChakraCorrection:
					ChakraPlayer.HandleCorrectionPacket(reader);
					break;
				case NetMessageType.SyncStaminaCorrection:
					StaminaPlayer.HandleCorrectionPacket(reader);
					break;
				case NetMessageType.HiraishinAddMark:
					HiraishinMarkerSystem.HandleAddMarkPacket(reader);
					break;
			}
		}
	}
}
