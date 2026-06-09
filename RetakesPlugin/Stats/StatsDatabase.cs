using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using RetakesPlugin.Utils;

namespace RetakesPlugin.Stats;

public record PlayerStats(
    ulong  SteamId,
    string PlayerName,
    int    Kills,
    int    Deaths,
    int    Assists,
    int    Plants,
    int    Defuses,
    int    RoundsPlayed,
    int    RoundsWon,
    int    RankPoints
);

public class StatsDatabase
{
    private readonly string _connectionString;

    // ── Native bootstrap (reuses GunsDatabase resolver if already set) ───────
    private static bool _nativeInitialized;
    private static readonly object _nativeLock = new();

    public static void InitializeNative(string pluginDirectory)
    {
        lock (_nativeLock)
        {
            if (_nativeInitialized) return;
            _nativeInitialized = true;
        }

        try
        {
            var providerAssembly = typeof(SQLitePCL.SQLite3Provider_e_sqlite3).Assembly;
            NativeLibrary.SetDllImportResolver(providerAssembly, (libraryName, _, _) =>
            {
                if (libraryName != "e_sqlite3") return IntPtr.Zero;

                string[] candidates =
                [
                    Path.Combine(pluginDirectory, "libe_sqlite3.so"),
                    Path.Combine(pluginDirectory, "runtimes", "linux-x64", "native", "libe_sqlite3.so"),
                ];

                foreach (var path in candidates)
                {
                    if (File.Exists(path))
                        return NativeLibrary.Load(path);
                }

                return IntPtr.Zero;
            });

            SQLitePCL.Batteries_V2.Init();
        }
        catch (InvalidOperationException)
        {
            // Resolver already registered by GunsDatabase – that's fine
        }
        catch (Exception ex)
        {
            Logger.LogException("Retakes Stats", ex);
        }
    }

    // ── Instance ──────────────────────────────────────────────────────────────

    public StatsDatabase(string moduleDirectory)
    {
        var dbPath = Path.Combine(moduleDirectory, "stats.db");
        _connectionString = $"Data Source={dbPath}";
        EnsureTable();
    }

    private void EnsureTable()
    {
        try
        {
            using var conn = Open();
            Exec(conn, @"
                CREATE TABLE IF NOT EXISTS PlayerStats (
                    SteamID      TEXT NOT NULL PRIMARY KEY,
                    PlayerName   TEXT NOT NULL,
                    Kills        INTEGER NOT NULL DEFAULT 0,
                    Deaths       INTEGER NOT NULL DEFAULT 0,
                    Assists      INTEGER NOT NULL DEFAULT 0,
                    Plants       INTEGER NOT NULL DEFAULT 0,
                    Defuses      INTEGER NOT NULL DEFAULT 0,
                    RoundsPlayed INTEGER NOT NULL DEFAULT 0,
                    RoundsWon    INTEGER NOT NULL DEFAULT 0,
                    RankPoints   INTEGER NOT NULL DEFAULT 50,
                    UpdatedAt    TEXT NOT NULL
                )");
        }
        catch (Exception ex)
        {
            Logger.LogException("Retakes Stats", ex);
        }
    }

    public PlayerStats? Load(ulong steamId)
    {
        try
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT PlayerName,Kills,Deaths,Assists,Plants,Defuses,RoundsPlayed,RoundsWon,RankPoints FROM PlayerStats WHERE SteamID=@id";
            cmd.Parameters.AddWithValue("@id", steamId.ToString());
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new PlayerStats(steamId, r.GetString(0), r.GetInt32(1), r.GetInt32(2),
                r.GetInt32(3), r.GetInt32(4), r.GetInt32(5), r.GetInt32(6), r.GetInt32(7), r.GetInt32(8));
        }
        catch (Exception ex) { Logger.LogException("Retakes Stats", ex); return null; }
    }

    public void Save(PlayerStats s)
    {
        try
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO PlayerStats
                    (SteamID,PlayerName,Kills,Deaths,Assists,Plants,Defuses,RoundsPlayed,RoundsWon,RankPoints,UpdatedAt)
                VALUES
                    (@id,@name,@k,@d,@a,@pl,@def,@rp,@rw,@pts,@at)
                ON CONFLICT(SteamID) DO UPDATE SET
                    PlayerName=excluded.PlayerName, Kills=excluded.Kills,
                    Deaths=excluded.Deaths, Assists=excluded.Assists,
                    Plants=excluded.Plants, Defuses=excluded.Defuses,
                    RoundsPlayed=excluded.RoundsPlayed, RoundsWon=excluded.RoundsWon,
                    RankPoints=excluded.RankPoints, UpdatedAt=excluded.UpdatedAt";
            cmd.Parameters.AddWithValue("@id",   s.SteamId.ToString());
            cmd.Parameters.AddWithValue("@name", s.PlayerName);
            cmd.Parameters.AddWithValue("@k",    s.Kills);
            cmd.Parameters.AddWithValue("@d",    s.Deaths);
            cmd.Parameters.AddWithValue("@a",    s.Assists);
            cmd.Parameters.AddWithValue("@pl",   s.Plants);
            cmd.Parameters.AddWithValue("@def",  s.Defuses);
            cmd.Parameters.AddWithValue("@rp",   s.RoundsPlayed);
            cmd.Parameters.AddWithValue("@rw",   s.RoundsWon);
            cmd.Parameters.AddWithValue("@pts",  s.RankPoints);
            cmd.Parameters.AddWithValue("@at",   DateTime.UtcNow.ToString("O"));
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { Logger.LogException("Retakes Stats", ex); }
    }

    /// <summary>Returns top N players ordered by RankPoints descending.</summary>
    public List<PlayerStats> GetTopPlayers(int count = 10)
    {
        var list = new List<PlayerStats>();
        try
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT SteamID,PlayerName,Kills,Deaths,Assists,Plants,Defuses,RoundsPlayed,RoundsWon,RankPoints FROM PlayerStats ORDER BY RankPoints DESC LIMIT @n";
            cmd.Parameters.AddWithValue("@n", count);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new PlayerStats(ulong.Parse(r.GetString(0)), r.GetString(1),
                    r.GetInt32(2), r.GetInt32(3), r.GetInt32(4), r.GetInt32(5),
                    r.GetInt32(6), r.GetInt32(7), r.GetInt32(8), r.GetInt32(9)));
        }
        catch (Exception ex) { Logger.LogException("Retakes Stats", ex); }
        return list;
    }

    private SqliteConnection Open()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    private static void Exec(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
