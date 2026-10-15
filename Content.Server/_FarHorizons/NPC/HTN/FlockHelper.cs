using Content.Server.NPC.HTN;
using Content.Shared.Mobs.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Map;

namespace Content.Server._FarHorizons.NPC.HTN;

public static class FlockHelper
{
    /// <summary>
    /// Follows the FollowTarget chain from <paramref name="start"/> to its root leader
    /// (an entity whose key points at itself). Null if the chain is broken, dead, or has no key.
    /// </summary>
    public static EntityUid? FindCurrentLeader(IEntityManager entMan, EntityUid start, string key)
    {
        var mobState = entMan.System<MobStateSystem>();
        var current = start;

        for (var i = 0; i < 4; i++) // depth cap also guards against cycles
        {
            if (!entMan.EntityExists(current) || mobState.IsIncapacitated(current))
                return null;

            if (!entMan.TryGetComponent(current, out HTNComponent? htn) ||
                !htn.Blackboard.TryGetValue<EntityCoordinates>(key, out var next, entMan))
                return null;

            if (next.EntityId == current)
                return current;

            current = next.EntityId;
        }

        return null;
    }

    /// <summary>
    /// Counts nearby flock members per root leader (including the owner) and returns
    /// the root with the most followers. Ties go to the lowest EntityUid so every
    /// zombie agrees on the winner.
    /// </summary>
    public static EntityUid? FindBestLeader(IEntityManager entMan, EntityUid owner, string key,
        float radius, EntityWhitelist? whitelist)
    {
        var lookup = entMan.System<EntityLookupSystem>();
        var whitelistSys = entMan.System<EntityWhitelistSystem>();
        var counts = new Dictionary<EntityUid, int>();

        void Vote(EntityUid member)
        {
            if (FindCurrentLeader(entMan, member, key) is { } root)
                counts[root] = counts.GetValueOrDefault(root) + 1;
        }

        Vote(owner); // no-op if the owner has no key yet

        var coords = entMan.GetComponent<TransformComponent>(owner).Coordinates;
        foreach (var member in lookup.GetEntitiesInRange<HTNComponent>(coords, radius))
        {
            if (member.Owner == owner)
                continue;

            if (whitelist != null && !whitelistSys.IsWhitelistPass(whitelist, member.Owner))
                continue;

            Vote(member.Owner);
        }

        EntityUid? best = null;
        var bestCount = 0;
        foreach (var (leader, count) in counts)
        {
            if (count > bestCount || (count == bestCount && best != null && leader.Id < best.Value.Id))
            {
                best = leader;
                bestCount = count;
            }
        }

        return best;
    }
}