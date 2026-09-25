
using Content.Shared._FarHorizons.Silicons.Laws.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Wires;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Silicons.Laws;

public sealed partial class SiliconLawSetterSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedSiliconLawSystem _laws = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    [SubscribeLocalEvent]
    private void OnInsertLawboard(Entity<LawboardSlotComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if(!TryComp<SiliconLawSetterComponent>(ent.Owner, out var slsComp) 
        || !TryComp<SiliconLawProviderComponent>(args.Entity, out var slpComp) )
            return;

        slsComp.Lawset = slpComp.Laws;
        slsComp.LawUploadSound = slpComp.LawUploadSound;
        Dirty(ent, slsComp);
    }

    [SubscribeLocalEvent]
    private void OnEjectLawboard(Entity<LawboardSlotComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if(!TryComp<SiliconLawSetterComponent>(ent.Owner, out var slsComp))
            return;

        slsComp.Lawset = default;
        slsComp.LawUploadSound = default;
        Dirty(ent, slsComp);
    }
    
    [SubscribeLocalEvent]
    private void OnInteractUsingSetter(Entity<SiliconLawSetterComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || ent.Comp.Lawset == null || !HasComp<SiliconLawBoundComponent>(args.Target))
            return;

        if(TryComp<WiresPanelComponent>(args.Target, out var wires) && !wires.Open)
        {
            _popup.PopupEntity(Loc.GetString("encryption-keys-panel-locked"), args.User, PopupType.SmallCaution);
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

        List<SiliconLaw> lawList = new();
        foreach (var lawProto in lawset.Laws)
        {
            if (!_proto.TryIndex(lawProto, out var law))
                continue;

            lawList.Add(law.ShallowClone());
        }

        var newLawset = new SiliconLawset
        {
            Laws = lawList
        };

        _laws.SetLawset(ent.Owner, newLawset, args.LawUploadSound);

        if(args.ResetSubversion && TryComp<MindContainerComponent>(ent.Owner, out var mindComp) && mindComp.Mind != null)
            _laws.RemoveSubvertedSiliconRole(mindComp.Mind.Value);
    }
}