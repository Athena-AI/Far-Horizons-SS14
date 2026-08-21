
using Content.Shared._FarHorizons.LimbDamage.Components;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Containers;

namespace Content.Shared._FarHorizons.PowerArmor;

public abstract partial class SharedPowerArmorSystem : EntitySystem
{
    [Dependency] protected SharedContainerSystem _container = default!;
    [Dependency] protected DamageableSystem _damageable = default!;
    [Dependency] protected SharedAppearanceSystem _appearance = default!;
    [Dependency] protected SharedInteractionSystem _interaction = default!;
    [Dependency] protected InventorySystem _inventory = default!;
    [Dependency] protected MovementSpeedModifierSystem _movement = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerArmorComponent, GetVerbsEvent<AlternativeVerb>>(OnEquipVerb);
        SubscribeLocalEvent<PowerArmorComponent, ClothingGotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<PowerArmorComponent, ClothingGotUnequippedEvent>(OnUnequip);
        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamage);
        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<LimbDamageModifyEvent>>(OnLimbDamage);
        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMoveSpeed);

        SubscribeLocalEvent<PowerArmorPartComponent, EntGotInsertedIntoContainerMessage>(OnPartInserted);
        SubscribeLocalEvent<PowerArmorPartComponent, EntGotRemovedFromContainerMessage>(OnPartEjected);
        SubscribeLocalEvent<PowerArmorPartComponent, BreakageEventArgs>(OnPartBroken);
    }

    #region Power Armor
    private void OnEquipVerb(Entity<PowerArmorComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanComplexInteract 
        || !_interaction.InRangeAndAccessible(args.User, args.Target)
        || _container.IsEntityInContainer(args.Target))
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

    private void OnEquip(Entity<PowerArmorComponent> ent, ref ClothingGotEquippedEvent args)
    {
        ent.Comp.Wearer = args.Wearer;
        Dirty(ent);
    }

    private void OnUnequip(Entity<PowerArmorComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        ent.Comp.Wearer = null;
        Dirty(ent);
    }
    
    private void OnDamage(Entity<PowerArmorComponent> ent, ref InventoryRelayedEvent<DamageModifyEvent> args)
    {
        if (!ent.Comp.Parts.TryGetValue(PowerArmorVisualLayers.Chest, out var part)
            || part == null
            || !TryComp<PowerArmorPartComponent>(part, out var papComp))
            return;

        var modifiers = papComp.Modifiers;
        if (papComp.isBroken)
            modifiers = papComp.BrokenModifiers;

        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, modifiers, args.Args.ArmorPenetration, args.Args.CanHeal);
        _damageable.ChangeDamage(part.Value, args.Args.Damage);
    }

    private void OnLimbDamage(Entity<PowerArmorComponent> ent, ref InventoryRelayedEvent<LimbDamageModifyEvent> args)
    {
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

        if (partType == null
            || !ent.Comp.Parts.TryGetValue(partType.Value, out var part)
            || part == null
            || !TryComp<PowerArmorPartComponent>(part, out var papComp))
            return;

        var modifiers = papComp.Modifiers;
        if (papComp.isBroken)
            modifiers = papComp.BrokenModifiers;
        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, modifiers, args.Args.ArmorPenetration, args.Args.CanHeal);
        _damageable.ChangeDamage(part.Value, args.Args.Damage);
    }

    private void OnRefreshMoveSpeed(Entity<PowerArmorComponent> ent, ref InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if (!ent.Comp.IsPrimary)
            return;

        var total = ent.Comp.TotalSpeedModifier;
        if (TryComp<PowerArmorComponent>(ent.Comp.OtherHalf, out var otherComp))
            total -= 1.0f-otherComp.TotalSpeedModifier;

        args.Args.ModifySpeed(total);
    }

    #endregion
    #region Power Armor Parts

    protected virtual void OnPartInserted(Entity<PowerArmorPartComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        ent.Comp.AttachedTo = args.Container.Owner;
        
        if(!TryComp<PowerArmorComponent>(ent.Comp.AttachedTo, out var paComp)) return;
        paComp.TotalSpeedModifier = (float) Math.Round(paComp.TotalSpeedModifier - ent.Comp.SpeedModifier, 2);

        paComp.Parts[ent.Comp.PartType] = ent.Owner;

        if(paComp.Wearer == null) return;

        _movement.RefreshMovementSpeedModifiers(paComp.Wearer.Value);
    }

    protected virtual void OnPartEjected(Entity<PowerArmorPartComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if(!TryComp<PowerArmorComponent>(ent.Comp.AttachedTo, out var paComp)) return;
        paComp.TotalSpeedModifier = (float) Math.Round(paComp.TotalSpeedModifier + ent.Comp.SpeedModifier, 2);

        paComp.Parts[ent.Comp.PartType] = null;
        ent.Comp.AttachedTo = null;

        if(paComp.Wearer == null) return;

        _movement.RefreshMovementSpeedModifiers(paComp.Wearer.Value);
    }

    public void OnPartBroken(Entity<PowerArmorPartComponent> ent, ref BreakageEventArgs args)
    {
        var powerArmor = Transform(ent.Owner).ParentUid;
        var gridUid = Transform(ent.Owner).GridUid;
        if(powerArmor == gridUid || !TryComp<PowerArmorComponent>(powerArmor, out var paComp)) return;

        paComp.TotalSpeedModifier = (float) Math.Round(paComp.TotalSpeedModifier + ent.Comp.SpeedModifier, 2);
        
        _appearance.SetData(ent.Owner, PowerArmorPartVisuals.PowerArmor, GetNetEntity(powerArmor));
        _appearance.SetData(ent.Owner, PowerArmorPartVisuals.Visible, false);
        ent.Comp.isBroken = true;
        Dirty(ent);

        if(paComp.Wearer == null) return;

        _movement.RefreshMovementSpeedModifiers(paComp.Wearer.Value);
    }
    #endregion
}