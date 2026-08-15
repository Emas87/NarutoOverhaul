using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.Players
{
	// TEMP debug aid for tracking down "something invisible is hitting me" boss reports (see
	// TailedBeastBoss investigation) - logs exactly what dealt the hit (NPC type/name/position vs
	// the player's own position) plus every currently-active NPC nearby, to
	// tModLoader-Logs/client.log. Grep that file for "[HitDebug]" after reproducing a hit. Remove
	// once boss hitbox/position issues are confirmed fixed.
	public class HitDebugLoggerPlayer : ModPlayer
	{
		private const float NearbyLogRangePx = 2500f;

		public override void PostHurt(Player.HurtInfo info)
		{
			string source = DescribeSource(info.DamageSource);

			Mod.Logger.Info($"[HitDebug] Player hit for {info.Damage} by {source} | player pos={Player.position} center={Player.Center} hitbox={Player.Hitbox}");

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];

				if (!npc.active)
				{
					continue;
				}

				float distance = Vector2.Distance(npc.Center, Player.Center);

				if (distance < NearbyLogRangePx)
				{
					Mod.Logger.Info($"[HitDebug] nearby NPC #{i}: {npc.FullName} (type={npc.type}) pos={npc.position} center={npc.Center} hitbox={npc.Hitbox} velocity={npc.velocity} dist={distance:F0}");
				}
			}
		}

		private static string DescribeSource(Terraria.DataStructures.PlayerDeathReason damageSource)
		{
			if (damageSource == null)
			{
				return "unknown source";
			}

			if (damageSource.SourceNPCIndex >= 0 && damageSource.SourceNPCIndex < Main.maxNPCs)
			{
				NPC npc = Main.npc[damageSource.SourceNPCIndex];
				return $"NPC #{damageSource.SourceNPCIndex} {npc.FullName} (type={npc.type}) pos={npc.position} center={npc.Center} hitbox={npc.Hitbox}";
			}

			if (damageSource.SourceProjectileLocalIndex >= 0 && damageSource.SourceProjectileLocalIndex < Main.maxProjectiles)
			{
				Projectile proj = Main.projectile[damageSource.SourceProjectileLocalIndex];
				return $"Projectile #{damageSource.SourceProjectileLocalIndex} type={damageSource.SourceProjectileType} pos={proj.position} center={proj.Center}";
			}

			if (damageSource.SourceOtherIndex >= 0)
			{
				return $"other source index={damageSource.SourceOtherIndex}";
			}

			if (damageSource.SourcePlayerIndex >= 0)
			{
				return $"player #{damageSource.SourcePlayerIndex}";
			}

			return "unattributed (fall damage, lava, etc.)";
		}
	}
}
