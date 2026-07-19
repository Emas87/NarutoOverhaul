using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NarutoOverhaul.Common.Systems
{
	// World-scoped registry of placed Hiraishin Seal tiles - a shared, permanent fast-travel
	// network (like vanilla Teleporters/Pylons) rather than a per-player list: any player can warp
	// to any seal placed by anyone, and a seal stays in the registry until it's physically mined
	// out. Same save/net shape as StoryProgressSystem: NetSend/NetReceive cover join-time sync,
	// AddMark/RemoveMark push a manual update for already-connected clients since marks can change
	// mid-session, not just at world load.
	public class HiraishinMarkerSystem : ModSystem
	{
		public static List<Point16> MarkedTiles { get; private set; } = new List<Point16>();

		public override void OnWorldLoad()
		{
			MarkedTiles = new List<Point16>();
		}

		public override void SaveWorldData(TagCompound tag)
		{
			var xs = new List<int>(MarkedTiles.Count);
			var ys = new List<int>(MarkedTiles.Count);

			foreach (Point16 mark in MarkedTiles)
			{
				xs.Add(mark.X);
				ys.Add(mark.Y);
			}

			tag["hiraishinMarkX"] = xs;
			tag["hiraishinMarkY"] = ys;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			MarkedTiles = new List<Point16>();

			if (!tag.ContainsKey("hiraishinMarkX"))
			{
				return;
			}

			IList<int> xs = tag.GetList<int>("hiraishinMarkX");
			IList<int> ys = tag.GetList<int>("hiraishinMarkY");

			for (int i = 0; i < xs.Count; i++)
			{
				MarkedTiles.Add(new Point16(xs[i], ys[i]));
			}
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write((ushort)MarkedTiles.Count);

			foreach (Point16 mark in MarkedTiles)
			{
				writer.Write(mark.X);
				writer.Write(mark.Y);
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			ushort count = reader.ReadUInt16();
			var marks = new List<Point16>(count);

			for (int i = 0; i < count; i++)
			{
				short x = reader.ReadInt16();
				short y = reader.ReadInt16();
				marks.Add(new Point16(x, y));
			}

			MarkedTiles = marks;
		}

		public static void AddMark(Point16 tile)
		{
			if (MarkedTiles.Contains(tile))
			{
				return;
			}

			MarkedTiles.Add(tile);
			SyncToClients();
		}

		public static void RemoveMark(Point16 tile)
		{
			if (MarkedTiles.Remove(tile))
			{
				SyncToClients();
			}
		}

		private static void SyncToClients()
		{
			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.WorldData);
			}
		}
	}
}
