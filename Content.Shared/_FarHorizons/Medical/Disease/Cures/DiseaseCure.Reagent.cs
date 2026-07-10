using Content.Shared._FarHorizons.Medical.Disease.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared._FarHorizons.Medical.Disease.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry.Components;

namespace Content.Shared._FarHorizons.Medical.Disease.Cures;

[Serializable, NetSerializable]
public sealed partial class CureReagent : CureStep
{
    [DataField]
    public FixedPoint2 Min = FixedPoint2.Zero;

    [DataField]
    public FixedPoint2 Max = FixedPoint2.MaxValue;

    [DataField(required: true)]
    public ProtoId<ReagentPrototype> Reagent;

    /// <summary>
    /// Cures the disease after the infection has lasted a configured duration.
    /// </summary>
    public override bool OnCure(EntityUid uid, DiseaseData disease)
    {
        var _entityManager = IoCManager.Resolve<IEntityManager>();

        if(!_entityManager.TryGetComponent<SolutionComponent>(uid, out var solution))
            return false;

        var soln = solution.Solution;
        var quant = soln.GetTotalPrototypeQuantity(Reagent);

         return quant >= Min && quant <= Max;
    }

    public override IEnumerable<string> BuildDiagnoserLines(IPrototypeManager prototypes)
    {
        yield return "";
    }
}
