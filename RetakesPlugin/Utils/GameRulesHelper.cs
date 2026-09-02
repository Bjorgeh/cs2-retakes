using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;

namespace RetakesPlugin.Utils;

public static class GameRulesHelper
{
    public static CCSGameRules? GetGameRulesOrNull()
    {
        var gameRulesEntities = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
        var gameRulesProxy = gameRulesEntities.FirstOrDefault();

        if (gameRulesProxy == null)
        {
            return null;
        }

        return gameRulesProxy.GameRules;
    }

    public static CCSGameRules? GetGameRules()
    {
        return GetGameRulesOrNull();
    }

    public static void RestartGame()
    {
        var gameRules = GetGameRules();
        if (gameRules == null)
        {
            Logger.LogWarning("GameRules", "Cannot restart game: game rules not available");
            return;
        }

        if (!gameRules.WarmupPeriod)
        {
            CheckRoundDone();
        }

        Server.ExecuteCommand("mp_restartgame 1");
    }

    public static void CheckRoundDone()
    {
        var gameRules = GetGameRulesOrNull();
        if (gameRules == null || gameRules.WarmupPeriod)
            return;

        var tHumanCount = PlayerHelper.GetPlayerCount(CsTeam.Terrorist);
        var ctHumanCount = PlayerHelper.GetPlayerCount(CsTeam.CounterTerrorist);

        if (tHumanCount == 0 || ctHumanCount == 0)
        {
            TerminateRound(RoundEndReason.TerroristsWin);
        }
    }

    public static void TerminateRound(RoundEndReason roundEndReason)
    {
        var gameRules = GetGameRules();
        if (gameRules == null)
        {
            Logger.LogWarning("GameRules", "Cannot terminate round: game rules not available");
            return;
        }

        try
        {
            gameRules.TerminateRound(0.1f, roundEndReason);
        }
        catch (Exception ex)
        {
            Logger.LogWarning("GameRules",
                $"TerminateRound threw ({ex.GetType().Name}: {ex.Message}), falling back to killing the losing team's players.");

            // Only kill the side that's supposed to lose this round - killing everyone
            // (including the side that just won, e.g. CTs after a defuse) is wrong.
            var losingTeam = GetLosingTeam(roundEndReason);
            var playersToKill = Utilities.GetPlayers()
                .Where(PlayerHelper.IsValid)
                .Where(player => player.PawnIsAlive)
                .Where(player => losingTeam == null || player.Team == losingTeam)
                .ToList();

            foreach (var player in playersToKill)
            {
                player.CommitSuicide(false, true);
            }
        }
    }

    // Returns the team whose alive players should die to force the round to end with the
    // given reason, or null if it's not win/loss-specific (e.g. a draw) and everyone should die.
    private static CsTeam? GetLosingTeam(RoundEndReason roundEndReason)
    {
        return roundEndReason switch
        {
            RoundEndReason.BombDefused => CsTeam.Terrorist,
            RoundEndReason.CTsWin => CsTeam.Terrorist,
            RoundEndReason.TerroristsWin => CsTeam.CounterTerrorist,
            RoundEndReason.TargetBombed => CsTeam.CounterTerrorist,
            _ => null,
        };
    }

    public static double GetDistanceBetweenVectors(Vector v1, Vector v2)
    {
        var dx = v1.X - v2.X;
        var dy = v1.Y - v2.Y;

        return Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
    }
}