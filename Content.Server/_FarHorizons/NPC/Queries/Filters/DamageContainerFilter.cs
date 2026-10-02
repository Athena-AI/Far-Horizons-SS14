using Content.Server.NPC;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.NPC.Queries.Filters;

public sealed partial class DamageContainerFilter : ExternalFilter
{
    /// <summary>
    /// Damage Containers to filter for.
    /// </summary>
    [DataField(required: true)]
    public HashSet<ProtoId<DamageContainerPrototype>> DamageContainers = new();

    [DataField]
    public bool Invert;

    public override List<EntityUid> GetEntities(NPCBlackboard blackboard, HashSet<EntityUid> entities, IEntityManager entMan)
    {
        _entityList.Clear();

        foreach(var ent in entities)
        {
            var matches = entMan.TryGetComponent<DamageableComponent>(ent, out var damageComp)
                          && damageComp.DamageContainerID != null
                          && !DamageContainers.Contains(damageComp.DamageContainerID.Value);

            if (matches == Invert)
                _entityList.Add(ent);
        }

        return _entityList;
    }
}
