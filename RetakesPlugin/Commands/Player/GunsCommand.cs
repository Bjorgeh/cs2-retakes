using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

using RetakesPlugin.Guns;
using RetakesPlugin.Utils;

namespace RetakesPlugin.Commands.Player;

/// <summary>
/// Handles !guns, !myguns and !resetguns chat commands.
/// </summary>
public class GunsCommand
{
    private readonly GunsManager _gunsManager;

    public GunsCommand(GunsManager gunsManager)
    {
        _gunsManager = gunsManager;
    }

    /// <summary>!guns — opens the weapon selection menu.</summary>
    public void OnGunsCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!PlayerHelper.IsValid(player))
        {
            commandInfo.ReplyToCommand(" [Retakes Guns] You must be a valid player to use this command.");
            return;
        }

        Logger.LogInfo("Retakes Guns", $"{player.PlayerName} opened the guns menu");
        GunsMenu.OpenMain(player, _gunsManager);
    }

    /// <summary>!myguns — prints the player's current selections to chat.</summary>
    public void OnMyGunsCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!PlayerHelper.IsValid(player))
        {
            commandInfo.ReplyToCommand(" [Retakes Guns] You must be a valid player to use this command.");
            return;
        }

        var pref = _gunsManager.GetPreference(player);
        string Disp(string key) => GunsManager.WeaponDisplayNames.TryGetValue(key, out var n) ? n : key;
        player.PrintToChat($" \x04[Retakes Guns]\x01 Your current loadout:");
        player.PrintToChat($"   T-Side Primary : \x06{Disp(pref.PrimaryT)}\x01");
        player.PrintToChat($"   CT-Side Primary: \x06{Disp(pref.PrimaryCT)}\x01");
        player.PrintToChat($"   T-Side Pistol  : \x06{Disp(pref.SecondaryT)}\x01");
        player.PrintToChat($"   CT-Side Pistol : \x06{Disp(pref.SecondaryCT)}\x01");
    }

    /// <summary>!resetguns — resets the player's selections to server defaults.</summary>
    public void OnResetGunsCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!PlayerHelper.IsValid(player))
        {
            commandInfo.ReplyToCommand(" [Retakes Guns] You must be a valid player to use this command.");
            return;
        }

        _gunsManager.ResetPreferences(player);
        player.PrintToChat($" \x04[Retakes Guns]\x01 Your weapon preferences have been \x02reset\x01 to server defaults.");
        Logger.LogInfo("Retakes Guns", $"{player.PlayerName} reset their gun preferences");
    }
}
