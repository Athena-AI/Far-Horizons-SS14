using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared._FarHorizons.Chemistry;

public sealed class SharedExternalSolutionSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _sharedSolution = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ExternalSolutionComponent, EntInsertedIntoContainerMessage>(OnContainerInserted);
        SubscribeLocalEvent<ExternalSolutionComponent, EntRemovedFromContainerMessage>(OnContainerEjected);
        SubscribeLocalEvent<ExternalSolutionComponent, SolutionContainerChangedEvent >(OnSolutionChanged);
        base.Initialize();
    }
    private void OnContainerInserted(Entity<ExternalSolutionComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if(!_gameTiming.IsFirstTimePredicted || !_sharedSolution.TryGetRefillableSolution(args.Entity, out var externalSolutionComp, out var externalSolution)
        || !_sharedSolution.ResolveSolution(ent.Owner, ent.Comp.Solution, ref ent.Comp.solutionComponent, out var internalSolution))
            return;

        ent.Comp.ExternalContainer = args.Entity;
        _sharedSolution.TryTransferSolution(ent.Comp.solutionComponent.Value, externalSolution, internalSolution.AvailableVolume);
        Dirty(ent);
    }

    private void OnContainerEjected(Entity<ExternalSolutionComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if(!_gameTiming.IsFirstTimePredicted || !_sharedSolution.TryGetRefillableSolution(args.Entity, out var externalSolutionComp, out var externalSolution)
        || !_sharedSolution.ResolveSolution(ent.Owner, ent.Comp.Solution, ref ent.Comp.solutionComponent, out var internalSolution))
            return;

        ent.Comp.ExternalContainer = null;
        _sharedSolution.TryTransferSolution(externalSolutionComp.Value, internalSolution, externalSolution.AvailableVolume);
        Dirty(ent);
    }

    private void OnSolutionChanged(Entity<ExternalSolutionComponent> ent, ref SolutionContainerChangedEvent args)
    {
        if(!_gameTiming.IsFirstTimePredicted || ent.Comp.ExternalContainer == null || !_sharedSolution.TryGetRefillableSolution(ent.Comp.ExternalContainer.Value, out var externalSolutionComp, out var externalSolution)
        || externalSolution.Volume <= 0 || !_sharedSolution.ResolveSolution(ent.Owner, ent.Comp.Solution, ref ent.Comp.solutionComponent, out var internalSolution))
            return;

        _sharedSolution.TryTransferSolution(ent.Comp.solutionComponent.Value, externalSolution, internalSolution.AvailableVolume);
    }
}