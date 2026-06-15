[![GitHub Downloads](https://img.shields.io/github/downloads/Bjorgeh/cs2-retakes/total.svg?style=flat-square&label=Downloads)](https://github.com/Bjorgeh/cs2-retakes/releases/latest)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Bjorgeh/cs2-retakes/plugin-build.yml?branch=master&style=flat-square&label=Latest%20Build)

# CS2 Retakes
CS2 retakes plugin written in C# for CounterStrikeSharp. Forked from [B3none/cs2-retakes](https://github.com/B3none/cs2-retakes) with custom features added.

## Custom features (this fork)
- **Ranking system** — Players earn points for kills, deaths, assists, plants, defuses and round wins/losses. Rank abbreviation shown as clan tag (e.g. `[GN1]`) and rank icon shown in scoreboard.
- **Admin indicator** — Admins with `@css/root` get a `★` suffix in their clan tag (e.g. `[GN1] ★`).
- **Insta-defuse** — If all Terrorists die while the bomb is planted, the round ends immediately as a CT win (BombDefused) instead of waiting for the timer.
- **Queue improvements** — Players who try to join during warmup are placed in queue and promoted automatically when a slot opens. A custom message is shown when a player is already in the queue.
- **Stats persistence** — Kill/death/assist/plant/defuse stats are saved to a local SQLite database and restored on reconnect.

## Features
- [x] Bombsite selection
- [x] Per map configurations
- [x] Ability to add spawns
- [x] Spawn system
- [x] Temporary weapon allocation (hard coded)
- [x] Temporary grenade allocation (hard coded)
- [x] Equipment allocation
- [x] Queue manager (Queue system)
- [x] Team manager (with team switch calculations)
- [x] Retakes config file
- [x] Add translations
- [x] Improve bombsite announcement
- [x] Queue priority for VIPs
- [x] Add autoplant
- [x] Add a command to view the spawns for the current bombsite
- [x] Add a command to delete the nearest spawn
- [x] Implement better spawn management system
- [x] Ranking system with persistent stats
- [x] Insta-defuse on last T death with bomb planted

## Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
2. Download the plugin from the [releases section](https://github.com/Bjorgeh/cs2-retakes/releases/latest):
   - **RetakesPlugin-[version].zip** - Includes pre-configured map spawns (recommended for new installations)
   - **RetakesPlugin-[version]-no-map-configs.zip** - Without map configurations (for custom setups)
3. Unzip the archive and upload it to the game server into your `addons/counterstrikesharp/` directory.
4. Start the server and wait for the config.json file to be generated in `addons/counterstrikesharp/configs/plugins/RetakesPlugin`.
5. Complete the configuration file with the parameters of your choice.

## Allocators
Although this plugin comes with its own weapon allocation system, you can also use one of the following plugins for a more customisable experience:
- Yoni's Allocator: https://github.com/yonilerner/cs2-retakes-allocator
- NokkviReyr's Allocator: https://github.com/nokkvireyr/kps-allocator
- Ravid's Allocator: https://github.com/Ravid-A/cs2-retakes-weapon-allocator

## Configuration
When the plugin is first loaded it will create a `retakes_config.json` file in the plugin directory. This file contains all of the configuration options for the plugin:

### GameSettings
| Config                    | Description                                                                                                                             | Default | Min   | Max   |
|---------------------------|-----------------------------------------------------------------------------------------------------------------------------------------|---------|-------|-------|
| MaxPlayers                | The maximum number of players allowed in the game at any time. (If you want to increase the max capability you need to add more spawns) | 9       | 2     | 10    |
| ShouldBreakBreakables     | Whether to break all breakable props on round start (People are noticing rare crashes when this is enabled).                            | false   | false | true  |
| ShouldOpenDoors           | Whether to open doors on round start (People are noticing rare crashes when this is enabled).                                           | false   | false | true  |
| EnableFallbackAllocation  | Whether to enable the fallback weapon allocation. You should set this value to false if you're using a standalone weapon allocator.     | true    | false | true  |

### QueueSettings
| Config                 | Description                                                                                                   | Default  | Min | Max |
|------------------------|---------------------------------------------------------------------------------------------------------------|----------|-----|-----|
| QueuePriorityFlag      | A list of priority flag configurations. Each entry contains DisplayName, Flag, and Priority. Players with higher priority can replace players with lower priority in the queue. | `[{"DisplayName": "VIP", "Flag": "@css/vip", "Priority": 0}]` | 0 | 100 |
| QueueImmunityFlag      | A list of immunity flag configurations. Each entry contains DisplayName, Flag, and Priority. Players with immunity priority cannot be replaced by players with equal or lower priority. | `[{"DisplayName": "VIP", "Flag": "@css/vip", "Priority": 0}]` | 0 | 100 |
| ShouldRemoveSpectators | When a player is moved to spectators, remove them from all retake queues. Ensures that AFK plugins work as expected. | true     | false | true |

**QueuePriorityFlag and QueueImmunityFlag Configuration:**
Each flag configuration object has the following properties:
- **DisplayName**: The display name shown in messages (e.g., "VIP", "VIP Plus")
- **Flag**: The CSS permission flag (e.g., "@css/vip", "@css/vipplus")
- **Priority**: The priority value (higher numbers = higher priority). Valid range: 0-100

**Example Configuration:**
```json
{
  "QueuePriorityFlag": [
    {"DisplayName": "VIP", "Flag": "@css/vip", "Priority": 0},
    {"DisplayName": "VIP Plus", "Flag": "@css/vipplus", "Priority": 100}
  ],
  "QueueImmunityFlag": [
    {"DisplayName": "VIP", "Flag": "@css/vip", "Priority": 0},
    {"DisplayName": "VIP Plus", "Flag": "@css/vipplus", "Priority": 100}
  ]
}
```

**Example: Disabling Slot Priority and Immunity:**
```json
{
  "QueuePriorityFlag": [],
  "QueueImmunityFlag": []
}
```

### TeamSettings
| Config                                            | Description                                                                                                                                     | Default | Min   | Max   |
|---------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------|---------|-------|-------|
| TerroristRatio                                    | The percentage of the total players that should be Terrorists.                                                                                  | 0.45    | 0     | 1     |
| RoundsToScramble                                  | The number of rounds won in a row before the teams are scrambled.                                                                               | 5       | -1    | 99999 |
| IsScrambleEnabled                                 | Whether to scramble the teams once the RoundsToScramble value is met.                                                                           | true    | false | true  |
| IsBalanceEnabled                                  | Whether to enable the default team balancing mechanic.                                                                                          | true    | false | true  |
| ShouldForceEvenTeamsWhenPlayerCountIsMultipleOf10 | Whether to force even teams when the active players is a multiple of 10 or not. (this means you will get 5v5 @ 10 players / 10v10 @ 20 players) | true    | false | true  |
| ShouldPreventTeamChangesMidRound                  | Whether or not to prevent players from switching teams at any point during the round.                                                           | true    | false | true  |

### MapConfigSettings
| Config                             | Description                                                                                     | Default | Min   | Max  |
|------------------------------------|-------------------------------------------------------------------------------------------------|---------|-------|------|
| EnableBombsiteAnnouncementVoices   | Whether to play the bombsite announcement voices.                                               | false   | false | true |
| EnableBombsiteAnnouncementCenter   | Whether to display the bombsite in the center announcement box.                                 | true    | false | true |
| EnableFallbackBombsiteAnnouncement | Whether to enable the fallback bombsite announcement.                                           | true    | false | true |

### BombSettings
| Config             | Description                                                         | Default | Min   | Max  |
|--------------------|---------------------------------------------------------------------|---------|-------|------|
| IsAutoPlantEnabled | Whether to enable auto bomb planting at the start of the round or not. | true    | false | true |

### DebugSettings
| Config      | Description                                                 | Default | Min   | Max  |
|-------------|-------------------------------------------------------------|---------|-------|------|
| IsDebugMode | Whether to enable debug output to the server console or not. | false   | false | true |

## Commands

### General Commands
| Command            | Arguments                         | Description                                                          | Permissions |
|--------------------|-----------------------------------|----------------------------------------------------------------------|-------------|
| !forcebombsite     | <A / B>                           | Force the retakes to occur from a single bombsite.                   | @css/root   |
| !forcebombsitestop |                                   | Clear the forced bombsite and return back to normal.                 | @css/root   |
| !scramble          |                                   | Scrambles the teams next round.                                      | @css/admin  |
| !scrambleteams     |                                   | Scrambles the teams next round (alias).                              | @css/admin  |
| !voices            |                                   | Toggles whether or not to hear the bombsite voice announcements.     |             |
| css_debugqueues    |                                   | **SERVER ONLY** Shows the current queue state in the server console. |             |

### Spawn Editor Commands
| Command            | Arguments                         | Description                                                          | Permissions |
|--------------------|-----------------------------------|----------------------------------------------------------------------|-------------|
| !showspawns        | <A / B>                           | Show the spawns for the specified bombsite.                          | @css/root   |
| !spawns            | <A / B>                           | Show the spawns for the specified bombsite (alias).                  | @css/root   |
| !edit              | <A / B>                           | Show the spawns for the specified bombsite (alias).                  | @css/root   |
| !addspawn          | <CT / T> <Y / N (can be planter)> | Adds a retakes spawn point for the bombsite spawns currently shown.  | @css/root   |
| !add               | <CT / T> <Y / N (can be planter)> | Adds a retakes spawn point (alias).                                  | @css/root   |
| !newspawn          | <CT / T> <Y / N (can be planter)> | Adds a retakes spawn point (alias).                                  | @css/root   |
| !new               | <CT / T> <Y / N (can be planter)> | Adds a retakes spawn point (alias).                                  | @css/root   |
| !removespawn       |                                   | Removes the nearest spawn point for the bombsite currently shown.    | @css/root   |
| !remove            |                                   | Removes the nearest spawn point (alias).                             | @css/root   |
| !deletespawn       |                                   | Removes the nearest spawn point (alias).                             | @css/root   |
| !delete            |                                   | Removes the nearest spawn point (alias).                             | @css/root   |
| !nearestspawn      |                                   | Teleports the player to the nearest spawn.                           | @css/root   |
| !nearest           |                                   | Teleports the player to the nearest spawn (alias).                   | @css/root   |
| !hidespawns        |                                   | Exits the spawn editing mode.                                        | @css/root   |
| !done              |                                   | Exits the spawn editing mode (alias).                                | @css/root   |
| !exitedit          |                                   | Exits the spawn editing mode (alias).                                | @css/root   |

### Map Config Commands
| Command            | Arguments          | Description                                    | Permissions |
|--------------------|--------------------|------------------------------------------------|-------------|
| !mapconfig         | <Config file name> | Forces a specific map config file to load.     | @css/root   |
| !setmapconfig      | <Config file name> | Forces a specific map config file to load (alias). | @css/root   |
| !loadmapconfig     | <Config file name> | Forces a specific map config file to load (alias). | @css/root   |
| !mapconfigs        |                    | Displays a list of available map configs.     | @css/root   |
| !viewmapconfigs    |                    | Displays a list of available map configs (alias). | @css/root   |
| !listmapconfigs    |                    | Displays a list of available map configs (alias). | @css/root   |

## Credits
Originally based on [B3none/cs2-retakes](https://github.com/B3none/cs2-retakes), which was inspired by the [CS:GO Retakes project](https://github.com/splewis/csgo-retakes) by [splewis](https://github.com/splewis).
