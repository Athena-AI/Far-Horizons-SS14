using Content.Shared._FarHorizons.Slasher.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._FarHorizons.Slasher.Systems;

public sealed class SharedSlasherSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherWeaponComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<SlasherComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnMeleeHit(Entity<SlasherWeaponComponent> ent, ref MeleeHitEvent args)
    {
        if(HasComp<SlasherComponent>(args.User))
            args.BonusDamage = ent.Comp.BonusDamage;
    }

    private void OnShotAttempted(Entity<SlasherComponent> ent, ref ShotAttemptedEvent args)
    {
        _popup.PopupClient(Loc.GetString("gun-disabled"), ent, ent);
        args.Cancel();
    }
}