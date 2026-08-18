
using Content.Shared._FarHorizons.LimbDamage.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

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
        if (!_container.TryGetContainer(ent.Owner, "parts", out var parts))
            return;
        parts.ContainedEntities.TryFirstOrNull(x =>
        {
            if (!TryComp<PowerArmorPartComponent>(x, out var papComp))
                return false;

            return papComp.PartType == PowerArmorVisualLayers.Chest;
        }, out var part);

        if (part == null || !TryComp<PowerArmorPartComponent>(part, out var papComp) || papComp.isBroken)
            return;

        var modifiers = papComp.Modifiers;
        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, modifiers, args.Args.ArmorPenetration, args.Args.CanHeal);
        _damageable.ChangeDamage(part.Value, args.Args.Damage);
    }

    private void OnLimbDamage(Entity<PowerArmorComponent> ent, ref InventoryRelayedEvent<LimbDamageModifyEvent> args)
    {
        if (!_container.TryGetContainer(ent.Owner, "parts", out var parts))
            return;

        var limb = args.Args.Target.Id;

        var partType = limb switch
        {
            "Head" => PowerArmorVisualLayers.Head,

            "ArmLeft" or "HandLeft" => PowerArmorVisualLayers.LArm,
            "ArmRight" or "HandRight" => PowerArmorVisualLayers.RArm,

            "LegLeft" or "FootLeft" => PowerArmorVisualLayers.LLeg,
            "LegRight" or "FootRight" => PowerArmorVisualLayers.RLeg,

            _ => (PowerArmorVisualLayers?) null
        };

        if (partType == null)
            return;

        parts.ContainedEntities.TryFirstOrNull(x =>
        {
            if (!TryComp<PowerArmorPartComponent>(x, out var papComp))
                return false;

            return papComp.PartType == partType.Value;
        }, out var part);

        if (part == null || !TryComp<PowerArmorPartComponent>(part, out var papComp) || papComp.isBroken)
            return;

        var modifiers = papComp.Modifiers;
        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, modifiers, args.Args.ArmorPenetration, args.Args.CanHeal);
        _damageable.ChangeDamage(part.Value, args.Args.Damage);
    }
}