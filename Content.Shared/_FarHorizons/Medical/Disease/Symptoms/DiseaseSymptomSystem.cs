using Content.Shared.Mobs.Systems;
using Content.Shared._FarHorizons.Medical.Disease.Systems;
using Content.Shared._FarHorizons.Medical.Disease.Components;
using Content.Shared._FarHorizons.Medical.Disease.Prototypes;
using Content.Shared.Random.Helpers;
using Robust.Shared.Timing;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared._Starlight.Abstract.Extensions;

namespace Content.Shared._FarHorizons.Medical.Disease.Symptoms;

/// <summary>
/// Encapsulates symptom-side effects and secondary spread mechanics for diseases.
/// </summary>
public sealed partial class SharedDiseaseSymptomSystem : EntitySystem
{
    [Dependency] private IEntitySystemManager _entitySystemManager = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private DiseaseAirborneSystem _airborneDisease = default!;

    /// <summary>
    /// Executes the side-effects for a triggered symptom on a carrier.
    /// </summary>
    public void TriggerSymptom(Entity<DiseaseCarrierComponent> ent, DiseaseData disease, StageData stage, DiseaseSymptomPrototype symptom)
    {
        // Skip this symptom when the carrier is dead.
        if (symptom.OnlyAlive && _mobState.IsDead(ent.Owner))
            return;

        var deps = _entitySystemManager.DependencyCollection;

        // Local helper to execute a single symptom behavior with dependencies injected.
        void RunSingleBehavior(SymptomBehavior behavior)
        {
            deps.InjectDependencies(behavior);
            if(behavior.TryTriggerSymptom(ent, disease, stage, symptom))
                ApplyAirborneBurst(symptom, ent, disease);
                
        }

        if (symptom.SingleBehavior && symptom.Behaviors.Count > 0)
        {
            // Run exactly one random behavior.
            // TODO: Replace with RandomPredicted once the engine PR is merged
            var seed = SharedRandomExtensions.HashCodeCombine(_timing.CurTime.Microseconds, GetNetEntity(ent).Id, symptom.Behaviors.Count);
            var behavior = _random.PickPredicted(_timing, symptom.Behaviors, seed);
            RunSingleBehavior(behavior);
        }
        else
        {
            // Run all behavior.
            foreach (var behavior in symptom.Behaviors)
            {
                RunSingleBehavior(behavior);
            }
        }
    }

    /// <summary>
    /// Applies a single-shot airborne spread burst if configured.
    /// </summary>
    public void ApplyAirborneBurst(DiseaseSymptomPrototype symptom, Entity<DiseaseCarrierComponent> ent, DiseaseData disease)
    {
        var cfg = symptom.AirborneBurst;

        if ((disease.SpreadPath & DiseaseSpreadPath.Airborne) == 0 || (cfg.ChanceMultiplier == 0 && cfg.RangeMultiplier == 0))
            return;

        var range = disease.AirborneRange * MathF.Max(0.1f, cfg.RangeMultiplier);
        var mult = MathF.Max(0f, cfg.ChanceMultiplier);
        _airborneDisease.TryAirborneSpread(ent.Owner, disease, overrideRange: range, chanceMultiplier: mult);
    }
}
