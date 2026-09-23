
using Content.Server.Popups;
using Content.Server.Silicons.Laws;
using Content.Shared._FarHorizons.Silicons.Laws.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mind.Components;
using Content.Shared.Overlays;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Wires;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Silicons.Laws;

public sealed partial class SiliconLawSetterSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private SiliconLawSystem _laws = default!;
    [Dependency] private SharedRoleSystem _roles = default!;
    [Dependency] private PopupSystem _popup = default!;

    [SubscribeLocalEvent]
    private void OnInsertLawboard(Entity<LawboardSlotComponent> entity, ref EntInsertedIntoContainerMessage args)
    {
        if(!TryComp<SiliconLawSetterComponent>(entity.Owner, out var slsComp) 
        || !TryComp<SiliconLawProviderComponent>(args.Entity, out var slpComp) )
            return;

        slsComp.Lawset = slpComp.Laws;
        slsComp.LawUploadSound = slpComp.LawUploadSound;
    }

    [SubscribeLocalEvent]
    private void OnEjectLawboard(Entity<LawboardSlotComponent> entity, ref EntRemovedFromContainerMessage args)
    {
        if(!TryComp<SiliconLawSetterComponent>(entity.Owner, out var slsComp))
            return;

        slsComp.Lawset = default;
        slsComp.LawUploadSound = default;
    }
    
    [SubscribeLocalEvent]
    private void OnInteractUsingSetter(Entity<SiliconLawSetterComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || ent.Comp.Lawset == null || !HasComp<SiliconLawBoundComponent>(args.Target))
            return;

        if(TryComp<WiresPanelComponent>(args.Target, out var wires) && !wires.Open)
        {
            _popup.PopupEntity(Loc.GetString("encryption-keys-panel-locked"), args.User, Shared.Popups.PopupType.SmallCaution);
            return;
        }

        var ev = new SetLawsetDoAfter(ent.Comp.Lawset.Value, ent.Comp.ResetSubversion, ent.Comp.LawUploadSound);
        var doAfter = new DoAfterArgs(EntityManager, args.User, TimeSpan.FromSeconds(ent.Comp.SettingTime), ev, args.Target, args.Target, args.Used)
        {
            BreakOnMove = true,
            NeedHand = true,
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfter);
    }

    [SubscribeLocalEvent]
    private void OnLawsetDoAfter(Entity<SiliconLawBoundComponent> ent, ref SetLawsetDoAfter args)
    {
        if (!_proto.TryIndex(args.Lawset, out var lawset))
            return;

        List<SiliconLaw> newLawset = new();
        foreach (var lawProto in lawset.Laws)
        {
            if (!_proto.TryIndex(lawProto, out var law))
                continue;

            newLawset.Add(law.ShallowClone());
        }

        _laws.SetLaws(newLawset, ent.Owner, args.LawUploadSound);

        if(args.ResetSubversion && TryComp<MindContainerComponent>(ent.Owner, out var mindComp) && mindComp.Mind != null)
        {
            if (TryComp<ShowCrewIconsComponent>(ent.Owner, out var crewIconComp))
            {
                crewIconComp.UncertainCrewBorder = false;
                Dirty(ent.Owner, crewIconComp);
            }

            if (_roles.MindHasRole<SubvertedSiliconRoleComponent>(mindComp.Mind.Value))
                _roles.MindRemoveRole<SubvertedSiliconRoleComponent>(mindComp.Mind.Value);
        }
    }
}