using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.Fluids.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class FluidFootprintSourceComponent : Component
{
    [DataField(required: true)] public float MinUnitsForFootPrint = default!;
    [DataField(required: true)] public int MinFootprints = default!;
    [DataField(required: true)] public float BonusPerUnit = default!;
    [DataField(required: true)] public TimeSpan StopAfter = default!;
}