using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;
using RetakesPlugin.Utils;

namespace RetakesPlugin.Stats;

public class StatsManager
{
    private readonly StatsDatabase? _database;
    private readonly Dictionary<ulong, PlayerStats> _cache = new();

    public StatsManager(string moduleDirectory, bool enablePersistence)
    {
        if (enablePersistence)
        {
            StatsDatabase.InitializeNative(moduleDirectory);
            _database = new StatsDatabase(moduleDirectory);
        }
    }

    // ── Player lifecycle ──────────────────────────────────────────────────────

    public void LoadPlayer(CCSPlayerController player)
    {
        if (player.IsBot) return;

        var steamId = player.SteamID;
        var stored  = _database?.Load(steamId);

        var stats = stored ?? new PlayerStats(steamId, player.PlayerName,
            0, 0, 0, 0, 0, 0, 0, RankSystem.PointsStarting);

        _cache[steamId] = stats;
        ApplyScoreboard(player, stats);
        Logger.LogInfo("Retakes Stats", $"Loaded stats for {player.PlayerName} — {RankSystem.GetRank(stats.RankPoints).Name} ({stats.RankPoints} pts)");
    }

    public void UnloadPlayer(CCSPlayerController player)
    {
        if (_cache.TryGetValue(player.SteamID, out var stats))
        {
            var updated = stats with { PlayerName = player.PlayerName };
            _database?.Save(updated);
            _cache.Remove(player.SteamID);
        }
    }

    // ── Event recording (cache-only — DB write deferred to round end / disconnect) ──

    public void OnKill(CCSPlayerController attacker)
    {
        if (!TryGet(attacker, out var s)) return;
        UpdateCache(attacker, s with { Kills = s.Kills + 1, RankPoints = Clamp(s.RankPoints + RankSystem.PointsPerKill) });
    }

    public void OnDeath(CCSPlayerController victim)
    {
        if (!TryGet(victim, out var s)) return;
        UpdateCache(victim, s with { Deaths = s.Deaths + 1, RankPoints = Clamp(s.RankPoints + RankSystem.PointsPerDeath) });
    }

    public void OnAssist(CCSPlayerController assister)
    {
        if (!TryGet(assister, out var s)) return;
        UpdateCache(assister, s with { Assists = s.Assists + 1, RankPoints = Clamp(s.RankPoints + RankSystem.PointsPerAssist) });
    }

    public void OnPlant(CCSPlayerController planter)
    {
        if (!TryGet(planter, out var s)) return;
        UpdateCache(planter, s with { Plants = s.Plants + 1, RankPoints = Clamp(s.RankPoints + RankSystem.PointsPerPlant) });
    }

    public void OnDefuse(CCSPlayerController defuser)
    {
        if (!TryGet(defuser, out var s)) return;
        UpdateCache(defuser, s with { Defuses = s.Defuses + 1, RankPoints = Clamp(s.RankPoints + RankSystem.PointsPerDefuse) });
    }

    /// <summary>
    /// Call at round end with the winning team.
    /// Flushes all dirty stats to the database in one pass.
    /// </summary>
    public void OnRoundEnd(IReadOnlyCollection<CCSPlayerController> activePlayers, CsTeam winningTeam)
    {
        foreach (var player in activePlayers)
        {
            if (!PlayerHelper.IsValid(player) || player.IsBot) continue;
            if (!TryGet(player, out var s)) continue;

            var won   = player.Team == winningTeam;
            var delta = won ? RankSystem.PointsPerRoundWin : RankSystem.PointsPerRoundLoss;

            var updated = s with
            {
                RoundsPlayed = s.RoundsPlayed + 1,
                RoundsWon    = s.RoundsWon + (won ? 1 : 0),
                RankPoints   = Clamp(s.RankPoints + delta),
                PlayerName   = player.PlayerName,
            };

            var oldRank = RankSystem.GetRank(s.RankPoints);
            var newRank = RankSystem.GetRank(updated.RankPoints);

            _cache[player.SteamID] = updated;
            _database?.Save(updated);
            ApplyScoreboard(player, updated);

            if (newRank.Id > oldRank.Id)
                player.PrintToChat($" \x04[Retakes Rank]\x01 Rank up! \x06{newRank.Name}\x01 ({updated.RankPoints} pts)");
            else if (newRank.Id < oldRank.Id)
                player.PrintToChat($" \x04[Retakes Rank]\x01 Rank down. \x02{newRank.Name}\x01 ({updated.RankPoints} pts)");
        }
    }

    // ── Public queries ────────────────────────────────────────────────────────

    public PlayerStats? GetStats(CCSPlayerController player)
    {
        _cache.TryGetValue(player.SteamID, out var s);
        return s;
    }

    public List<PlayerStats> GetTopPlayers(int count = 10)
        => _database?.GetTopPlayers(count) ?? [];

    // ── Scoreboard ────────────────────────────────────────────────────────────

    private const string AdminSuffix = " [ADMIN]";

    public static void ApplyScoreboard(CCSPlayerController player, PlayerStats stats)
    {
        if (!PlayerHelper.IsValid(player)) return;

        var rank = RankSystem.GetRank(stats.RankPoints);
        player.Clan               = $"[{rank.ShortName}]";
        player.CompetitiveRanking = rank.Id;
        Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");

        // Strip any existing admin suffix first (idempotency), then re-apply if still admin.
        var baseName = player.PlayerName.EndsWith(AdminSuffix)
            ? player.PlayerName[..^AdminSuffix.Length]
            : player.PlayerName;

        var isAdmin  = AdminManager.PlayerHasPermissions(player, "@css/root");
        var newName  = isAdmin ? baseName + AdminSuffix : baseName;

        if (player.PlayerName != newName)
            player.PlayerName = newName;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool TryGet(CCSPlayerController player, out PlayerStats stats)
    {
        stats = default!;
        if (!PlayerHelper.IsValid(player) || player.IsBot) return false;
        return _cache.TryGetValue(player.SteamID, out stats!);
    }

    // Cache-only update; DB flush happens at round end and on disconnect.
    private void UpdateCache(CCSPlayerController player, PlayerStats updated)
    {
        _cache[player.SteamID] = updated;
        ApplyScoreboard(player, updated);
    }

    private static int Clamp(int value) => Math.Max(RankSystem.PointsMin, value);
}
