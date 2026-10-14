using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Mobs.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Map;

namespace Content.Server._FarHorizons.NPC.HTN.PrimitiveTasks.Operators;

public sealed partial class AssignLeaderOperator : HTNOperator
{
    [Dependency] private IEntityManager _entMan = default!;

    [DataField(required: true)]
    public string TargetKey = default!;

    [DataField]
    public float SearchRadius = 30f;

    /// <summary>Which nearby entities count as swarm members. Null = any HTN NPC.</summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(
        NPCBlackboard blackboard, CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        var leader = FindLeader(owner) ?? owner; 

        return (true, new Dictionary<string, object>
        {
            { TargetKey, new EntityCoordinates(leader, Vector2.Zero) }
        });
    }

    public override void Startup(NPCBlackboard blackboard)
    {
        base.Startup(blackboard);

        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        var leader = FindLeader(owner) ?? owner;

        blackboard.SetValue(TargetKey, new EntityCoordinates(leader, Vector2.Zero));
    }

    private EntityUid? FindLeader(EntityUid owner)
    {
        var lookup = _entMan.System<EntityLookupSystem>();
        var mobState = _entMan.System<MobStateSystem>();
        var whitelist = _entMan.System<EntityWhitelistSystem>();

        var votes = new Dictionary<EntityUid, int>();

        foreach (var member in lookup.GetEntitiesInRange<HTNComponent>(
                     _entMan.GetComponent<TransformComponent>(owner).Coordinates, SearchRadius))
        {
            if (member.Owner == owner)
                continue;

            if (Whitelist != null && !whitelist.IsWhitelistPass(Whitelist, member.Owner))
                continue;

            if (!member.Comp.Blackboard.TryGetValue<EntityCoordinates>(TargetKey, out var target, _entMan))
                continue;

            var leader = target.EntityId;
            if (!_entMan.EntityExists(leader) || mobState.IsIncapacitated(leader))
                continue;

            votes[leader] = votes.GetValueOrDefault(leader) + 1;
        }

        EntityUid? best = null;
        var bestVotes = 0;
        foreach (var (leader, count) in votes)
        {
            if (count <= bestVotes)
                continue;
            best = leader;
            bestVotes = count;
        }

        return best;
    }
}