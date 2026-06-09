namespace RetakesPlugin.Guns;

public class GunsConfig
{
    public bool AllowAWP { get; set; } = true;
    public bool EnablePersistence { get; set; } = true;
    public string DefaultPrimary_T { get; set; } = "weapon_ak47";
    public string DefaultPrimary_CT { get; set; } = "weapon_m4a1_silencer";
    public string DefaultSecondary { get; set; } = "weapon_deagle";
}
