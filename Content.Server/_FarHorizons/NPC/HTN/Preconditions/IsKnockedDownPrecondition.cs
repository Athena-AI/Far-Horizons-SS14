using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Content.Shared.Stunnable;
using Content.Shared.Traits.Assorted;

namespace Content.Server._FarHorizons.NPC.HTN.Preconditions;

public sealed partial class IsKnockedDownPrecondition : HTNPrecondition
{
    [Dependency] private IEntityManager _entMan = default!;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (_entMan.HasComponent<LegsParalyzedComponent>(owner))
            return false;

        var isKnockedDown = _entMan.HasComponent<CrawlerComponent>(owner);
        return isKnockedDown;
    }
}