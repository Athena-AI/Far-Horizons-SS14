using Robust.Shared.Audio;
using Robust.Shared.Map;

namespace Content.Shared._FarHorizons.Audio;

[ByRefEvent]
public readonly record struct NoiseEmittedEvent(EntityUid Source, MapCoordinates Position, AudioParams? Params);