using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization; //FH
using Content.Shared.Actions; //FH
namespace Content.Shared.Starlight;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LimbItemStorageComponent : Component
{
    [DataField(required: true)]
    public List<EntProtoId> Items = [];

    [DataField, AutoNetworkedField]
    public Dictionary<EntityUid, bool> ItemEntities = new(); //FH Edit

    [DataField, AutoNetworkedField]
    public string ContainerId = "cyberlimb";
}

#region FarHorizons
[Serializable, NetSerializable]
public enum LimbItemToggleMenuUiKey : byte
{
    Key
}

public sealed partial class ToggleCyberlimbMenuEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed class LimbToggleMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class LimbItemToggleMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity Item;
    public LimbItemToggleMessage(NetEntity item) 
        => Item = item;
}
#endregion