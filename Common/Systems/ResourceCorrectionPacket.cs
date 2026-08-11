using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Systems
{
	// Shared shape for a one-off server->single-client correction packet, used identically by
	// ChakraPlayer (Pain's Preta drain) and StaminaPlayer (kept symmetric for whenever a
	// server-side Stamina drain is added). Not used by the broadcast-relay packets
	// (MoonLordBlessingPlayer/TransformationPlayer) - those sync a value to every peer rather than
	// correcting one player's own copy, a different enough shape not to force into this helper.
	public static class ResourceCorrectionPacket
	{
		public static void Send(NetMessageType type, Player player, float value)
		{
			if (Main.netMode != NetmodeID.Server)
			{
				return;
			}

			ModPacket packet = ModContent.GetInstance<NarutoOverhaul>().GetPacket();
			packet.Write((byte)type);
			packet.Write((byte)player.whoAmI);
			packet.Write(value);
			packet.Send(player.whoAmI);
		}

		public static void Receive(BinaryReader reader, out byte playerIndex, out float value)
		{
			playerIndex = reader.ReadByte();
			value = reader.ReadSingle();
		}
	}
}
