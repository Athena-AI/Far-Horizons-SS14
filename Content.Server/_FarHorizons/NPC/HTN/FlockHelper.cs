using Content.Server.NPC.HTN;
using Content.Shared.Mobs.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Map;

namespace Content.Server._FarHorizons.NPC.HTN;

public static class FlockHelper
{
    private const int MaxDepth = 16;

    /// <summary>
    /// Follows the FollowTarget chain from <paramref name="start"/> to its root leader
    /// (an entity whose key points at itself). Null if the chain is broken, dead, or has no key.
    /// </summary>
    public static EntityUid? FindCurrentLeader(IEntityManager entMan, EntityUid start, string key)
    {
        var mobState = entMan.System<MobStateSystem>();
        var current = start;

        for (var i = 0; i < MaxDepth; i++) // depth cap also guards against cycles
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
    /// Returns the lowest-EntityUid living flock member in range, counting the owner.
    /// Every member in the same neighborhood picks the same leader.
    /// </summary>
    public static EntityUid FindBestLeader(IEntityManager entMan, EntityUid owner, string key,
        float radius, EntityWhitelist? whitelist)
    {
        var lookup = entMan.System<EntityLookupSystem>();
        var whitelistSys = entMan.System<EntityWhitelistSystem>();
        var mobState = entMan.System<MobStateSystem>();

        var best = owner;
        var coords = entMan.GetComponent<TransformComponent>(owner).Coordinates;

        foreach (var member in lookup.GetEntitiesInRange<HTNComponent>(coords, radius))
        {
            if (member.Owner.Id >= best.Id)
                continue;

            if (whitelist != null && !whitelistSys.IsWhitelistPass(whitelist, member.Owner))
                continue;

            if (mobState.IsIncapacitated(member.Owner))
                continue;

            best = member.Owner;
        }

        return FindCurrentLeader(entMan, best, key) ?? best;
    }
}