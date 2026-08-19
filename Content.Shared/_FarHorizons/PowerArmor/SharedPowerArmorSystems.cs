
using Content.Shared._FarHorizons.LimbDamage.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Shared._FarHorizons.PowerArmor;

public abstract partial class SharedPowerArmorSystem : EntitySystem
{
    [Dependency] protected SharedContainerSystem _container = default!;
    [Dependency] protected DamageableSystem _damageable = default!;
    [Dependency] protected SharedAppearanceSystem _appearance = default!;
    [Dependency] protected SharedInteractionSystem _interaction = default!;
    [Dependency] protected InventorySystem _inventory = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerArmorComponent, GetVerbsEvent<AlternativeVerb>>(OnEquipVerb);
        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamage);
        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<LimbDamageModifyEvent>>(OnLimbDamage);
        SubscribeLocalEvent<PowerArmorPartComponent, BreakageEventArgs>(OnPartBroken);
    }

    private void OnEquipVerb(Entity<PowerArmorComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanComplexInteract 
        || !_interaction.InRangeAndAccessible(args.User, args.Target))
            return;    

        var user = args.User;
        AlternativeVerb equip = new()
        {
            Act = () => _inventory.TryEquip(user, ent.Owner, "outerClothing", checkDoafter: true),
            Text = Loc.GetString("power-armor-verb-equip"),
            Priority = 1
        };

        args.Verbs.Add(equip);
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
        if(papComp.isBroken)
            modifiers = papComp.BrokenModifiers;

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
        if(papComp.isBroken)
            modifiers = papComp.BrokenModifiers;
        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, modifiers, args.Args.ArmorPenetration, args.Args.CanHeal);
        _damageable.ChangeDamage(part.Value, args.Args.Damage);
    }

    public void OnPartBroken(Entity<PowerArmorPartComponent> ent, ref BreakageEventArgs args)
    {
        var powerArmor = Transform(ent.Owner).ParentUid;
        var gridUid = Transform(ent.Owner).GridUid;
        if(powerArmor == gridUid) return;
        
        _appearance.SetData(ent.Owner, PowerArmorPartVisuals.PowerArmor, GetNetEntity(powerArmor));
        _appearance.SetData(ent.Owner, PowerArmorPartVisuals.Visible, false);
        ent.Comp.isBroken = true;
        Dirty(ent);
    }
}