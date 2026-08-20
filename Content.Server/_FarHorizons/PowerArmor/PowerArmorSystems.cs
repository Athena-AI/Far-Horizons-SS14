
using Content.Server.Destructible;
using Content.Shared._FarHorizons.PowerArmor;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Robust.Shared.Containers;

namespace Content.Server._FarHorizons.PowerArmor;

public sealed partial class PowerArmorSystem : SharedPowerArmorSystem
{
    [Dependency] DestructibleSystem _destructible = default!;   
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PowerArmorPartComponent, MapInitEvent>(OnPAPInit);
        SubscribeLocalEvent<PowerArmorComponent, MapInitEvent>(OnPAInit, after:[typeof(ToggleableClothingSystem)]);
    }

    private void OnPAPInit(Entity<PowerArmorPartComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.MaxIntegrity = _destructible.DestroyedAt(ent.Owner);
        Dirty(ent);
    }

    private void OnPAInit(Entity<PowerArmorComponent> ent, ref MapInitEvent args)
    {
        if(TryComp<AttachedClothingComponent>(ent.Owner, out var acComp))
            ent.Comp.OtherHalf = acComp.AttachedUid;

        else if (_container.TryGetContainer(ent.Owner, "toggleable-clothing", out var tcContainer))
        {
            foreach (var item in tcContainer.ContainedEntities)
            {
                if (TryComp<PowerArmorComponent>(item, out var paComp1)
                    && paComp1.Parts.TryGetValue(PowerArmorVisualLayers.Head, out var headPart))
                {
                    ent.Comp.OtherHalf = item;
                }
            }
        }

        Dirty(ent);
    }

    protected override void OnPartInserted(Entity<PowerArmorPartComponent> ent, ref EntGotInsertedIntoContainerMessage args) 
        => base.OnPartInserted(ent, ref args);

    protected override void OnPartEjected(Entity<PowerArmorPartComponent> ent, ref EntGotRemovedFromContainerMessage args) 
        => base.OnPartEjected(ent, ref args);
}