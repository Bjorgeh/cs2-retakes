namespace RetakesPlugin.Stats;

/// <summary>
/// Maps rank points to a CS2 competitive rank (0–18).
/// Icons are displayed natively in the Tab scoreboard via
/// CCSPlayerController.CompetitiveRanking.
/// </summary>
public static class RankSystem
{
    // ── Point rewards ────────────────────────────────────────────────────────
    public const int PointsPerKill       =  2;
    public const int PointsPerAssist     =  1;
    public const int PointsPerDeath      = -1;
    public const int PointsPerPlant      =  3;
    public const int PointsPerDefuse     =  5;
    public const int PointsPerRoundWin   =  3;
    public const int PointsPerRoundLoss  = -1;
    public const int PointsStarting      = 50;
    public const int PointsMin           =  0;

    // ── Rank definitions (index = CS2 CompetitiveRanking value) ─────────────
    public static readonly RankInfo[] Ranks =
    [
        new(0,  "Unranked",                    "UR",   0),
        new(1,  "Silver I",                    "S1",   50),
        new(2,  "Silver II",                   "S2",   150),
        new(3,  "Silver III",                  "S3",   250),
        new(4,  "Silver IV",                   "S4",   375),
        new(5,  "Silver Elite",                "SE",   500),
        new(6,  "Silver Elite Master",         "SEM",  650),
        new(7,  "Gold Nova I",                 "GN1",  800),
        new(8,  "Gold Nova II",                "GN2",  1000),
        new(9,  "Gold Nova III",               "GN3",  1250),
        new(10, "Gold Nova Master",            "GNM",  1500),
        new(11, "Master Guardian I",           "MG1",  1800),
        new(12, "Master Guardian II",          "MG2",  2150),
        new(13, "Master Guardian Elite",       "MGE",  2550),
        new(14, "Distinguished Master Guard.", "DMG",  3000),
        new(15, "Legendary Eagle",             "LE",   3500),
        new(16, "Legendary Eagle Master",      "LEM",  4100),
        new(17, "Supreme Master First Class",  "SMFC", 4800),
        new(18, "Global Elite",                "GE",   5600),
    ];

    /// <summary>Returns the RankInfo for the given point total.</summary>
    public static RankInfo GetRank(int points)
    {
        RankInfo result = Ranks[0];
        foreach (var rank in Ranks)
        {
            if (points >= rank.MinPoints)
                result = rank;
            else
                break;
        }
        return result;
    }

    /// <summary>Points needed to reach the next rank (0 if already GE).</summary>
    public static int PointsToNextRank(int points)
    {
        var current = GetRank(points);
        // Use Array.IndexOf so this is safe even if Id values are ever reordered.
        var idx = Array.IndexOf(Ranks, current);
        if (idx < 0 || idx >= Ranks.Length - 1) return 0;
        return Ranks[idx + 1].MinPoints - points;
    }
}

public record RankInfo(int Id, string Name, string ShortName, int MinPoints);
