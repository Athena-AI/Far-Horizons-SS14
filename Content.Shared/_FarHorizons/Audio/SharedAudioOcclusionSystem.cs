using System.Numerics;
using Content.Shared._FarHorizons.CCVar;
using Content.Shared.Doors.Components;
using Content.Shared.Physics;
using Content.Shared.Tools.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._FarHorizons.Audio;

public sealed partial class SharedAudioOcclusionSystem : EntitySystem
{
    [Dependency] private IConfigurationManager _cfgManager = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;

    private const float OcclusionReferenceThickness = 1f;
    private float _muffleDecayConstant;
    private float _maxOcclusion;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_cfgManager, FHCCVars.AudioOcclusionMuffleDecay, v => _muffleDecayConstant = v, true);
        Subs.CVar(_cfgManager, FHCCVars.AudioOcclusionMax, v => _maxOcclusion = v, true);
    }

    /// <summary>
    /// The raw unsmoothed calculation.
    /// </summary>
    public float CalculateOcclusion(
        MapCoordinates listener,
        Vector2 delta,
        float distance,
        float maxRayLength,
        EntityUid? ignoredEnt)
    {
        if (distance <= 0.1f)
            return 0f;

        var rayLength = MathF.Min(distance, maxRayLength);
        var direction = delta / distance;
        var ray = new CollisionRay(listener.Position, direction, (int) (CollisionGroup.Opaque | CollisionGroup.Impassable));

        var results = _physics.IntersectRay(listener.MapId, ray, rayLength, ignoredEnt, returnOnFirstHit: false);

        var totalOcclusion = 0f;
        var seen = new HashSet<EntityUid>();

        foreach (var result in results)
        {
            var hit = result.HitEntity;

            if (!seen.Add(hit))
                continue;

            if (!TryComp<StructureOcclusionComponent>(hit, out var occlusionComp))
                continue;

            if (!occlusionComp.DoesOcclusionWorkWhenOpen
                && TryComp<DoorComponent>(hit, out var doorComp)
                && doorComp.State is DoorState.Open or DoorState.Opening)
                continue;

            var aabb = _lookup.GetWorldAABB(hit);
            if (!TryGetRayAabbPenetration(listener.Position, direction, aabb, rayLength, out var penetration))
                continue;

            var occlusion = occlusionComp.OcclusionAmount
                * Math.Clamp(penetration / OcclusionReferenceThickness, 0f, 1f);

            if (TryComp<WeldableComponent>(hit, out var weldable) && weldable.IsWelded)
                occlusion *= occlusionComp.WeldedOcclusionModifier;

            totalOcclusion += occlusion;
        }

        return _maxOcclusion * (1f - MathF.Exp(-_muffleDecayConstant * totalOcclusion));
    }

    private static bool TryGetRayAabbPenetration(Vector2 origin, Vector2 dir, Box2 aabb, float rayLength, out float penetration)
    {
        penetration = 0f;

        var tMin = 0f;
        var tMax = rayLength;

        for (var axis = 0; axis < 2; axis++)
        {
            var o = axis == 0 ? origin.X : origin.Y;
            var d = axis == 0 ? dir.X : dir.Y;
            var min = axis == 0 ? aabb.Left : aabb.Bottom;
            var max = axis == 0 ? aabb.Right : aabb.Top;

            if (MathF.Abs(d) < 1e-6f)
            {
                if (o < min || o > max)
                    return false;
                continue;
            }

            var invD = 1f / d;
            var t1 = (min - o) * invD;
            var t2 = (max - o) * invD;

            if (t1 > t2)
                (t1, t2) = (t2, t1);

            tMin = MathF.Max(tMin, t1);
            tMax = MathF.Min(tMax, t2);

            if (tMin > tMax)
                return false;
        }

        penetration = MathF.Max(0f, tMax - tMin);
        return penetration > 0f;
    }
}