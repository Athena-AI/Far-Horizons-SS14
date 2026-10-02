using Content.Server.NPC;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.NPC.Queries.Filters;

public sealed partial class HasDamageTypeFilter : ExternalFilter
{
    /// <summary>
    /// Damage group types to filter for.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<DamageGroupPrototype>> DamageGroups = new();

    /// <summary>
    /// Damage type types to filter for.
    /// </summary>
    [DataField]
    public List<ProtoId<DamageTypePrototype>> DamageTypes = new();

    [DataField]
    public bool Invert;

    public override List<EntityUid> GetEntities(NPCBlackboard blackboard, HashSet<EntityUid> entities, IEntityManager entMan)
    {
        var damageable = entMan.System<DamageableSystem>();
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        _entityList.Clear();

        foreach (var ent in entities)
        {
            var matches = false;

            if (entMan.HasComponent<DamageableComponent>(ent))
            {
                var damage = damageable.GetAllDamage(ent);
                matches = HasMatchingDamage(damage, protoMan);
            }

            if (matches == Invert)
                _entityList.Add(ent);
        }

        return _entityList;
    }

    private bool HasMatchingDamage(DamageSpecifier damage, IPrototypeManager protoMan)
    {
        foreach (var type in DamageTypes)
        {
            if (damage.DamageDict.TryGetValue(type, out var value) && value > FixedPoint2.Zero)
                return true;
        }

        if (DamageGroups.Count == 0)
            return false;

        var perGroup = damage.GetDamagePerGroup(protoMan);
        foreach (var group in DamageGroups)
        {
            if (perGroup.TryGetValue(group, out var total) && total > FixedPoint2.Zero)
                return true;
        }

        return false;
    }
}
