using Content.Shared.DoAfter;
using Content.Shared.Stunnable;
using Content.Shared.Traits.Assorted;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Combat;

public sealed partial class StandUpOperator : HTNOperator
{
    [Dependency] private IEntityManager _entManager = default!;
    private SharedStunSystem _stunSystem = default!;

    [DataField("shutdownState")]
    public HTNPlanState ShutdownState { get; private set; } = HTNPlanState.TaskFinished;

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _stunSystem = sysManager.GetEntitySystem<SharedStunSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        if(!_entManager.HasComponent<CrawlerComponent>(owner))
            return HTNOperatorStatus.Finished;

        if (_entManager.HasComponent<LegsParalyzedComponent>(owner))
            return HTNOperatorStatus.Failed;

        if (_entManager.HasComponent<ActiveDoAfterComponent>(owner))
            return HTNOperatorStatus.Continuing;

        _stunSystem.TryStanding(owner);
        return HTNOperatorStatus.Continuing;
    }
}
