
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
    /// The damage reduction
    /// </summary>
    [DataField(required: true)]
    public DamageModifierSet Modifiers = default!;

    [DataField(required: true)]
    public PowerArmorVisualLayers PartType = default;

    [ViewVariables, AutoNetworkedField]
    public bool isBroken = false;
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