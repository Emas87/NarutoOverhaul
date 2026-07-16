namespace NarutoOverhaul.Common.Systems
{
	// Single dispatch byte for every custom ModPacket this mod sends - written first in the packet,
	// read first in NarutoOverhaul.HandlePacket to route to the right handler.
	public enum NetMessageType : byte
	{
		SyncMoonLordBlessing,
		SyncTransformation,
		SyncChakraCorrection,
		SyncStaminaCorrection,
	}
}
