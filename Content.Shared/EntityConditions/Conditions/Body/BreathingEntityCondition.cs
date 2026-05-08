using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.EntityConditions.Conditions.Body;

/// <inheritdoc cref="EntityCondition"/>
[NetSerializable, Serializable] //Far Horizons
public sealed partial class BreathingCondition : EntityConditionBase<BreathingCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype) =>
        Loc.GetString("entity-condition-guidebook-breathing", ("isBreathing", !Inverted));
}
