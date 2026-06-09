using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;

namespace RetakesPlugin.Guns;

/// <summary>
/// Builds and opens the multi-level ChatMenu for weapon selection.
/// No weapon entities are touched here — selections are stored in
/// GunsManager and applied by AllocationService at round post-start.
/// </summary>
public static class GunsMenu
{
    // ── Entry point ──────────────────────────────────────────────────────────

    public static void OpenMain(CCSPlayerController player, GunsManager gunsManager)
    {
        var current = gunsManager.GetPreference(player);

        var menu = new ChatMenu(" \x04[Retakes Guns]\x01 Weapon Selection");
        menu.AddMenuOption($"T-Side Primary  [{GetDisplayName(current.PrimaryT)}]",
            (p, _) => OpenPrimaryCategory(p, gunsManager, isTSide: true));
        menu.AddMenuOption($"CT-Side Primary [{GetDisplayName(current.PrimaryCT)}]",
            (p, _) => OpenPrimaryCategory(p, gunsManager, isTSide: false));
        menu.AddMenuOption($"Pistol          [{GetDisplayName(current.Secondary)}]",
            (p, _) => OpenSecondaryMenu(p, gunsManager));

        MenuManager.OpenChatMenu(player, menu);
    }

    // ── Primary category picker ──────────────────────────────────────────────

    private static void OpenPrimaryCategory(CCSPlayerController player, GunsManager gunsManager, bool isTSide)
    {
        var sideLabel = isTSide ? "T-Side" : "CT-Side";
        var catalogue = isTSide ? GunsManager.PrimaryWeapons_T : GunsManager.PrimaryWeapons_CT;

        var menu = new ChatMenu($" \x04[Retakes Guns]\x01 {sideLabel} Primary — Category");
        foreach (var category in catalogue.Keys)
        {
            var capturedCategory = category;
            menu.AddMenuOption(category,
                (p, _) => OpenPrimaryWeaponList(p, gunsManager, isTSide, capturedCategory));
        }

        MenuManager.OpenChatMenu(player, menu);
    }

    // ── Primary weapon list ──────────────────────────────────────────────────

    private static void OpenPrimaryWeaponList(
        CCSPlayerController player,
        GunsManager gunsManager,
        bool isTSide,
        string category)
    {
        var sideLabel = isTSide ? "T-Side" : "CT-Side";
        var catalogue = isTSide ? GunsManager.PrimaryWeapons_T : GunsManager.PrimaryWeapons_CT;

        if (!catalogue.TryGetValue(category, out var weapons))
        {
            return;
        }

        var menu = new ChatMenu($" \x04[Retakes Guns]\x01 {sideLabel} Primary — {category}");
        foreach (var (displayName, weaponKey) in weapons)
        {
            var capturedKey  = weaponKey;
            var capturedName = displayName;

            // Disable AWP option when server has it blocked
            var isAwp     = weaponKey == "weapon_awp";
            var disabled  = isAwp && !gunsManager.Config.AllowAWP;
            var label     = disabled ? $"{displayName} [disabled]" : displayName;

            menu.AddMenuOption(label, (p, _) =>
            {
                if (isTSide)
                {
                    gunsManager.SetPrimaryT(p, capturedKey);
                }
                else
                {
                    gunsManager.SetPrimaryCT(p, capturedKey);
                }

                p.PrintToChat($" \x04[Retakes Guns]\x01 {sideLabel} primary set to \x06{capturedName}\x01.");
            }, disabled);
        }

        MenuManager.OpenChatMenu(player, menu);
    }

    // ── Secondary / pistol menu ──────────────────────────────────────────────

    private static void OpenSecondaryMenu(CCSPlayerController player, GunsManager gunsManager)
    {
        var menu = new ChatMenu(" \x04[Retakes Guns]\x01 Pistol");
        foreach (var (displayName, weaponKey) in GunsManager.SecondaryWeapons)
        {
            var capturedKey  = weaponKey;
            var capturedName = displayName;
            menu.AddMenuOption(displayName, (p, _) =>
            {
                gunsManager.SetSecondary(p, capturedKey);
                p.PrintToChat($" \x04[Retakes Guns]\x01 Pistol set to \x06{capturedName}\x01.");
            });
        }

        MenuManager.OpenChatMenu(player, menu);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Returns a short human-readable name for a weapon key.</summary>
    private static string GetDisplayName(string weaponKey)
    {
        // Search all catalogues for a matching key
        foreach (var weapons in GunsManager.PrimaryWeapons_T.Values)
        {
            var match = weapons.FirstOrDefault(w => w.WeaponKey == weaponKey);
            if (match.WeaponKey != null) return match.DisplayName;
        }

        foreach (var weapons in GunsManager.PrimaryWeapons_CT.Values)
        {
            var match = weapons.FirstOrDefault(w => w.WeaponKey == weaponKey);
            if (match.WeaponKey != null) return match.DisplayName;
        }

        var sec = GunsManager.SecondaryWeapons.FirstOrDefault(w => w.WeaponKey == weaponKey);
        if (sec.WeaponKey != null) return sec.DisplayName;

        return weaponKey;
    }
}
