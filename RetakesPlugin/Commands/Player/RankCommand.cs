using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

using RetakesPlugin.Stats;
using RetakesPlugin.Utils;

namespace RetakesPlugin.Commands.Player;

public class RankCommand
{
    private readonly StatsManager _statsManager;

    public RankCommand(StatsManager statsManager)
    {
        _statsManager = statsManager;
    }

    /// <summary>!rank — shows own rank and points.</summary>
    public void OnRankCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!PlayerHelper.IsValid(player)) return;

        var stats = _statsManager.GetStats(player);
        if (stats == null)
        {
            player.PrintToChat(" \x04[Retakes Rank]\x01 No stats found yet — play a round first.");
            return;
        }

        var rank    = RankSystem.GetRank(stats.RankPoints);
        var toNext  = RankSystem.PointsToNextRank(stats.RankPoints);
        var kd      = stats.Deaths > 0 ? (float)stats.Kills / stats.Deaths : stats.Kills;
        var winPct  = stats.RoundsPlayed > 0 ? (int)((float)stats.RoundsWon / stats.RoundsPlayed * 100) : 0;

        player.PrintToChat($" \x04[Retakes Rank]\x01 ── {player.PlayerName} ──");
        player.PrintToChat($"   Rank    : \x06{rank.Name}\x01 ({stats.RankPoints} pts){(toNext > 0 ? $" — \x01{toNext} to next" : " \x06MAX\x01")}");
        player.PrintToChat($"   K/D/A   : {stats.Kills} / {stats.Deaths} / {stats.Assists}  (KD {kd:F2})");
        player.PrintToChat($"   Rounds  : {stats.RoundsPlayed} played, {stats.RoundsWon} won ({winPct}%)");
        player.PrintToChat($"   Plants  : {stats.Plants}  |  Defuses: {stats.Defuses}");
    }

    /// <summary>!top — shows top 10 players by rank points.</summary>
    public void OnTopCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!PlayerHelper.IsValid(player)) return;

        var top = _statsManager.GetTopPlayers(10);
        if (top.Count == 0)
        {
            player.PrintToChat(" \x04[Retakes Rank]\x01 No stats recorded yet.");
            return;
        }

        player.PrintToChat(" \x04[Retakes Rank]\x01 ── Top Players ──");
        for (var i = 0; i < top.Count; i++)
        {
            var s    = top[i];
            var rank = RankSystem.GetRank(s.RankPoints);
            player.PrintToChat($"   \x06{i + 1}.\x01 {s.PlayerName}  [{rank.ShortName}]  {s.RankPoints} pts  K:{s.Kills} D:{s.Deaths}");
        }
    }
}
