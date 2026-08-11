using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Systems
{
	// World-scoped story flags, same pattern as vanilla's downedBoss1/downedBoss2/etc.
	// Set from each story boss's OnKill() override; read by whatever jutsu/item that boss's
	// defeat is meant to unlock.
	public class StoryProgressSystem : ModSystem
	{
		public static bool DownedHaku;
		public static bool DownedShukaku;
		public static bool DownedOrochimaru;
		public static bool DownedKakuzu;
		public static bool DownedPain;
		public static bool DownedMadara;
		public static bool DownedKaguya;

		public override void SaveWorldData(TagCompound tag)
		{
			tag["downedHaku"] = DownedHaku;
			tag["downedShukaku"] = DownedShukaku;
			tag["downedOrochimaru"] = DownedOrochimaru;
			tag["downedKakuzu"] = DownedKakuzu;
			tag["downedPain"] = DownedPain;
			tag["downedMadara"] = DownedMadara;
			tag["downedKaguya"] = DownedKaguya;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			DownedHaku = tag.GetBool("downedHaku");
			DownedShukaku = tag.GetBool("downedShukaku");
			DownedOrochimaru = tag.GetBool("downedOrochimaru");
			DownedKakuzu = tag.GetBool("downedKakuzu");
			DownedPain = tag.GetBool("downedPain");
			DownedMadara = tag.GetBool("downedMadara");
			DownedKaguya = tag.GetBool("downedKaguya");
		}

		public override void OnWorldLoad()
		{
			DownedHaku = false;
			DownedShukaku = false;
			DownedOrochimaru = false;
			DownedKakuzu = false;
			DownedPain = false;
			DownedMadara = false;
			DownedKaguya = false;
		}

		// These flags are read client-side by every jutsu's CanUseItem, every transformation's
		// IsUnlocked, and every class vendor's shop condition - without netcode, a non-host client
		// never learns a boss died and stays locked out of everything that boss unlocks forever.
		// NetSend/NetReceive cover join-time sync (tModLoader calls these as part of the standard
		// mod-world-data handshake); SyncToClients must be called manually from each boss's OnKill
		// so already-connected clients pick up the change mid-session too.
		// Two BitsByte (16 bits) instead of one - 7 flags already fill 7 of a single byte's 8 bits,
		// so the next story boss added would silently need a wider wire format. Widening now leaves
		// room for 9 more flags before this needs revisiting again.
		public override void NetSend(BinaryWriter writer)
		{
			var flags = new BitsByte
			{
				[0] = DownedHaku,
				[1] = DownedShukaku,
				[2] = DownedOrochimaru,
				[3] = DownedKakuzu,
				[4] = DownedPain,
				[5] = DownedMadara,
				[6] = DownedKaguya,
			};
			BitsByte flags2 = default;

			writer.Write(flags);
			writer.Write(flags2);
		}

		public override void NetReceive(BinaryReader reader)
		{
			BitsByte flags = reader.ReadByte();
			_ = (BitsByte)reader.ReadByte();
			DownedHaku = flags[0];
			DownedShukaku = flags[1];
			DownedOrochimaru = flags[2];
			DownedKakuzu = flags[3];
			DownedPain = flags[4];
			DownedMadara = flags[5];
			DownedKaguya = flags[6];
		}

		public static void SyncToClients()
		{
			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.WorldData);
			}
		}
	}
}
