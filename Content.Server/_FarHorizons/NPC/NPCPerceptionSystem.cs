using Content.Server.NPC.HTN;
using Content.Shared._FarHorizons.Audio;
using Robust.Shared.Timing;

namespace Content.Server._FarHorizons.NPC;

public sealed partial class NPCPerceptionSystem : EntitySystem
{
    private TimeSpan _nextUpdate;
    private readonly float _cooldown = 1f;
    
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;

    [SubscribeLocalEvent]
    private void OnNoiseEmitted(ref NoiseEmittedEvent args)
    {
        if(args.Params == null || _nextUpdate > _timing.CurTime)
            return;

        _nextUpdate+=TimeSpan.FromSeconds(_cooldown);

        var npcs = _lookup.GetEntitiesInRange<HTNComponent>(args.Position, args.Params.Value.MaxDistance, LookupFlags.Dynamic);
    }
}   