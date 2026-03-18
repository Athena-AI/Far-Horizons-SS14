using Content.Shared._FarHorizons.Slasher.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.Slasher.Components;

/// <summary>
/// Component for the slasher
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedSlasherSystem))]
public sealed partial class SlasherComponent : Component
{
}
