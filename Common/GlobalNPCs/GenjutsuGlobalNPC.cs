using Microsoft.Xna.Framework;
using NarutoOverhaul.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace NarutoOverhaul.Common.GlobalNPCs
{
	// The mod's first GlobalNPC. While an NPC has GenjutsuControlDebuff, its own AI (targeting,
	// attacks, pathing) never runs - PreAI returning false skips it entirely - and instead its
	// movement is puppeted directly from the casting player's own movement input each tick.
	// Movement only for now, per the user's explicit scope: no forcing the puppeted NPC to deal
	// damage to other NPCs (would need a custom NPC-vs-NPC collision check) and it can't attack
	// the player either, since its AI is fully skipped while controlled.
	public class GenjutsuGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private const float PuppetSpeed = 4f;

		public int ControllingPlayerIndex = -1;

		public override bool PreAI(NPC npc)
		{
			if (!npc.HasBuff(ModContent.BuffType<GenjutsuControlDebuff>()))
			{
				ControllingPlayerIndex = -1;
				return true;
			}

			if (ControllingPlayerIndex < 0 || !Main.player[ControllingPlayerIndex].active || Main.player[ControllingPlayerIndex].dead)
			{
				return true; // debuff present but no valid controller - let normal AI run this tick
			}

			Player controller = Main.player[ControllingPlayerIndex];
			var direction = new Vector2(
				(controller.controlRight ? 1 : 0) - (controller.controlLeft ? 1 : 0),
				(controller.controlDown ? 1 : 0) - (controller.controlUp ? 1 : 0));

			npc.velocity = direction.SafeNormalize(Vector2.Zero) * PuppetSpeed;
			npc.position += npc.velocity;

			if (npc.velocity.X != 0)
			{
				npc.spriteDirection = npc.velocity.X > 0 ? 1 : -1;
			}

			return false;
		}
	}
}
