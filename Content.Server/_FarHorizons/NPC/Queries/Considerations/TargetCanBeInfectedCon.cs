using Content.Server.NPC;
using Content.Server.Zombies;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Zombies;

namespace Content.Server._FarHorizons.NPC.Queries.Considerations;

/// <summary>
/// Returns 1f when target can be infected or 0f
/// </summary>
public sealed partial class TargetCanBeInfectedCon : ExternalConsideration
{
    public const SlotFlags ProtectiveSlots =
        SlotFlags.FEET |
        SlotFlags.HEAD |
        SlotFlags.EYES |
        SlotFlags.GLOVES |
        SlotFlags.MASK |
        SlotFlags.NECK |
        SlotFlags.INNERCLOTHING |
        SlotFlags.OUTERCLOTHING;

    public override float GetScore(NPCBlackboard blackboard, EntityUid targetUid, IEntityManager entMan)
    {
        var mobstate = entMan.System<MobStateSystem>();
        if(mobstate.IsDead(targetUid) || mobstate.IsCritical(targetUid))
        {
            var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
            if(entMan.HasComponent<NonSpreaderZombieComponent>(owner))
                return 0f;
                
            var ev = new ZombificationResistanceQueryEvent(ProtectiveSlots);
            entMan.EventBus.RaiseLocalEvent(targetUid, ev);
            
            if(entMan.HasComponent<ZombieImmuneComponent>(targetUid) || ev.TotalCoefficient <= 0f)
                return 0f;
        }
        return 1f;
    }
}
