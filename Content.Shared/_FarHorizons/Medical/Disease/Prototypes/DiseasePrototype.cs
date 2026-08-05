using Content.Shared.Metabolism;
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
    /// Optional incubation time in seconds before symptoms/spread begin after infection.
    /// </summary>
    [DataField]
    public float IncubationSeconds { get; private set; }

    /// <summary>
    /// Symptoms for this disease.
    /// </summary>
    [DataField]
    public List<SymptomEntry> Symptoms { get; private set; } = new List<SymptomEntry>();

    /// <summary>
    /// Optional list of metabolizers that are affected by this disease.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<MetabolizerTypePrototype>>? MetabolizerTypes;

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
    public int Speed { get; set; } = 0;

    [DataField]
    public int Transmittable { get; set; } = 0;
}