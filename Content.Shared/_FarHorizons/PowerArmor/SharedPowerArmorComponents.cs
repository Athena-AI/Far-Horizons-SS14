
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.PowerArmor;

[RegisterComponent]
public sealed partial class PowerArmorComponent : Component
{
    
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PowerArmorPartComponent : Component
{
    /// <summary>
    /// The damage reduction when the part is unbroken
    /// </summary>
    [DataField(required: true)]
    public DamageModifierSet Modifiers = default!;

    /// <summary>
    /// The damage reduction when the part is broken
    /// </summary>
    [DataField(required: true)]
    public DamageModifierSet BrokenModifiers = default!;

    [DataField(required: true)]
    public PowerArmorVisualLayers PartType = default;

    [ViewVariables, AutoNetworkedField]
    public bool isBroken = false;
}

[Serializable, NetSerializable]
public enum PowerArmorPartVisuals : byte
{
    PowerArmor,
    Visible
}

[Serializable, NetSerializable]
public enum PowerArmorVisualLayers : byte
{
    UnderArmor,
    UnderArmorHelmet,
    Chest,
    Head,
    RArm,
    LArm,
    RLeg,
    LLeg
}