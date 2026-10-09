namespace Content.Server._FarHorizons.Zombies;

[RegisterComponent]
public sealed partial class ZombifyOnSpawnComponent : Component
{
    [DataField]
    public bool AllowSpreading = true;
}