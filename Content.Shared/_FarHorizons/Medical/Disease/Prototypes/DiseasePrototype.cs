using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Medical.Disease.Prototypes;

/// <summary>
/// Describes information about a specific disease.
/// </summary>
[Prototype]
public sealed partial class DiseasePrototype : IPrototype
{
    /// <summary>
    /// ID of the disease.
    /// </summary>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Displayed name of the disease.
    /// </summary>
    [DataField(required: true)]
    public string Name { get; private set; } = default!;

    /// <summary>
    /// Displayed description of the disease.
    /// </summary>
    [DataField("desc", required: true)]
    public string Description { get; private set; } = default!;

    /// <summary>
    /// Disease icon prototype to show on HUDs.
    /// </summary>
    [DataField]
    public ProtoId<DiseaseIconPrototype>? IconDisease { get; private set; } = "DiseaseIconIll";

    /// <summary>
    /// Default immunity strength granted after curing this disease (0-1).
    /// </summary>
    [DataField]
    public float PostCureImmunity { get; private set; } = 1.0f;

    /// <summary>
    /// Optional incubation time in seconds before symptoms/spread begin after infection.
    /// </summary>
    [DataField]
    public float IncubationSeconds { get; private set; }

    /// <summary>
    /// Per-disease permeability multiplier (0-1) applied to PPE/internals effectiveness.
    /// Values > 1 reduce protection; values < 1 increase protection.
    /// </summary>
    [DataField]
    public float PermeabilityMod { get; private set; } = 1.0f;

    /// <summary>
    /// Base per-contact infection probability for this disease (0-1). Used when two entities make contact.
    /// </summary>
    [DataField]
    public float ContactInfect { get; private set; } = 0.025f;

    /// <summary>
    /// Amount of residue intensity deposited when a carrier with this disease contacts a surface.
    /// Expressed as (0-1) fraction added to per-disease residue intensity.
    /// </summary>
    [DataField]
    public float ContactDeposit { get; private set; } = 0.2f;

    /// <summary>
    /// Base per-target airborne infection probability (0-1) before PPE adjustments.
    /// </summary>
    [DataField]
    public float AirborneInfect { get; private set; } = 0.025f;

    /// <summary>
    /// Airborne infection radius in world units, used when <see cref="SpreadPath"/> contains Airborne.
    /// </summary>
    [DataField]
    public float AirborneRange { get; private set; } = 3f;

    /// <summary>
    /// Time Configuration for each stage this also handles max stage level. So 3 entry means a disease have 4 stages counting stage 0.
    /// </summary>
    [DataField(required: true)]
    public List<float> Stages { get; private set; } = [];

    /// <summary>
    /// Symptoms for this disease.
    /// </summary>
    [DataField]
    public List<SymptomEntry> Symptoms { get; private set; } = [];

    /// <summary>
    /// Optional list of cure steps for the disease. Each entry is a specific cure action.
    /// </summary>
    [DataField]
    public List<CureStep> CureSteps { get; private set; } = [];
}

[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class SymptomEntry
{
    /// <summary>
    /// Symptom prototype ID to trigger.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<DiseaseSymptomPrototype> Symptom { get; private set; }
    
    /// <summary>
    /// At what stage levels does this symptom present?
    /// </summary>
    [DataField]
    public List<int> Stages { get; private set; } = new() { 0 };
    
    /// <summary>
    /// Per stage level probability overwrites the symptom level probability if > -1.
    /// </summary>
    [DataField]
    public Dictionary<int, float> Probability { get; private set; } = new()
    {
        { 0, -1f } // Int: Stage, Float: Chance
    };
}

[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class DiseaseStats
{
    [DataField]
    public int Stealth { get; set; } = 0;

    [DataField]
    public int Resistance { get; set; } = 0;

    [DataField]
    public int StageSpeed { get; set; } = 0;

    [DataField]
    public int Transmittable { get; set; } = 0;
}