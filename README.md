# CS2 Retakes

CS2 retakes plugin written in C# for CounterStrikeSharp. Forked from [B3none/cs2-retakes](https://github.com/B3none/cs2-retakes) with custom features added.

## Custom features (this fork)

- **Ranking system** — Players earn points for kills, deaths, assists, plants, defuses, and round wins/losses. Rank abbreviation shown as clan tag (e.g. `[GN1]`) and rank icon shown in scoreboard.
- **Admin indicator** — Admins with `@css/root` get a `★` suffix in their clan tag (e.g. `[GN1] ★`).
- **Insta-defuse** — If all Terrorists die while the bomb is planted, the round ends immediately as a CT win instead of waiting for the timer.
- **Queue improvements** — Players who try to join during warmup are placed in queue and promoted automatically when a slot opens.
- **Stats persistence** — Kill/death/assist/plant/defuse stats are saved to a local SQLite database and restored on reconnect.

## Features

- [x] Bombsite selection
- [x] Per map configurations
- [x] Ability to add spawns
- [x] Spawn system
- [x] Temporary weapon allocation (hard coded)
- [x] Temporary grenade allocation (hard coded)
- [x] Equipment allocation
- [x] Queue manager
- [x] Team manager (with team switch calculations)
- [x] Retakes config file
- [x] Translations
- [x] Bombsite announcement
- [x] Queue priority for VIPs
- [x] Auto-plant
- [x] Spawn viewer / editor commands
- [x] Ranking system with persistent stats
- [x] Insta-defuse on last T death with bomb planted

## Installation

1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
2. Download the plugin from the [releases section](https://github.com/Bjorgeh/cs2-retakes/releases/latest):
   - **RetakesPlugin-[version].zip** — includes pre-configured map spawns (recommended)
   - **RetakesPlugin-[version]-no-map-configs.zip** — without map configs (for custom setups)
3. Unzip and upload to `addons/counterstrikesharp/` on the server.
4. Start the server — the config file is generated automatically at `addons/counterstrikesharp/configs/plugins/RetakesPlugin/retakes_config.json`.
5. Edit the config as needed and restart/reload the plugin.

## Building from source

Requires .NET 8.0 SDK.

```bash
git clone https://github.com/Bjorgeh/cs2-retakes
cd cs2-retakes
dotnet build RetakesPlugin/RetakesPlugin.csproj -c Release
```

Output lands in `RetakesPlugin/bin/Release/net8.0/`.

## Allocators

This plugin ships with its own weapon allocator. Alternatively, use one of these:

- [Yoni's Allocator](https://github.com/yonilerner/cs2-retakes-allocator)
- [NokkviReyr's Allocator](https://github.com/nokkvireyr/kps-allocator)
- [Ravid's Allocator](https://github.com/Ravid-A/cs2-retakes-weapon-allocator)

Set `EnableFallbackAllocation: false` in GameSettings when using a standalone allocator.

## Configuration

Generated at first load: `addons/counterstrikesharp/configs/plugins/RetakesPlugin/retakes_config.json`

### GameSettings

| Config | Description | Default |
|--------|-------------|---------|
| `MaxPlayers` | Max players in-game at once | `9` |
| `ShouldBreakBreakables` | Break props on round start | `false` |
| `ShouldOpenDoors` | Open doors on round start | `false` |
| `EnableFallbackAllocation` | Use built-in weapon allocator | `true` |

### QueueSettings

| Config | Description | Default |
|--------|-------------|---------|
| `QueuePriorityFlag` | Priority flags list (DisplayName, Flag, Priority 0–100) | `[{"DisplayName":"VIP","Flag":"@css/vip","Priority":0}]` |
| `QueueImmunityFlag` | Immunity flags list | `[{"DisplayName":"VIP","Flag":"@css/vip","Priority":0}]` |
| `ShouldRemoveSpectators` | Remove spectators from queue (for AFK plugins) | `true` |

**Priority example — VIP Plus outranks VIP:**
```json
{
  "QueuePriorityFlag": [
    {"DisplayName": "VIP",      "Flag": "@css/vip",    "Priority": 0},
    {"DisplayName": "VIP Plus", "Flag": "@css/vipplus", "Priority": 100}
  ],
  "QueueImmunityFlag": [
    {"DisplayName": "VIP",      "Flag": "@css/vip",    "Priority": 0},
    {"DisplayName": "VIP Plus", "Flag": "@css/vipplus", "Priority": 100}
  ]
}
```

**Disable priority/immunity entirely:**
```json
{ "QueuePriorityFlag": [], "QueueImmunityFlag": [] }
```

### TeamSettings

| Config | Description | Default |
|--------|-------------|---------|
| `TerroristRatio` | Fraction of players as T | `0.45` |
| `RoundsToScramble` | Consecutive wins before scramble | `5` |
| `IsScrambleEnabled` | Enable auto-scramble | `true` |
| `IsBalanceEnabled` | Enable team balancing | `true` |
| `ShouldForceEvenTeamsWhenPlayerCountIsMultipleOf10` | Force 5v5 at 10 players, 10v10 at 20, etc. | `true` |
| `ShouldPreventTeamChangesMidRound` | Block team switches during round | `true` |

### MapConfigSettings

| Config | Description | Default |
|--------|-------------|---------|
| `EnableBombsiteAnnouncementVoices` | Play bombsite voice lines | `false` |
| `EnableBombsiteAnnouncementCenter` | Show bombsite in center HUD | `true` |
| `EnableFallbackBombsiteAnnouncement` | Fallback announcement | `true` |

### BombSettings

| Config | Description | Default |
|--------|-------------|---------|
| `IsAutoPlantEnabled` | Auto-plant bomb at round start | `true` |

### DebugSettings

| Config | Description | Default |
|--------|-------------|---------|
| `IsDebugMode` | Verbose console output | `false` |

## Commands

### General

| Command | Arguments | Description | Permission |
|---------|-----------|-------------|------------|
| `!forcebombsite` | `<A \| B>` | Lock retakes to one bombsite | `@css/root` |
| `!forcebombsitestop` | | Clear forced bombsite | `@css/root` |
| `!scramble` | | Scramble teams next round | `@css/admin` |
| `!voices` | | Toggle bombsite voice announcements | — |
| `css_debugqueues` | | Print queue state to server console | server only |

### Spawn editor

| Command | Arguments | Description | Permission |
|---------|-----------|-------------|------------|
| `!showspawns` / `!spawns` / `!edit` | `<A \| B>` | Show spawns for a bombsite | `@css/root` |
| `!addspawn` / `!add` / `!newspawn` / `!new` | `<CT \| T> <Y \| N>` | Add spawn at current position (Y = can plant) | `@css/root` |
| `!removespawn` / `!remove` / `!deletespawn` / `!delete` | | Remove nearest spawn | `@css/root` |
| `!nearestspawn` / `!nearest` | | Teleport to nearest spawn | `@css/root` |
| `!hidespawns` / `!done` / `!exitedit` | | Exit spawn editor | `@css/root` |

### Map config

| Command | Arguments | Description | Permission |
|---------|-----------|-------------|------------|
| `!mapconfig` / `!setmapconfig` / `!loadmapconfig` | `<filename>` | Force-load a map config | `@css/root` |
| `!mapconfigs` / `!viewmapconfigs` / `!listmapconfigs` | | List available map configs | `@css/root` |

## Credits

Originally based on [B3none/cs2-retakes](https://github.com/B3none/cs2-retakes), which was inspired by [splewis/csgo-retakes](https://github.com/splewis/csgo-retakes).
