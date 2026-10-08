using Content.Server.NPC;
using Content.Shared.Mobs.Systems;
using Content.Shared.Zombies;

namespace Content.Server._FarHorizons.NPC.Queries.Considerations;

/// <summary>
/// Returns 1f when target can be infected or 0f
/// </summary>
public sealed partial class TargetCanBeInfectedCon : ExternalConsideration
{
    public override float GetScore(NPCBlackboard blackboard, EntityUid targetUid, IEntityManager entMan)
    {
        var mobstate = entMan.System<MobStateSystem>();
        if((mobstate.IsDead(targetUid) || mobstate.IsCritical(targetUid)) && entMan.HasComponent<ZombieImmuneComponent>(targetUid))
            return 0f;
        return 1f;
    }
}
