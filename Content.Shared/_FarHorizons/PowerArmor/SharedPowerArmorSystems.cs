
using System.Linq;
using Content.Shared._FarHorizons.LimbDamage.Components;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Shared.Tools.Components;
using Content.Shared.Verbs;
using Content.Shared.Wires;
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
    [Dependency] protected SharedPopupSystem _popUp = default!;
    [Dependency] protected SharedDoAfterSystem _doAfter = default!;
    [Dependency] protected PowerCellSystem _powerCell = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PowerArmorComponent, GetVerbsEvent<AlternativeVerb>>(OnEquipVerb);
        SubscribeLocalEvent<PowerArmorComponent, ClothingGotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<PowerArmorComponent, ClothingGotUnequippedEvent>(OnUnequip);
        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamage);
        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<LimbDamageModifyEvent>>(OnLimbDamage);
        SubscribeLocalEvent<PowerArmorComponent, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<PowerArmorComponent, UninstallArmorPartMessage>(OnUninstallMessage);
        SubscribeLocalEvent<PowerArmorComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<PowerArmorComponent, AttemptSimpleToolUseEvent>(OnToolAttempt);
        SubscribeLocalEvent<PowerArmorComponent, SimpleToolDoAfterEvent>(ToolDoAfterComplete);
        SubscribeLocalEvent<PowerArmorComponent, InstallPartDoAfter>(AfterInstallDoAfter);
        SubscribeLocalEvent<PowerArmorComponent, TogglePowerArmorMessage>(OnPowerToggle);

        SubscribeLocalEvent<PowerArmorPartComponent, EntGotInsertedIntoContainerMessage>(OnPartInserted);
        SubscribeLocalEvent<PowerArmorPartComponent, EntGotRemovedFromContainerMessage>(OnPartEjected);
        SubscribeLocalEvent<PowerArmorPartComponent, BreakageEventArgs>(OnPartBroken);
        SubscribeLocalEvent<PowerArmorPartComponent, AfterInteractEvent>(OnInteractUsing);

        SubscribeLocalEvent<PowerArmorModuleComponent, EntGotInsertedIntoContainerMessage>(OnModuleInstalled);
        SubscribeLocalEvent<PowerArmorModuleComponent, EntGotRemovedFromContainerMessage>(OnModuleUninstalled);
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

    private void OnUninstallMessage(Entity<PowerArmorComponent> ent, ref UninstallArmorPartMessage args)
    {
        var partUid = GetEntity(args.Part);
        ent.Comp.UninstallTarget = partUid;
        Dirty(ent);
    }

    private void OnExamine(Entity<PowerArmorComponent> ent, ref ExaminedEvent args)
    {
        if(ent.Comp.UninstallTarget == null) return;
        if(TryComp<WiresPanelComponent>(ent.Owner, out var wires) && !wires.Open)
            args.PushMarkup("Open maintenance panel first.");
        else
            args.PushMarkup("Use a crowbar to uninstall part.");
    }

    private void OnToolAttempt(Entity<PowerArmorComponent> ent, ref AttemptSimpleToolUseEvent args)
    {
        if(!TryComp<WiresPanelComponent>(ent.Owner, out var wire))
            return;

        if(ent.Comp.UninstallTarget != null || wire.Open)
            return;
            
        if(ent.Comp.UninstallTarget == null)
            _popUp.PopupClient("No uninstall target on this armor.", args.User);
        if(!wire.Open)
            _popUp.PopupClient("Open wire panel first.", args.User);

        args.Cancelled = true;
    }

    private void ToolDoAfterComplete(Entity<PowerArmorComponent> ent, ref SimpleToolDoAfterEvent args)
    {
        if(!Exists(ent.Comp.UninstallTarget)) return;

        _container.TryRemoveFromContainer(ent.Comp.UninstallTarget.Value);
        ent.Comp.UninstallTarget = null;
        Dirty(ent);
    }

    private void AfterInstallDoAfter(Entity<PowerArmorComponent> ent, ref InstallPartDoAfter args)
    {
        var part = GetEntity(args.Part);
        if(!HasComp<PowerArmorPartComponent>(part)) return;
        if(args.PartType == PowerArmorVisualLayers.Head 
        && _container.TryGetContainer(ent.Comp.OtherHalf, "parts", out var headPartContainer)
        && TryComp<PowerArmorComponent>(ent.Comp.OtherHalf, out var paComp))
        {
            _container.InsertOrDrop(part, headPartContainer);
            paComp.Parts[args.PartType] = part;
            Dirty(ent.Comp.OtherHalf, paComp);
        }
        else if(_container.TryGetContainer(ent.Owner, "parts", out var partContainer))
        {
            _container.InsertOrDrop(part, partContainer);
            ent.Comp.Parts[args.PartType] = part;
            Dirty(ent);
        }
    }

    private void OnPowerToggle(Entity<PowerArmorComponent> ent, ref TogglePowerArmorMessage args)
    {
        if(!TryComp<PowerCellDrawComponent>(ent.Owner, out var drawComp)) return;

        _powerCell.SetDrawEnabled(ent.Owner, !drawComp.Enabled);
        ent.Comp.IsPowered = !ent.Comp.IsPowered;
        Dirty(ent);
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

    private void OnInteractUsing(Entity<PowerArmorPartComponent> ent, ref AfterInteractEvent args)
    {
        if(!TryComp<PowerArmorComponent>(args.Target, out var PAComp)
        || !PAComp.IsPrimary) 
            return;

        foreach(var part in PAComp.Parts)
        {
            if(ent.Comp.PartType == part.Key && part.Value != null)
            {
                _popUp.PopupClient("This piece is already installed.", args.User);
                return;
            }
        }

        if(Exists(PAComp.OtherHalf))
        {
            if(TryComp<PowerArmorComponent>(PAComp.OtherHalf, out var PAComp2))
            if(ent.Comp.PartType == PAComp2.Parts.First().Key && PAComp2.Parts.First().Value != null)
            {
                _popUp.PopupClient("This piece is already installed.", args.User);
                return;
            }
        }

        var InstallDoAfter = new InstallPartDoAfter(ent.Comp.PartType , GetNetEntity(args.Used));
        var installDoAfter = new DoAfterArgs(EntityManager, args.User, ent.Comp.InstallTime, InstallDoAfter, args.Target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = true
        };
        _doAfter.TryStartDoAfter(installDoAfter);
        args.Handled = true;   
    }

    #endregion
    #region Power Armor Modules
    private void OnModuleInstalled(Entity<PowerArmorModuleComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if(!TryComp<PowerCellDrawComponent>(args.Entity, out var drawComp)) return;

        if(TryComp<PowerArmorPassiveModuleComponent>(ent.Owner, out var components))
        {
            foreach (var comp in (components.Components ?? []).Values)
                if (!HasComp(args.Entity, comp.Component.GetType()))
                    AddComp(args.Entity, comp.Component);
            _powerCell.SetDrawRate(args.Entity, drawComp.DrawRate + ent.Comp.PowerDrain);
        }
    }

    private void OnModuleUninstalled(Entity<PowerArmorModuleComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if(!TryComp<PowerCellDrawComponent>(args.Entity, out var drawComp)) return;

        if(TryComp<PowerArmorPassiveModuleComponent>(ent.Owner, out var components))
        {
            foreach (var comp in (components.Components ?? []).Values)
                if (HasComp(args.Entity, comp.Component.GetType()))
                    RemComp(args.Entity, EntityManager.GetComponent(args.Entity, comp.Component.GetType()));
            _powerCell.SetDrawRate(args.Entity, drawComp.DrawRate - ent.Comp.PowerDrain);
        }
    }
    
    #endregion
}