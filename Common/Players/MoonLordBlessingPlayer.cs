using System.IO;
using NarutoOverhaul.Common.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Players
{
	// Tracks whether this character has ever consumed the Otsutsuki Chakra Fragment - a permanent,
	// one-time blessing rather than an equipped accessory, so it can't be re-applied by re-equipping.
	public class MoonLordBlessingPlayer : ModPlayer
	{
		public bool HasKaguyaBlessing;

		public override void SaveData(TagCompound tag)
		{
			tag["hasKaguyaBlessing"] = HasKaguyaBlessing;
		}

		public override void LoadData(TagCompound tag)
		{
			HasKaguyaBlessing = tag.GetBool("hasKaguyaBlessing");
		}

		// MoonLordWeakenGlobalNPC.OnSpawn runs server-side and reads this flag for every player -
		// without sync, the server only knows the true value for a blessing consumed while
		// connected to this same server session, so a returning character's blessing was silently
		// ignored. SyncPlayer/SendClientChanges/CopyClientState is the standard tModLoader trio for
		// "push this rarely-changing per-player flag to the server/other clients."
		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)NetMessageType.SyncMoonLordBlessing);
			packet.Write((byte)Player.whoAmI);
			packet.Write(HasKaguyaBlessing);
			packet.Send(toWho, fromWho);
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			var clone = (MoonLordBlessingPlayer)clientPlayer;

			if (clone.HasKaguyaBlessing != HasKaguyaBlessing)
			{
				SyncPlayer(-1, Player.whoAmI, false);
			}
		}

		public override void CopyClientState(ModPlayer targetCopy)
		{
			var clone = (MoonLordBlessingPlayer)targetCopy;
			clone.HasKaguyaBlessing = HasKaguyaBlessing;
		}

		public static void HandlePacket(BinaryReader reader, int whoAmI)
		{
			byte playerIndex = reader.ReadByte();
			bool hasBlessing = reader.ReadBoolean();

			Main.player[playerIndex].GetModPlayer<MoonLordBlessingPlayer>().HasKaguyaBlessing = hasBlessing;

			if (Main.netMode == NetmodeID.Server)
			{
				ModPacket relay = ModContent.GetInstance<NarutoOverhaul>().GetPacket();
				relay.Write((byte)NetMessageType.SyncMoonLordBlessing);
				relay.Write(playerIndex);
				relay.Write(hasBlessing);
				relay.Send(-1, whoAmI);
			}
		}
	}
}
