using Content.Shared._FarHorizons.Slasher.Systems;
using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.Slasher.Components;

/// <summary>
/// Component for the slasher weapons
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedSlasherSystem))]
public sealed partial class SlasherWeaponComponent : Component
{
    /// <summary>
    /// Bonus Damage for the slasher weapon when wielded by the slasher
    /// </summary>
    [DataField]
    public DamageSpecifier BonusDamage = new();
}
