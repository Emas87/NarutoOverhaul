# NarutoOverhaul — Roadmap

Produced 2026-07-15 from a full code audit (correctness pass + content census). This file is the
working backlog for the mod: bugs to fix, mechanics still missing, and a suggested order to tackle
them. Check items off as batches land. Every implementation batch still goes through the standard
`tools/build.sh` + `tools/smoke_test_server.sh` cycle before committing.

---

## 1. Bug fixes

### 1a. Netcode (multiplayer) — one root cause, many symptoms

The mod currently has **zero netcode**: no `SyncPlayer`, `NetSend`/`NetReceive`, `ModPacket`,
`SendExtraAI`, or `netUpdate` anywhere. Single-player is unaffected. Multiplayer breaks in layers,
worst first. One focused netcode pass closes almost all of it.

- [ ] **Story flags never reach clients** (progression-breaking). `Common/Systems/StoryProgressSystem.cs`'s
  7 static `Downed*` flags are set in server-side `OnKill` and never sent to clients — on any
  non-host client, every jutsu `CanUseItem`, every transformation `IsUnlocked`, all 3 class-vendor
  shop conditions, the Sharingan dodge, and Kaguya's summon gate stay locked forever.
  *Fix:* `NetSend`/`NetReceive` on the ModSystem (a `BitsByte` of the 7 flags) for join-time sync,
  plus a push (e.g. `NetMessage.SendData(MessageID.WorldData)`) from each boss `OnKill`.
- [ ] **Genjutsu control is dead in MP** (class-breaking). `GenjutsuGlobalNPC.ControllingPlayerIndex`
  is set client-side in `GenjutsuIllusionProjectile.OnHitNPC` (line ~46), but NPC AI authority is
  the server, where the field stays `-1` — puppet and flee modes silently do nothing (only sleep
  works, since it ignores the index).
  *Fix:* set the field on the server path and sync via `GlobalNPC.SendExtraAI`/`ReceiveExtraAI` +
  `npc.netUpdate = true`.
- [ ] **Boss AI spawns projectiles on every client.** All 7 bosses call `Projectile.NewProjectile`
  inside `AI()` with no `if (Main.netMode != NetmodeID.MultiplayerClient)` guard → phantom
  duplicate hostile projectiles on clients. 8 call sites: `HakuBoss.cs:169`,
  `OrochimaruBoss.cs:200`, `KakuzuBoss.cs:143`, `MadaraBoss.cs:166`, `PainBoss.cs:250,291`,
  `KaguyaBoss.cs:205,247`.
- [ ] **Per-player flags never reach the server.** `MoonLordBlessingPlayer.HasKaguyaBlessing` has no
  `SyncPlayer` → the server never learns a returning character's blessing → the Moon Lord weaken
  (`MoonLordWeakenGlobalNPC`, server-side `OnSpawn`) never fires in MP. Same gap:
  `TransformationPlayer.ActiveFormIndex`/`EightGatesLevel` (other players' transformations are
  invisible to everyone) and Chakra/Stamina (Pain's Preta chakra-drain, `PainBoss.cs:313`, hits a
  phantom server-side copy of the resource).
- [ ] **Cosmetic desyncs.** Orochimaru/Kaguya `OnSpawn` scaling changes `lifeMax` server-side only →
  client HP bars read >100% when the soft difficulty scaling is active. Madara's Susanoo
  transition (`MadaraBoss.cs:130-137`) changes `NPC.scale` unsynced → small Madara on clients.
  `ElementalBoltProjectile.BoltElement` is assigned after `NewProjectile` without `netUpdate` →
  every bolt looks like Fire on clients (and Lightning's homing branch never runs there).
  *Fix:* pass the element through the `ai0` spawn parameter; sync lifeMax/scale via `SendExtraAI`.

### 1b. Save-data edge case (single-player too)

- [ ] **Scroll bonus can be silently lost.** `ChakraPlayer`/`StaminaPlayer.LoadData`: if
  `baseMaxChakra` is missing/≤0 while `consumedChakraScrolls > 0` (legacy or partially corrupt
  save), `BaseMax` resets to 100 but the counter stays advanced — the +20/scroll bonus is gone
  forever, and the sequential gate (`CanUseItem` requires `counter == N-1`) blocks re-consuming.
  *Fix:* on reset, reconstruct `BaseMax = 100 + counter × 20` instead of a flat 100.

### 1c. Unobtainable content (quick wins — shop/recipe wiring only)

- [ ] **`ShadowCloneItem`** — Naruto's signature jutsu, fully implemented, obtainable nowhere.
  → Kakashi's shop, ungated (its own design comment says it's meant to be pre-boss).
- [ ] **All 4 resource potions** (`ChakraPotionItem`, `ChakraRegenPotionItem`, `StaminaPotionItem`,
  `StaminaRegenPotionItem`) — no recipe, no shop, no drop, despite being the natural counter to
  running dry mid-fight. → Tenten's shop (or cheap recipes; shop is simpler).
- [ ] **3 orphaned accessories**: `NinjutsuFocusSealItem` → Kakashi, `TaijutsuWrapsItem` → Guy,
  `GenjutsuVeilItem` → Itachi. Each class currently has only 1 obtainable accessory.
- [ ] **`GenjutsuVeilItem` duplicates `IllusionCharmItem`** (both +10% dmg / +1s debuff duration) —
  differentiate the Veil (e.g. reduced aggro or chakra-cost reduction) when adding it to the shop.
- [ ] **Dead localization keys** `ChakraScrollItem`/`StaminaScrollItem` in
  `Localization/en-US_Mods.NarutoOverhaul.hjson` — leftovers from the numbered-scroll rename.

### Verified non-issues (checked during audit; don't re-investigate)

- Numbered scrolls can't be double-consumed: `CanUseItem` re-checks `counter == N-1` each use.
- Eight Gates' Gate-8 `KillMe` is correctly owner-local (armed only via `ProcessTriggers`).
- `NPC.value = Item.buyPrice(...)` on bosses: `buyPrice` and `sellPrice` compute the same copper
  value — a balance choice, not a unit bug.
- `Main.rand` in projectile `AI()` is dust-only (cosmetic), acceptable for MP.

---

## 2. Missing mechanics

Ranked by impact. Each entry has a sketch that reuses established patterns in this repo.

- [ ] **Endgame weapons (biggest gameplay hole).** No weapon unlocks past boss 4 (Kakuzu); damage
  tops out at 45. Pain, Madara, and Kaguya — the whole Hardmode back half — award no weapons. The
  marquee absences map exactly onto the late-boss materials that currently only feed summon items:
  - *Chidori* — Ninjutsu, `DownedPain` gate, crafted from `RinneganFragmentItem` (same item shape
    as `RasenganItem`; short-range lightning burst, Kakashi shop or recipe).
  - *Susanoo* — Genjutsu showpiece, `DownedMadara` gate, crafted from `SusanooCoreItem` (currently
    summon-material-only) — e.g. a temporary damage-absorbing avatar buff, giving Genjutsu its
    missing late-game identity.
  - *Rinnegan ability* — Genjutsu/Ninjutsu utility from `RinneganFragmentItem` (e.g. Shinra Tensei
    knockback burst).
  - *Ōtsutsuki-tier weapon* — post-Kaguya capstone, crafted from her drop.
  - Plus one Hardmode-tier Taijutsu weapon (Taijutsu currently ends at boss 2 unlocks).
- [ ] **Localization pass.** 100% of player-facing text is auto-generated: every DisplayName ends in
  "Item/Boss/Buff/Projectile" (`Chakra Scroll1 Item`, `Shinobi Vendor N P C`), every tooltip is
  `""`, every buff description renders as a raw key, keybind names show double spaces. One
  hjson-editing pass fixes all of it; no code changes.
- [ ] **Class armor sets.** No armor exists at all. One 3-piece set per class (Taijutsu/Ninjutsu/
  Genjutsu) with set bonuses tied to the class resource (e.g. Ninjutsu set: +max Chakra, set bonus
  reduces jutsu chakra costs). Standard `ModItem` armor + `IsArmorSet`/`UpdateArmorSet`.
- [ ] **Keybind onboarding.** 5 keybinds exist (`.` `/` `,` `[` `]`) but nothing in-game reveals
  them. Cheapest fix: mention the keybind in each unlock's item tooltip and in vendor chat lines;
  optionally a "Shinobi Handbook" starter item whose tooltip lists all keybinds.
- [ ] **Expert/Master support.** No boss bags, trophies, relics, or boss music. Boss bags first
  (`ItemDropRule` + `BossBag` items), trophies/relics after; music is optional/art-dependent.
- [ ] **Genjutsu intra-class progression.** All 3 Genjutsu weapons unlock at boss 1 with 6-8 damage.
  Spread the unlocks across bosses and add the Susanoo/Rinnegan items above so the class grows.
- [ ] **Mobility endgame.** No wings equivalent. "Chakra Wings" (Six Paths-themed, post-Madara,
  standard `wings` accessory stats) fits both canon and the vanilla progression expectation.
- [ ] **Smaller items:** ModConfig (toggle HUD positions, drain rates); Ramen healing food (potion-
  sickness food buff, sold by Tenten); headband vanity items per village; shuriken thrown-weapon
  variety; recipe-discoverability hints (vendor chat pointing at the anvil recipes).

---

## 3. Suggested batch order

1. **Unobtainable-content quick wins + localization pass** — pure value, zero gameplay risk, makes
   everything already built actually reachable and readable (1c + localization from section 2).
2. **Endgame weapons for bosses 5-7** — fills the biggest gameplay hole; the materials and gates
   already exist, so it's mostly new `ModItem`s following `RasenganItem`'s shape.
3. **Class armor sets** — the next-largest expectation gap; establishes the armor pattern for
   future tiers.
4. **Netcode pass (1a)** — big and focused; MP-only so it doesn't block single-player content work,
   but should land before anyone actually hosts a server. Needs two real clients to verify, so the
   standing playtest deferral applies doubly here.
5. **Boss polish** — bags/trophies/relics (Expert/Master incentive layer).
6. **Onboarding + ModConfig + small items** — polish once the content is in place.

---

## 4. Known-unverified (first-playtest checklist)

Things that compile and load cleanly but have never been seen running (a dedicated server can't
render or simulate them). Check these first when real playtesting starts:

1. **Shadow Clone player-appearance rendering** (`ShadowCloneProjectile.PreDraw` +
   `Main.PlayerRenderer.DrawPlayer` with a dummy `Player`) — the single most likely thing in the
   mod to need iteration; falls back to a placeholder texture if the dummy is null.
2. **Chakra Control wall-climb** (`ChakraControlForm.PreUpdateMovement`) — bespoke velocity/gravity
   override with no vanilla hook to lean on; feel and hook-ordering both unproven.
3. **Moon Lord weaken** (`MoonLordWeakenGlobalNPC`) — needs a real save at endgame progression.
4. **Soft boss difficulty scaling** (Orochimaru pre-Skeletron, Kaguya pre-Golem) — does +50%/+30%/
   +20% actually feel like a "nudge" rather than a wall?
5. **Genjutsu puppet/sleep/flee** on varied vanilla AI styles — only ever exercised in theory.
6. **Eight Gates full ladder** — gates 1-7 drain feel, and the Gate-8 windup death firing correctly.
