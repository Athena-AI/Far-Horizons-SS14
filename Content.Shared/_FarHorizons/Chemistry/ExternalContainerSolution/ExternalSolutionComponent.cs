using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.Chemistry;

/// <summary>
/// Component for having a solution be read from an external container.
/// </summary>
[RegisterComponent, AutoGenerateComponentState, NetworkedComponent]
public sealed partial class ExternalSolutionComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string Solution = "default";

    [ViewVariables(VVAccess.ReadWrite)]
    public Entity<SolutionComponent>? solutionComponent;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public EntityUid? ExternalContainer;
} 