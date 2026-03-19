using Content.Shared._FarHorizons.Slasher.Systems;
using Content.Shared.Actions;
using Content.Shared.Polymorph;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Slasher.Components;

/// <summary>
/// Component for the slasher jaunt
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedSlasherSystem))]
public sealed partial class SlasherGhostComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId RevertIncorporealizeAction = "ActionRevertIncorporealize";
    
    [DataField, AutoNetworkedField] public EntityUid? RevertIncorporealizeActionEntity;
}

public sealed partial class IncorporealizeActionEvent : InstantActionEvent
{
    [DataField]
    public ProtoId<PolymorphPrototype>? ProtoId;

    public IncorporealizeActionEvent(ProtoId<PolymorphPrototype> protoId) : this()
        => ProtoId = protoId;
}

public sealed partial class RevertIncorporealizeActionEvent : InstantActionEvent;
