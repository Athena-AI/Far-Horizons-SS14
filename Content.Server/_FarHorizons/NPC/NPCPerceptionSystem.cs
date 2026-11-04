using Content.Server.NPC.HTN;
using Content.Shared._FarHorizons.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._FarHorizons.NPC;

public sealed partial class NPCPerceptionSystem : EntitySystem
{
    private TimeSpan _nextUpdate;
    private readonly float _cooldown = 1f;
    
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private TransformSystem _transform = default!;
    [Dependency] private SharedAudioOcclusionSystem _occlusion = default!;
    [Dependency] private SharedMapSystem _map = default!;

    [SubscribeLocalEvent]
    private void OnNoiseEmitted(ref NoiseEmittedEvent args)
    {
        if (args.Params == null || _nextUpdate > _timing.CurTime)
            return;

        var noisePos = args.Position;
        if (!_map.TryFindGridAt(noisePos, out var gridUid, out var grid))
            return;
        
        _nextUpdate = _timing.CurTime + TimeSpan.FromSeconds(_cooldown);
        var target = new EntityCoordinates(gridUid, _map.WorldToLocal(gridUid, grid, noisePos.Position));

        foreach (var npc in _lookup.GetEntitiesInRange<HTNComponent>(noisePos, args.Params.Value.MaxDistance, LookupFlags.Dynamic))
        {
            if (npc.Owner == args.Source)
                continue;

            if (!npc.Comp.Blackboard.TryGetValue("HearingThreshold", out float hearingThreshold, EntityManager))
                continue;

            var npcPos = _transform.GetMapCoordinates(npc.Owner);
            var delta = noisePos.Position - npcPos.Position;

            var occlusion = _occlusion.CalculateOcclusion(
                npcPos, delta, delta.Length(), args.Params.Value.MaxDistance, args.Source);

            if (occlusion >= hearingThreshold)
                continue;
            
            npc.Comp.Blackboard.SetValue("NoisePos", target);
        }
    }
}   