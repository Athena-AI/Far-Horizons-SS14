using Content.Shared._FarHorizons.Slasher.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Slasher.Components;

/// <summary>
/// Component for the slasher
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedSlasherSystem))]
public sealed partial class SlasherComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId IncorporealizeAction = "ActionPolymorphIncorporealize";
    
    [DataField, AutoNetworkedField] 
    public EntityUid? IncorporealizeActionEntity;
}
