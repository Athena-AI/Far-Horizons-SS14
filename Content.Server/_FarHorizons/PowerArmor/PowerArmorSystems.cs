
using Content.Shared._FarHorizons.PowerArmor;
using Robust.Shared.Containers;

namespace Content.Server._FarHorizons.PowerArmor;

public sealed partial class PowerArmorSystem : SharedPowerArmorSystem
{
    public override void Initialize()
        => base.Initialize();

    protected override void OnPartInserted(Entity<PowerArmorPartComponent> ent, ref EntGotInsertedIntoContainerMessage args) 
        => base.OnPartInserted(ent, ref args);

    protected override void OnPartEjected(Entity<PowerArmorPartComponent> ent, ref EntGotRemovedFromContainerMessage args) 
        => base.OnPartEjected(ent, ref args);
}