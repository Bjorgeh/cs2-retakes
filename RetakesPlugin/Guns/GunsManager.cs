using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using RetakesPlugin.Utils;

namespace RetakesPlugin.Guns;

/// <summary>In-memory weapon preference for a single player.</summary>
public record WeaponPreference(string PrimaryT, string PrimaryCT, string Secondary);

public class GunsManager
{
    public readonly GunsConfig Config;
    private readonly GunsDatabase? _database;
    private readonly Dictionary<ulong, WeaponPreference> _preferences = new();

    // ── Weapon catalogue ────────────────────────────────────────────────────

    public static readonly Dictionary<string, List<(string DisplayName, string WeaponKey)>> PrimaryWeapons_T = new()
    {
        ["Rifles"] =
        [
            ("AK-47",      "weapon_ak47"),
            ("Galil AR",   "weapon_galilar"),
            ("SG 553",     "weapon_sg556"),
        ],
        ["Snipers"] =
        [
            ("AWP",              "weapon_awp"),
            ("SSG 08",           "weapon_ssg08"),
            ("G3SG1 (Auto)",     "weapon_g3sg1"),
        ],
        ["SMGs"] =
        [
            ("MP7",      "weapon_mp7"),
            ("MP5-SD",   "weapon_mp5sd"),
            ("MAC-10",   "weapon_mac10"),
            ("UMP-45",   "weapon_ump45"),
            ("P90",      "weapon_p90"),
            ("PP-Bizon", "weapon_bizon"),
        ],
        ["Heavy"] =
        [
            ("Nova",      "weapon_nova"),
            ("XM1014",    "weapon_xm1014"),
            ("Sawed-Off", "weapon_sawedoff"),
            ("M249",      "weapon_m249"),
            ("Negev",     "weapon_negev"),
        ],
    };

    public static readonly Dictionary<string, List<(string DisplayName, string WeaponKey)>> PrimaryWeapons_CT = new()
    {
        ["Rifles"] =
        [
            ("M4A4",   "weapon_m4a1"),
            ("M4A1-S", "weapon_m4a1_silencer"),
            ("FAMAS",  "weapon_famas"),
            ("AUG",    "weapon_aug"),
        ],
        ["Snipers"] =
        [
            ("AWP",              "weapon_awp"),
            ("SSG 08",           "weapon_ssg08"),
            ("SCAR-20 (Auto)",   "weapon_scar20"),
        ],
        ["SMGs"] =
        [
            ("MP9",      "weapon_mp9"),
            ("MP5-SD",   "weapon_mp5sd"),
            ("P90",      "weapon_p90"),
            ("MAC-10",   "weapon_mac10"),
            ("UMP-45",   "weapon_ump45"),
            ("PP-Bizon", "weapon_bizon"),
        ],
        ["Heavy"] =
        [
            ("Nova",   "weapon_nova"),
            ("XM1014", "weapon_xm1014"),
            ("MAG-7",  "weapon_mag7"),
            ("M249",   "weapon_m249"),
            ("Negev",  "weapon_negev"),
        ],
    };

    public static readonly List<(string DisplayName, string WeaponKey)> SecondaryWeapons =
    [
        ("Glock-18",      "weapon_glock"),
        ("P2000",         "weapon_p2000"),
        ("USP-S",         "weapon_usp_silencer"),
        ("P250",          "weapon_p250"),
        ("Five-SeveN",    "weapon_fiveseven"),
        ("Tec-9",         "weapon_tec9"),
        ("CZ75-Auto",     "weapon_cz75a"),
        ("Desert Eagle",  "weapon_deagle"),
        ("R8 Revolver",   "weapon_revolver"),
        ("Dual Berettas", "weapon_elite"),
    ];

    // ── Constructor ─────────────────────────────────────────────────────────

    public GunsManager(GunsConfig config, string moduleDirectory)
    {
        Config = config;
        if (config.EnablePersistence)
        {
            GunsDatabase.InitializeNative(moduleDirectory);
            _database = new GunsDatabase(moduleDirectory);
        }
    }

    // ── Player lifecycle ─────────────────────────────────────────────────────

    public void LoadPlayer(CCSPlayerController player)
    {
        if (_database == null || player.IsBot) return;

        var steamId = player.SteamID;
        var (pt, pct, sec) = _database.Load(steamId);

        if (pt != null && pct != null && sec != null)
        {
            _preferences[steamId] = new WeaponPreference(pt, pct, sec);
            Logger.LogInfo("Retakes Guns", $"Loaded weapons for SteamID {steamId}: T={pt} CT={pct} Pistol={sec}");
        }
    }

    public void UnloadPlayer(CCSPlayerController player)
    {
        _preferences.Remove(player.SteamID);
    }

    // ── Preference access ────────────────────────────────────────────────────

    public WeaponPreference GetPreference(CCSPlayerController player)
    {
        if (_preferences.TryGetValue(player.SteamID, out var pref))
        {
            return pref;
        }

        // Return defaults
        return new WeaponPreference(Config.DefaultPrimary_T, Config.DefaultPrimary_CT, Config.DefaultSecondary);
    }

    /// <summary>Returns the primary weapon key for the player's current team,
    /// respecting the AllowAWP config. Falls back to default if restricted.</summary>
    public string GetPrimary(CCSPlayerController player)
    {
        var pref = GetPreference(player);
        var key = player.Team == CsTeam.Terrorist ? pref.PrimaryT : pref.PrimaryCT;

        if (!Config.AllowAWP && key == "weapon_awp")
        {
            key = player.Team == CsTeam.Terrorist ? Config.DefaultPrimary_T : Config.DefaultPrimary_CT;
            Logger.LogInfo("Retakes Guns", $"AWP blocked for {player.PlayerName}, falling back to {key}");
        }

        return key;
    }

    public string GetSecondary(CCSPlayerController player)
    {
        return GetPreference(player).Secondary;
    }

    // ── Preference mutation ──────────────────────────────────────────────────

    public void SetPrimaryT(CCSPlayerController player, string weaponKey)
    {
        var current = GetPreference(player);
        var updated = current with { PrimaryT = weaponKey };
        _preferences[player.SteamID] = updated;
        Persist(player, updated);
        Logger.LogInfo("Retakes Guns", $"Player {player.PlayerName} selected T-primary: {weaponKey}");
    }

    public void SetPrimaryCT(CCSPlayerController player, string weaponKey)
    {
        var current = GetPreference(player);
        var updated = current with { PrimaryCT = weaponKey };
        _preferences[player.SteamID] = updated;
        Persist(player, updated);
        Logger.LogInfo("Retakes Guns", $"Player {player.PlayerName} selected CT-primary: {weaponKey}");
    }

    public void SetSecondary(CCSPlayerController player, string weaponKey)
    {
        var current = GetPreference(player);
        var updated = current with { Secondary = weaponKey };
        _preferences[player.SteamID] = updated;
        Persist(player, updated);
        Logger.LogInfo("Retakes Guns", $"Player {player.PlayerName} selected secondary: {weaponKey}");
    }

    public void ResetPreferences(CCSPlayerController player)
    {
        _preferences.Remove(player.SteamID);
        _database?.Delete(player.SteamID);
        Logger.LogInfo("Retakes Guns", $"Reset preferences for {player.PlayerName}");
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void Persist(CCSPlayerController player, WeaponPreference pref)
    {
        if (_database == null || player.IsBot) return;
        _database.Save(player.SteamID, pref.PrimaryT, pref.PrimaryCT, pref.Secondary);
    }
}
