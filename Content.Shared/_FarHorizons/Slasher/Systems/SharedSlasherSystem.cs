using Content.Shared._FarHorizons.Slasher.Components;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._FarHorizons.Slasher.Systems;

public abstract class SharedSlasherSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<SlasherWeaponComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMapInit(Entity<SlasherComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.IncorporealizeActionEntity, ent.Comp.IncorporealizeAction);
        Dirty(ent);
    }

    private void OnShotAttempted(Entity<SlasherComponent> ent, ref ShotAttemptedEvent args)
    {
        _popup.PopupClient(Loc.GetString("gun-disabled"), ent, ent);
        args.Cancel();
    }

    private void OnMeleeHit(Entity<SlasherWeaponComponent> ent, ref MeleeHitEvent args)
    {
        if(HasComp<SlasherComponent>(args.User))
            args.BonusDamage = ent.Comp.BonusDamage;
    }
}