
using System.Threading;
using System.Threading.Tasks;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;

namespace Content.Server._FarHorizons.NPC.HTN.PrimitiveTasks.Operators;
public sealed partial class RemoveKeyOperator : HTNOperator
{
    /// <summary>
    /// Blackboard key to remove.
    /// </summary>
    [DataField(required: true)] public string Key = default!;

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(
        NPCBlackboard blackboard, CancellationToken cancelToken) 
        => (true, null);

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        blackboard.Remove<object>(Key);
        return HTNOperatorStatus.Finished;
    }
}