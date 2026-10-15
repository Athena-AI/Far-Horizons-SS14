using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Content.Shared.Whitelist;

namespace Content.Server._FarHorizons.NPC.HTN.Preconditions;

public sealed partial class FlockMergePrecondition : HTNPrecondition
{
    [Dependency] private IEntityManager _entMan = default!;

    [DataField(required: true)]
    public string TargetKey = default!;

    [DataField]
    public float SearchRadius = 5f;

    [DataField]
    public EntityWhitelist? Whitelist;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        // Only leaders merge; followers just follow.
        if (FlockHelper.FindCurrentLeader(_entMan, owner, TargetKey) != owner)
            return false;

        var best = FlockHelper.FindBestLeader(_entMan, owner, TargetKey, SearchRadius, Whitelist);
        return best != null && best != owner;
    }
}