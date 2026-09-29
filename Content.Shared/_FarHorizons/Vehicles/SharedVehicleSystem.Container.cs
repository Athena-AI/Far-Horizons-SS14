using Content.Shared._FarHorizons.Vehicles.Components;
using Content.Shared._FarHorizons.Vehicles.Events;
using System.Linq;
using Robust.Shared.Containers;
using Content.Shared.Verbs;
using Content.Shared.DoAfter;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.DragDrop;

namespace Content.Shared._FarHorizons.Vehicles;

public abstract partial class SharedVehicleSystem
{    
    [SubscribeLocalEvent]
    private void OnVehicleEntryDoAfter(Entity<VehicleContainerComponent> ent, ref VehicleEntryDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if(!TryComp<VehicleComponent>(ent, out var vehicleComp)) return;
        if(!TryInsert(args.Args.Target, ent.Owner, ent.Comp)) return;

        SetUpRider(args.Args.Target!.Value, ent.Owner, vehicleComp);

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnVehicleRemoveDoAfter(Entity<VehicleContainerComponent> ent, ref VehicleRemoveDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;
        
        if(!TryComp<VehicleComponent>(ent, out var vehicleComp)) return;

        var target = GetEntity(args.Target);
        RemoveRider(target, ent.Owner, vehicleComp);
        TryRemove(target, ent.Owner, ent.Comp);

        args.Handled = true;
    }

    [SubscribeLocalEvent(after:[typeof(SharedContainerSystem)])]
    private void OnInsertAttempt(Entity<VehicleContainerComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        if (ent.Comp.PassengerSlot == null || args.Container.ID != ent.Comp.PassengerSlot.ID || _tags.HasTag(args.EntityUid, s_vehicleKeyTag)) return;
        if (_whitelist.IsWhitelistFail(ent.Comp.PassengerWhitelist, args.EntityUid))
            args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnAlternativeVerb(Entity<VehicleContainerComponent> ent , ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if(!TryComp<VehicleComponent>(ent.Owner, out var vehicleComp) || vehicleComp.isBroken) return; 
        var user = args.User;

        if (CanInsert(ent) && !ent.Comp.PassengerSlot.ContainedEntities.Contains(user))
        {
            var enterVerb = new AlternativeVerb
            {
                Text = Loc.GetString("vehicle-verb-enter"),
                Act = () =>
                {
                    var doAfterEventArgs = new DoAfterArgs(EntityManager, user, ent.Comp.EntryTime, new VehicleEntryDoAfter(), ent.Owner, target: user)
                    {
                        BreakOnMove = true,
                    };
                        
                    _doAfter.TryStartDoAfter(doAfterEventArgs);
                }
            };
            args.Verbs.Add(enterVerb);
        }
        else if(ent.Comp.PassengerSlot.ContainedEntities.Contains(user))
        {
            var exitVerb = new AlternativeVerb
            {
                Text = Loc.GetString("vehicle-verb-leave"),
                Act = () =>
                {
                    TryRemove(user, ent);
                    if(HasComp<RiderComponent>(user))
                        RemoveRider(user, ent.Owner, vehicleComp);
                }
            };
            args.Verbs.Add(exitVerb);
        }
            
        if(ent.Comp.PassengerSlot.ContainedEntities.Count != 0 && !ent.Comp.PassengerSlot.ContainedEntities.Contains(user))
        {
            var category = new VerbCategory("Remove", null);
            foreach (var passenger in ent.Comp.PassengerSlot.ContainedEntities)
            {
                var removeVerb = new AlternativeVerb
                {
                    Text = Loc.GetString("vehicle-verb-remove", ("passenger", MetaData(passenger).EntityName)),
                    Category = category,
                    Act = () =>
                    {
                        if(_gameTiming.IsFirstTimePredicted && _net.IsClient)
                            _popup.PopupClient(Loc.GetString("vehicle-remove-passenger-attempt", ("user", MetaData(user).EntityName), ("passenger", MetaData(passenger).EntityName)), ent.Owner, passenger, PopupType.LargeCaution);
                            var doAfterEventArgs = new DoAfterArgs(EntityManager, user, ent.Comp.RemoveTime, new VehicleRemoveDoAfter(GetNetEntity(passenger)), ent.Owner, target: ent.Owner)
                        {
                            BreakOnMove = true,
                        };
                        _adminLogger.Add(LogType.Verb, LogImpact.Medium, $"{ToPrettyString(user)} attempted to remove a passenger from {ToPrettyString(ent.Owner)}");

                        _doAfter.TryStartDoAfter(doAfterEventArgs);
                    }
                };
                args.Verbs.Add(removeVerb);
            }
        }
    }

    [SubscribeLocalEvent]
    private void OnDragDrop(Entity<VehicleContainerComponent> ent, ref DragDropTargetEvent args)
    {
        if(args.Handled) return;
        args.Handled = true;
        if(TryComp<VehicleComponent>(ent.Owner, out var vehicleComp) && vehicleComp.isBroken) return;

        if(!CanInsert(ent.Owner, ent.Comp)) return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, ent.Comp.EntryTime, new VehicleEntryDoAfter(), ent.Owner, target: args.Dragged)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }
}