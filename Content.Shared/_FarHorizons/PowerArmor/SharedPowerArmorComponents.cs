
using Content.Shared.Damage;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.PowerArmor;

[RegisterComponent]
public sealed partial class PowerArmorComponent : Component
{
    
}

[RegisterComponent]
public sealed partial class PowerArmorPartComponent : Component
{
    /// <summary>
    /// The damage reduction
    /// </summary>
    [DataField(required: true)]
    public DamageModifierSet Modifiers = default!;

    [DataField(required: true)]
    public PowerArmorVisualLayers PartType = default;
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