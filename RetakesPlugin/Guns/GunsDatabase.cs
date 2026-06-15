using Microsoft.Data.Sqlite;
using RetakesPlugin.Utils;

namespace RetakesPlugin.Guns;

public class GunsDatabase
{
    private readonly string _connectionString;

    public static void InitializeNative(string pluginDirectory)
        => SQLiteBootstrap.Initialize(pluginDirectory);

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

            // Fresh installs: create with all columns
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS PlayerWeapons (
                    SteamID     TEXT NOT NULL PRIMARY KEY,
                    PrimaryT    TEXT NOT NULL,
                    PrimaryCT   TEXT NOT NULL,
                    Secondary   TEXT NOT NULL DEFAULT '',
                    SecondaryT  TEXT NOT NULL DEFAULT '',
                    SecondaryCT TEXT NOT NULL DEFAULT '',
                    UpdatedAt   TEXT NOT NULL
                )";
            cmd.ExecuteNonQuery();

            // Existing installs: add new columns if missing
            foreach (var alter in new[]
            {
                "ALTER TABLE PlayerWeapons ADD COLUMN SecondaryT  TEXT NOT NULL DEFAULT ''",
                "ALTER TABLE PlayerWeapons ADD COLUMN SecondaryCT TEXT NOT NULL DEFAULT ''",
            })
            {
                try { cmd.CommandText = alter; cmd.ExecuteNonQuery(); }
                catch { /* column already exists */ }
            }

            // Migrate old single Secondary value into per-team columns
            cmd.CommandText = @"
                UPDATE PlayerWeapons
                   SET SecondaryT  = Secondary,
                       SecondaryCT = Secondary
                 WHERE Secondary != '' AND SecondaryT = ''";
            cmd.ExecuteNonQuery();

            Logger.LogInfo("Retakes Guns", "Database table ensured");
        }
        catch (Exception ex)
        {
            Logger.LogException("Retakes Guns", ex);
        }
    }

    public (string? primaryT, string? primaryCT, string? secondaryT, string? secondaryCT) Load(ulong steamId)
    {
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT PrimaryT, PrimaryCT, SecondaryT, SecondaryCT FROM PlayerWeapons WHERE SteamID = @id";
            cmd.Parameters.AddWithValue("@id", steamId.ToString());
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var secT  = reader.GetString(2);
                var secCT = reader.GetString(3);
                return (
                    reader.GetString(0),
                    reader.GetString(1),
                    secT.Length  > 0 ? secT  : null,
                    secCT.Length > 0 ? secCT : null
                );
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("Retakes Guns", ex);
        }

        return (null, null, null, null);
    }

    public void Save(ulong steamId, string primaryT, string primaryCT, string secondaryT, string secondaryCT)
    {
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO PlayerWeapons (SteamID, PrimaryT, PrimaryCT, SecondaryT, SecondaryCT, UpdatedAt)
                VALUES (@id, @pt, @pct, @st, @sct, @at)
                ON CONFLICT(SteamID) DO UPDATE SET
                    PrimaryT    = excluded.PrimaryT,
                    PrimaryCT   = excluded.PrimaryCT,
                    SecondaryT  = excluded.SecondaryT,
                    SecondaryCT = excluded.SecondaryCT,
                    UpdatedAt   = excluded.UpdatedAt";
            cmd.Parameters.AddWithValue("@id",  steamId.ToString());
            cmd.Parameters.AddWithValue("@pt",  primaryT);
            cmd.Parameters.AddWithValue("@pct", primaryCT);
            cmd.Parameters.AddWithValue("@st",  secondaryT);
            cmd.Parameters.AddWithValue("@sct", secondaryCT);
            cmd.Parameters.AddWithValue("@at",  DateTime.UtcNow.ToString("O"));
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
