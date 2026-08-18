
using System.Linq;
using Content.Shared._FarHorizons.LimbDamage.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Robust.Shared.Containers;

namespace Content.Shared._FarHorizons.PowerArmor;

public abstract partial class SharedPowerArmorSystem : EntitySystem
{
    [Dependency] protected SharedContainerSystem _container = default!;
    [Dependency] protected DamageableSystem _damageable = default!;
    [Dependency] protected SharedAppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamage);
        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<LimbDamageModifyEvent>>(OnLimbDamage);
    }

    private void OnDamage(Entity<PowerArmorComponent> ent, ref InventoryRelayedEvent<DamageModifyEvent> args)
    {
        if(!_container.TryGetContainer(ent.Owner, "chest", out var armor)) return;
        if(armor == null || !TryComp<PowerArmorPartComponent>(armor.ContainedEntities.FirstOrDefault(), out var papComp)) return;

        DamageModifierSet modifiers = papComp.Modifiers;

        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, modifiers, args.Args.ArmorPenetration, args.Args.CanHeal);
        _damageable.TryChangeDamage(armor.ContainedEntities.FirstOrDefault(), args.Args.Damage);
    }

    private void OnLimbDamage(Entity<PowerArmorComponent> ent, ref InventoryRelayedEvent<LimbDamageModifyEvent> args)
    {
        BaseContainer? armor;
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
                return;
        }
        if(armor == null || !TryComp<PowerArmorPartComponent>(armor.ContainedEntities.FirstOrDefault(), out var papComp)) return;

        DamageModifierSet modifiers = papComp.Modifiers;

        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, modifiers, args.Args.ArmorPenetration, args.Args.CanHeal);
        _damageable.TryChangeDamage(armor.ContainedEntities.FirstOrDefault(), args.Args.Damage);
    }
}