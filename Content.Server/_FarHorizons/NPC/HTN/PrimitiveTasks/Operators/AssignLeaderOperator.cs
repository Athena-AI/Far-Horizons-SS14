using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Whitelist;
using Robust.Shared.Map;

namespace Content.Server._FarHorizons.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class AssignLeaderOperator : HTNOperator
{
    [Dependency] private IEntityManager _entMan = default!;

    [DataField(required: true)]
    public string TargetKey = default!;

    [DataField]
    public float SearchRadius = 5f;

    [DataField]
    public EntityWhitelist? Whitelist;

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(
        NPCBlackboard blackboard, CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var leader = FlockHelper.FindBestLeader(_entMan, owner, TargetKey, SearchRadius, Whitelist) ?? owner;

        return (true, new Dictionary<string, object>
        {
            { TargetKey, new EntityCoordinates(leader, Vector2.Zero) }
        });
    }

    public override void Startup(NPCBlackboard blackboard)
    {
        base.Startup(blackboard);

        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var leader = FlockHelper.FindBestLeader(_entMan, owner, TargetKey, SearchRadius, Whitelist) ?? owner;

        blackboard.SetValue(TargetKey, new EntityCoordinates(leader, Vector2.Zero));
    }
}