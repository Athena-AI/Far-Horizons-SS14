using Content.Shared._FarHorizons.Audio;

namespace Content.Server._FarHorizons.NPC;

public sealed partial class NPCPerceptionSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnNoiseEmitted(ref NoiseEmittedEvent args)
    {
        var source = args.Source;
    }
}   