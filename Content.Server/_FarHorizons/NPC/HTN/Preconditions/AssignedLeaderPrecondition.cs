using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;

namespace Content.Server._FarHorizons.NPC.HTN.Preconditions;

public sealed partial class AssignedLeaderPrecondition : HTNPrecondition
{
    [Dependency] private IEntityManager _entMan = default!;

    [DataField("targetKey", required: true)] 
    public string TargetKey = default!;
    [DataField]
    public bool Invert;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var mobState = _entMan.System<MobStateSystem>();

        var hasLeader = blackboard.TryGetValue<EntityCoordinates>(TargetKey, out var target, _entMan)
                        && target.EntityId.IsValid()
                        && !mobState.IsIncapacitated(target.EntityId);

        return hasLeader ^ Invert;
    }
}