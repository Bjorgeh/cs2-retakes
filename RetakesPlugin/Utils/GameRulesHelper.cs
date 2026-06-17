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
        catch
        {
            Logger.LogWarning("GameRules",
                "Incorrect signature detected (Can't use TerminateRound), killing all alive players instead.");

            var alivePlayers = Utilities.GetPlayers()
                .Where(PlayerHelper.IsValid)
                .Where(player => player.PawnIsAlive)
                .ToList();

            foreach (var player in alivePlayers)
            {
                player.CommitSuicide(false, true);
            }
        }
    }

    public static double GetDistanceBetweenVectors(Vector v1, Vector v2)
    {
        var dx = v1.X - v2.X;
        var dy = v1.Y - v2.Y;

        return Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
    }
}