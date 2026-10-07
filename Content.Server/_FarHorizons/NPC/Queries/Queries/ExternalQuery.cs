using Content.Server.NPC;
using Content.Server.NPC.Queries.Queries;

namespace Content.Server._FarHorizons.NPC.Queries.Queries;

// The wizden code has a giant ass switch statement that declares code for every single UtilityConsideration in one function. I'd rather not do that and move all AI code we add away from one giant function and into something generic
public abstract partial class ExternalQuery : UtilityQuery
{
    public virtual List<EntityUid> GetEntities(NPCBlackboard blackboard, IEntityManager entMan) 
        => throw new NotImplementedException();
}