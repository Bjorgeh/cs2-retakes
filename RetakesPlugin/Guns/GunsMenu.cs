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
        menu.AddMenuOption($"T-Side Primary  [{DisplayName(current.PrimaryT)}]",
            (p, _) => OpenPrimaryCategory(p, gunsManager, isTSide: true));
        menu.AddMenuOption($"CT-Side Primary [{DisplayName(current.PrimaryCT)}]",
            (p, _) => OpenPrimaryCategory(p, gunsManager, isTSide: false));
        menu.AddMenuOption($"T-Side Pistol   [{DisplayName(current.SecondaryT)}]",
            (p, _) => OpenSecondaryMenu(p, gunsManager, isTSide: true));
        menu.AddMenuOption($"CT-Side Pistol  [{DisplayName(current.SecondaryCT)}]",
            (p, _) => OpenSecondaryMenu(p, gunsManager, isTSide: false));

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
            return;

        var menu = new ChatMenu($" \x04[Retakes Guns]\x01 {sideLabel} Primary — {category}");
        foreach (var (displayName, weaponKey) in weapons)
        {
            var capturedKey  = weaponKey;
            var capturedName = displayName;

            // Disable AWP option when server has it blocked
            var disabled = weaponKey == "weapon_awp" && !gunsManager.Config.AllowAWP;
            var label    = disabled ? $"{displayName} [disabled]" : displayName;

            menu.AddMenuOption(label, (p, _) =>
            {
                if (disabled) return;
                if (isTSide)
                    gunsManager.SetPrimaryT(p, capturedKey);
                else
                    gunsManager.SetPrimaryCT(p, capturedKey);

                p.PrintToChat($" \x04[Retakes Guns]\x01 {sideLabel} primary set to \x06{capturedName}\x01.");
            }, disabled);
        }

        MenuManager.OpenChatMenu(player, menu);
    }

    // ── Secondary / pistol menu ──────────────────────────────────────────────

    private static void OpenSecondaryMenu(CCSPlayerController player, GunsManager gunsManager, bool isTSide)
    {
        var sideLabel = isTSide ? "T-Side" : "CT-Side";
        var menu = new ChatMenu($" \x04[Retakes Guns]\x01 {sideLabel} Pistol");

        foreach (var (displayName, weaponKey) in GunsManager.SecondaryWeapons)
        {
            var capturedKey  = weaponKey;
            var capturedName = displayName;
            menu.AddMenuOption(displayName, (p, _) =>
            {
                if (isTSide)
                    gunsManager.SetSecondaryT(p, capturedKey);
                else
                    gunsManager.SetSecondaryCT(p, capturedKey);

                p.PrintToChat($" \x04[Retakes Guns]\x01 {sideLabel} pistol set to \x06{capturedName}\x01.");
            });
        }

        MenuManager.OpenChatMenu(player, menu);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string DisplayName(string weaponKey)
        => GunsManager.WeaponDisplayNames.TryGetValue(weaponKey, out var name) ? name : weaponKey;
}
