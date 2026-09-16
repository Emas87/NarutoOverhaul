using Microsoft.Xna.Framework;
using NarutoOverhaul.Common.Systems;
using NarutoOverhaul.Common.VFX;
using Terraria;
using Terraria.ModLoader;
using NarutoOverhaul.Content.Projectiles.Bursts;

namespace NarutoOverhaul.Content.Minions
{
	// Kage Bunshin - unlike AnimalMinionProjectile (vanilla Summon class, buff-keepalive lifetime),
	// this is Ninjutsu-class and duration-based: Projectile.timeLeft is set once at spawn and
	// simply counts down, no owner buff needed to keep it alive.
	//
	// Visual: attempts to render using the owner's actual equipped appearance (Main.PlayerRenderer
	// + a dummy Player instance with armor/dye/hair/skin copied over) rather than a fixed sprite -
	// the riskier option the user explicitly chose. PreDraw falls back to returning true (drawing
	// the placeholder texture normally) if the dummy Player is ever null, so a problem with the
	// render hook degrades to a plain placeholder sprite instead of a crash.
	public class ShadowCloneProjectile : ModProjectile
	{
		private const float MoveSpeed = 6f;
		private const float AttackRange = 500f;
		public const int Lifetime = 900; // 15 seconds

		private Player visualClone;

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 42;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.DamageType = ModContent.GetInstance<NinjutsuDamageClass>();
			Projectile.timeLeft = Lifetime;
			Projectile.netImportant = true;
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];

			if (!owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}

			NPC target = FindTarget(owner);

			if (target != null)
			{
				Vector2 toTarget = target.Center - Projectile.Center;
				Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * MoveSpeed * 1.5f;
			}
			else
			{
				Vector2 idlePoint = owner.Center + new Vector2(-owner.direction * 50f, 0f);
				Vector2 toIdle = idlePoint - Projectile.Center;
				Projectile.velocity = toIdle.Length() > 30f
					? toIdle.SafeNormalize(Vector2.Zero) * MoveSpeed
					: Projectile.velocity * 0.9f;
			}

			Projectile.direction = Projectile.velocity.X < 0 ? -1 : 1;
			Projectile.spriteDirection = Projectile.direction;

			UpdateVisualClone(owner);

			if (Projectile.timeLeft <= 60 && Projectile.timeLeft % 10 == 0)
			{
				ChakraVFX.SpawnBurstEffect<SmokeBurstProjectile>(Projectile.Center, 0.5f);
			}
		}

		private NPC FindTarget(Player owner)
		{
			NPC closest = null;
			float closestDistance = AttackRange;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];

				if (!npc.CanBeChasedBy(Projectile, false))
				{
					continue;
				}

				float distance = Vector2.Distance(npc.Center, owner.Center);

				if (distance < closestDistance)
				{
					closest = npc;
					closestDistance = distance;
				}
			}

			return closest;
		}

		private void UpdateVisualClone(Player owner)
		{
			visualClone ??= new Player();

			visualClone.skinVariant = owner.skinVariant;
			visualClone.hair = owner.hair;
			visualClone.hairDye = owner.hairDye;
			visualClone.hairColor = owner.hairColor;
			visualClone.skinColor = owner.skinColor;
			visualClone.eyeColor = owner.eyeColor;
			visualClone.face = owner.face;

			System.Array.Copy(owner.armor, visualClone.armor, owner.armor.Length);
			System.Array.Copy(owner.dye, visualClone.dye, owner.dye.Length);

			// Player.head/body/legs (the draw-slot IDs PlayerDrawLayers indexes into
			// TextureAssets.Armor*[]) are normally computed from the armor array once per tick
			// inside the real Player.Update(). visualClone never runs that, so without this they
			// stay at their -1 default and the renderer throws mid-draw (silently, since it's a
			// mod hook) - which is why the clone was fully invisible instead of just unarmored.
			visualClone.head = visualClone.armor[0].headSlot;
			visualClone.body = visualClone.armor[1].bodySlot;
			visualClone.legs = visualClone.armor[2].legSlot;

			if (visualClone.armor[10].headSlot >= 0)
			{
				visualClone.head = visualClone.armor[10].headSlot;
			}

			if (visualClone.armor[11].bodySlot >= 0)
			{
				visualClone.body = visualClone.armor[11].bodySlot;
			}

			if (visualClone.armor[12].legSlot >= 0)
			{
				visualClone.legs = visualClone.armor[12].legSlot;
			}

			visualClone.position = Projectile.position;
			visualClone.width = Projectile.width;
			visualClone.height = Projectile.height;
			visualClone.direction = Projectile.direction;
			visualClone.velocity = Projectile.velocity;
			visualClone.PlayerFrame();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (visualClone == null)
			{
				return true;
			}

			// PlayerRenderer.DrawPlayer wants the player's raw top-left position, matching vanilla's
			// own call sites exactly (Main's normal player-draw loop passes player.position
			// untouched, with the width/height offset only in rotationOrigin below). Adding that
			// same offset into the position too - on top of passing it as rotationOrigin - double
			// counted it and shifted the clone down by its own height, which is why it rendered
			// below the player instead of right next to them.
			Main.PlayerRenderer.DrawPlayer(Main.Camera, visualClone, Projectile.position, 0f, new Vector2(visualClone.width / 2f, visualClone.height), 0f, 1f);

			return false;
		}

		public override void OnKill(int timeLeft)
		{
			ChakraVFX.SpawnBurstEffect<SmokeBurstProjectile>(Projectile.Center, 1.95f);
		}
	}
}
