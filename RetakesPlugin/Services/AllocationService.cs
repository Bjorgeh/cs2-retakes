using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;

using RetakesPlugin.Guns;
using RetakesPlugin.Utils;

namespace RetakesPlugin.Services;

public class AllocationService
{
    private readonly Random _random;
    private readonly GunsManager? _gunsManager;

    public AllocationService(Random random, GunsManager? gunsManager = null)
    {
        _random = random;
        _gunsManager = gunsManager;
    }

    public void AllocatePlayer(CCSPlayerController player)
    {
        AllocateEquipment(player);
        AllocateWeapons(player);
        AllocateGrenades(player);
    }

    private void AllocateEquipment(CCSPlayerController player)
    {
        player.GiveNamedItem(CsItem.KevlarHelmet);

        if (
            player.Team == CsTeam.CounterTerrorist
            && player.PlayerPawn.IsValid
            && player.PlayerPawn.Value != null
            && player.PlayerPawn.Value.IsValid
            && player.PlayerPawn.Value.ItemServices != null
        )
        {
            var itemServices = new CCSPlayer_ItemServices(player.PlayerPawn.Value.ItemServices.Handle);
            itemServices.HasDefuser = true;
        }
    }

    private void AllocateWeapons(CCSPlayerController player)
    {
        if (_gunsManager != null)
        {
            var primary   = _gunsManager.GetPrimary(player);
            var secondary = _gunsManager.GetSecondary(player);

            player.GiveNamedItem(primary);
            player.GiveNamedItem(secondary);
            player.GiveNamedItem(CsItem.Knife);

            Logger.LogInfo("Retakes Guns", $"Applied custom loadout during Retakes spawn for {player.PlayerName}: {primary} / {secondary}");
            return;
        }

        // ── Fallback (no GunsManager configured) ────────────────────────────
        if (player.Team == CsTeam.Terrorist)
        {
            player.GiveNamedItem(CsItem.AK47);
            player.GiveNamedItem(CsItem.Deagle);
        }

        if (player.Team == CsTeam.CounterTerrorist)
        {
            // Easter egg for klippy
            if (player.PlayerName.Trim() == "klip")
            {
                player.GiveNamedItem(CsItem.M4A4);
            }
            else
            {
                player.GiveNamedItem(CsItem.M4A1S);
            }

            player.GiveNamedItem(CsItem.Deagle);
        }

        player.GiveNamedItem(CsItem.Knife);
    }

    private void AllocateGrenades(CCSPlayerController player)
    {
        switch (_random.Next(4))
        {
            case 0:
                player.GiveNamedItem(CsItem.SmokeGrenade);
                break;
            case 1:
                player.GiveNamedItem(CsItem.Flashbang);
                break;
            case 2:
                player.GiveNamedItem(CsItem.HEGrenade);
                break;
            case 3:
                player.GiveNamedItem(player.Team == CsTeam.Terrorist ? CsItem.Molotov : CsItem.Incendiary);
                break;
        }
    }
}