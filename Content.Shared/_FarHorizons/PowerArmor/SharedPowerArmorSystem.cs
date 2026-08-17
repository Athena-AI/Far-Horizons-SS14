
using Content.Shared._FarHorizons.LimbDamage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Robust.Shared.Containers;

namespace Content.Shared._FarHorizons.PowerArmor;

public sealed partial class SharedPowerArmorSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamage);
        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<LimbDamageModifyEvent>>(OnLimbDamage);
    }

    private void OnDamage(Entity<PowerArmorComponent> ent, ref InventoryRelayedEvent<DamageModifyEvent> args)
    {
        if(!_container.TryGetContainer(ent.Owner, "chest", out var armor)) return;
    }

    private void OnLimbDamage(Entity<PowerArmorComponent> ent, ref InventoryRelayedEvent<LimbDamageModifyEvent> args)
    {
        BaseContainer? armor = null;
        switch (args.Args.Target.Id)
        {
            case "Head":
                if(!_container.TryGetContainer(ent.Owner, "head", out armor)) return;
                break;
            case "ArmLeft" or "HandLeft":
                if(!_container.TryGetContainer(ent.Owner, "larm", out armor)) return;
                break;
            case "ArmRight" or "HandRight":
                if(!_container.TryGetContainer(ent.Owner, "rarm", out armor)) return;
                break;
            case "LegLeft" or "FootLeft":
                if(!_container.TryGetContainer(ent.Owner, "lleg", out armor)) return;
                break;
            case "LegRight" or "FootRight":
                if(!_container.TryGetContainer(ent.Owner, "rleg", out armor)) return;
                break;
            default:
                break;
        }
        if(armor == null) return;
    }
}