
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.PowerArmor;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PowerArmorComponent : Component
{
    /// <summary>
    /// The total speed penalty applied by this power armor.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public float TotalSpeedModifier = 1.0f;
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

    /// <summary>
    /// The speed penalty applied by this part.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SpeedModifier = 1.0f;

    /// <summary>
    /// The type of part this part is.
    /// </summary>
    [DataField(required: true)]
    public PowerArmorVisualLayers PartType = default;

    /// <summary>
    /// Determines if the part is broken.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool isBroken = false;
}

[Serializable, NetSerializable]
public enum PowerArmorMenuUiKey : byte
{
    Key
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