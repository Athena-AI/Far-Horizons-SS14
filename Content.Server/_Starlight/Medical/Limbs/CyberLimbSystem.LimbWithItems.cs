using Content.Server.Body;
using Content.Shared._Starlight.Medical.Limbs;
using Content.Shared.Actions.Components;
using Content.Shared.Body;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Components;
using Content.Shared.Starlight;
using Content.Shared._Starlight.Cybernetics;
using Content.Shared._Starlight.Cybernetics.Components;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;

namespace Content.Server._Starlight.Medical.Limbs;
public sealed partial class CyberLimbSystem : EntitySystem
{
    public void InitializeLimbWithItems()
    {
        SubscribeLocalEvent<LimbItemDeployerComponent, ToggleLimbEvent>(OnLimbToggle);
        SubscribeLocalEvent<LimbItemDeployerComponent, OrganGotInsertedEvent>(LimbWithItemsInserted);
        SubscribeLocalEvent<LimbItemDeployerComponent, OrganGotRemovedEvent>(LimbWithItemsRemoved);

        SubscribeLocalEvent<LimbItemDeployerComponent, BodyRelayedEvent<CyberneticDisruptionEvent>>(OnCyberneticsDisrupted);
    }

    private void LimbWithItemsInserted(Entity<LimbItemDeployerComponent> ent, ref OrganGotInsertedEvent args) => 
        _actions.GrantContainedActions(_slEnt.Entity<ActionsComponent>(args.Target), _slEnt.Entity<ActionsContainerComponent>(ent));

    private void LimbWithItemsRemoved(Entity<LimbItemDeployerComponent> ent, ref OrganGotRemovedEvent args)
    {
        if (TerminatingOrDeleted(ent)) return;
        if (ent.Comp.Toggled)
        {
            var toggleLimbEvent = new ToggleLimbEvent()
            {
                Performer = args.Target,
            };
            OnLimbToggle(ent, ref toggleLimbEvent);
        }

        _actions.RemoveProvidedActions(args.Target, ent);
    }

    private void OnLimbToggle(Entity<LimbItemDeployerComponent> ent, ref ToggleLimbEvent args)
    {
        if (!TryComp<LimbItemStorageComponent>(ent, out var storage))
            return;
            
        ent.Comp.Toggled = !ent.Comp.Toggled && (!ent.Comp.IsCybernetic || !TryComp(args.Performer, out CyberneticDisruptionComponent? _));

        if (ent.Comp.Toggled)
        {
            foreach (var item in storage.ItemEntities)
            {
                var handId = $"{ent.Owner}_{item}";
                var hands = EnsureComp<HandsComponent>(args.Performer);
                _hands.AddHand((args.Performer, hands), handId, HandLocation.Functional, whitelist: ent.Comp.HandWhitelist);
                _hands.DoPickup(args.Performer, handId, item, hands);
                EnsureComp<UnremoveableComponent>(item);
                if (storage.ItemEntities.Count == 1) //FH, if this cybernetic only has one item, set it to active hand
                    _hands.SetActiveHand((args.Performer, hands), handId); //FH, this makes combat cybernetics with weapons much stronger
            }
        }
        else
        {
            var container = _container.EnsureContainer<Container>(ent.Owner, ent.Comp.ContainerId, out _);
            foreach (var item in storage.ItemEntities)
            {
                var handId = $"{ent.Owner}_{item}";
                RemComp<UnremoveableComponent>(item);
                _container.Insert(_slEnt.Entity<TransformComponent, MetaDataComponent, PhysicsComponent>(item), container, force: true);
                _hands.RemoveHand(args.Performer, handId);
            }
        }

        if (TryComp<VisualOrganComponent>(ent, out var visualOrgan))
        {
            visualOrgan.Data.State = ent.Comp.Toggled ? ent.Comp.StateOn : ent.Comp.StateOff;
            Dirty<VisualOrganComponent>((ent.Owner, visualOrgan));
        }

        _audio.PlayPvs(ent.Comp.Sound, args.Performer);

        Dirty(ent);
    }

    private void OnCyberneticsDisrupted(Entity<LimbItemDeployerComponent> ent, ref BodyRelayedEvent<CyberneticDisruptionEvent> args)
    {
        if(!ent.Comp.IsCybernetic)
            return;

        if (ent.Comp.Toggled)
        {
            var ev = new ToggleLimbEvent
            {
                Performer = args.Args.Target,
            };
            RaiseLocalEvent(ent, ev);
        }
    }
}
