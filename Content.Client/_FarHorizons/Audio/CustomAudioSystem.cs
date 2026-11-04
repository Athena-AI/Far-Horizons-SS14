using System.Collections.Concurrent;
using System.Numerics;
using Content.Shared._FarHorizons.Audio;
using Content.Shared._FarHorizons.CCVar;
using Robust.Client.Audio;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client._FarHorizons.Audio.CustomAudio;

public sealed partial class CustomAudioSystem : EntitySystem
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private SharedAudioOcclusionSystem _occlusion = default!;
    [Dependency] private IConfigurationManager _cfgManager = default!;
    [Dependency] private IGameTiming _timing = default!;

    private static readonly TimeSpan _smoothingTimeout = TimeSpan.FromSeconds(0.25);

    private struct SmoothState
    {
        public float Value;
        public TimeSpan LastUpdate;
    }

    private readonly ConcurrentDictionary<EntityUid, SmoothState> _smoothedOcclusion = new();

    // CVAR Controlled Values
    private float _occlusionSmoothingRate;
    private float _maxRayLength;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_cfgManager, CVars.AudioRaycastLength, OnRaycastLengthChanged, true);
        Subs.CVar(_cfgManager, FHCCVars.AudioOcclusionSmoothingRate, OnSmoothingRateChanged, true);
        _audio.GetOcclusionOverride += GetOcclusion;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _audio.GetOcclusionOverride -= GetOcclusion;
    }

    private void OnRaycastLengthChanged(float value)
        => _maxRayLength = value;

    [SubscribeLocalEvent]
    private void OnEntityTerminating(ref EntityTerminatingEvent ev)
        => _smoothedOcclusion.TryRemove(ev.Entity, out _);

    private void OnSmoothingRateChanged(float value)
        => _occlusionSmoothingRate = value;

    private float GetOcclusion(MapCoordinates listener, Vector2 delta, float distance, EntityUid? ignoredEnt)
        => SmoothOcclusion(ignoredEnt,
            _occlusion.CalculateOcclusion(listener, delta, distance, _maxRayLength, ignoredEnt));

    private float SmoothOcclusion(EntityUid? source, float target)
    {
        if (source is not { } key)
            return target;

        var now = _timing.RealTime;

        if (!_smoothedOcclusion.TryGetValue(key, out var state)
            || now - state.LastUpdate > _smoothingTimeout)
        {
            _smoothedOcclusion[key] = new SmoothState { Value = target, LastUpdate = now };
            return target;
        }

        var dt = (float) (now - state.LastUpdate).TotalSeconds;

        if (dt <= 0f)
            return state.Value;

        var maxDelta = _occlusionSmoothingRate * dt;
        var diff = target - state.Value;

        state.Value = MathF.Abs(diff) <= maxDelta
            ? target
            : state.Value + (MathF.Sign(diff) * maxDelta);
        state.LastUpdate = now;

        _smoothedOcclusion[key] = state;
        return state.Value;
    }
}