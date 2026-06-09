using Microsoft.Data.Sqlite;
using RetakesPlugin.Utils;

namespace RetakesPlugin.Guns;

public class GunsDatabase
{
    private readonly string _connectionString;

    public GunsDatabase(string moduleDirectory)
    {
        var dbPath = Path.Combine(moduleDirectory, "guns.db");
        _connectionString = $"Data Source={dbPath}";
        EnsureTable();
    }

    private void EnsureTable()
    {
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS PlayerWeapons (
                    SteamID      TEXT NOT NULL PRIMARY KEY,
                    PrimaryT     TEXT NOT NULL,
                    PrimaryCT    TEXT NOT NULL,
                    Secondary    TEXT NOT NULL,
                    UpdatedAt    TEXT NOT NULL
                )";
            cmd.ExecuteNonQuery();
            Logger.LogInfo("Retakes Guns", "Database table ensured");
        }
        catch (Exception ex)
        {
            Logger.LogException("Retakes Guns", ex);
        }
    }

    public (string? primaryT, string? primaryCT, string? secondary) Load(ulong steamId)
    {
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT PrimaryT, PrimaryCT, Secondary FROM PlayerWeapons WHERE SteamID = @id";
            cmd.Parameters.AddWithValue("@id", steamId.ToString());
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader.GetString(0), reader.GetString(1), reader.GetString(2));
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Retakes Guns", ex);
        }

        return (null, null, null);
    }

    public void Save(ulong steamId, string primaryT, string primaryCT, string secondary)
    {
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO PlayerWeapons (SteamID, PrimaryT, PrimaryCT, Secondary, UpdatedAt)
                VALUES (@id, @pt, @pct, @sec, @at)
                ON CONFLICT(SteamID) DO UPDATE SET
                    PrimaryT  = excluded.PrimaryT,
                    PrimaryCT = excluded.PrimaryCT,
                    Secondary = excluded.Secondary,
                    UpdatedAt = excluded.UpdatedAt";
            cmd.Parameters.AddWithValue("@id", steamId.ToString());
            cmd.Parameters.AddWithValue("@pt", primaryT);
            cmd.Parameters.AddWithValue("@pct", primaryCT);
            cmd.Parameters.AddWithValue("@sec", secondary);
            cmd.Parameters.AddWithValue("@at", DateTime.UtcNow.ToString("O"));
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Logger.LogException("Retakes Guns", ex);
        }
    }

    public void Delete(ulong steamId)
    {
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM PlayerWeapons WHERE SteamID = @id";
            cmd.Parameters.AddWithValue("@id", steamId.ToString());
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Logger.LogException("Retakes Guns", ex);
        }
    }
}
