using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
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

    /// <summary>
    /// All the parts assigned to this power armor
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<PowerArmorVisualLayers, EntityUid?> Parts = new();

    [ViewVariables, AutoNetworkedField]
    public EntityUid OtherHalf;

    [AutoNetworkedField]
    public bool IsPrimary = true;

    [ViewVariables, AutoNetworkedField]
    public bool IsPowered = false;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? Wearer;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? UninstallTarget;
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

    [DataField]
    public TimeSpan InstallTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The max integrity of a part.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public FixedPoint2 MaxIntegrity = FixedPoint2.Zero;

    /// <summary>
    /// Determines if the part is broken.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool isBroken = false;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? AttachedTo;
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

[Serializable, NetSerializable]
public sealed class UninstallArmorPartMessage : BoundUserInterfaceMessage
{
    public readonly PowerArmorVisualLayers PartType;
    public readonly NetEntity Part;
    public UninstallArmorPartMessage(PowerArmorVisualLayers partType, NetEntity part)
    {
        PartType = partType;
        Part = part;
    }
}

[Serializable, NetSerializable]
public sealed partial class InstallPartDoAfter : SimpleDoAfterEvent
{
    public readonly PowerArmorVisualLayers PartType;
    public readonly NetEntity Part;
    public InstallPartDoAfter(PowerArmorVisualLayers partType, NetEntity part)
    {
        PartType = partType;
        Part = part;
    }
}

[Serializable, NetSerializable]
public sealed class TogglePowerArmorMessage : BoundUserInterfaceMessage;