
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.PowerArmor;

[RegisterComponent]
public sealed partial class PowerArmorComponent : Component
{
    
}

[RegisterComponent]
public sealed partial class PowerArmorPartComponent : Component
{
    
}

[Serializable, NetSerializable]
public enum PowerArmorVisualLayers : byte
{
    UnderArmor,
    Chest,
    Head,
    RArm,
    LArm,
    RLeg,
    LLeg
}