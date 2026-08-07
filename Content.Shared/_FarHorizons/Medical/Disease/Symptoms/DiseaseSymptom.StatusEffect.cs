using Content.Shared.EntityEffects;
using Content.Shared._FarHorizons.Medical.Disease.Prototypes;
using Content.Shared._FarHorizons.Medical.Disease.Systems;
using Content.Shared._FarHorizons.Medical.Disease.Components;
using Robust.Shared.Timing;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Shared._FarHorizons.Medical.Disease.Symptoms;

[DataDefinition]
public sealed partial class SymptomStatusEffect : SymptomBehavior
{
    /// <summary>
    /// List of effects to execute on symptom trigger. Supports any <see cref="EntityEffect"/>.
    /// </summary>
    [DataField(required: true)]
    public EntityEffect[] Effects { get; private set; } = [];
}

public sealed partial class SymptomStatusEffect
{
    [Dependency] private readonly SharedEntityEffectsSystem _effects = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    /// <summary>
    /// Executes the status effects.
    /// </summary>
    public override void OnSymptom(Entity<DiseaseCarrierComponent> entity, DiseaseData disease, StageData stage,  DiseaseSymptomPrototype symptom)
    {
        if (Effects.Length == 0)
            return;

        foreach(var condition in Conditions)
            if(!condition.Check(entity, disease, stage))
                return;

        var probOverride = disease.Symptoms.FirstOrDefault(p => p.Symptom.Id == symptom.ID);
        if(probOverride != null && probOverride.Probability.TryGetValue(stage.Stage, out var stageProb))
            Probability = stageProb;
            
        var seed = SharedRandomExtensions.HashCodeCombine((int)_timing.CurTick.Value, stage.AdvanceStageAt.Microseconds, stage.Stage, symptom.ID.GetHashCode());
        var rand = new System.Random(seed);
        if(!rand.Prob(Probability))
            return;

        _effects.ApplyEffects(entity, Effects);
    }
}
