using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared._FarHorizons.Slasher.Components;
using Content.Shared._FarHorizons.Slasher.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Slasher.Systems;

public sealed class SlasherSystem : SharedSlasherSystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PolymorphableComponent, IncorporealizeActionEvent>(OnIncorporealizeActionEvent);
        SubscribeLocalEvent<PolymorphedEntityComponent, RevertIncorporealizeActionEvent>(OnRevertIncorporealizeActionEvent);
    }

    private void OnIncorporealizeActionEvent(Entity<PolymorphableComponent> ent, ref IncorporealizeActionEvent args)
    {
        if (!_proto.Resolve(args.ProtoId, out var prototype) || args.Handled)
            return;

        _polymorph.PolymorphEntity(ent, prototype.ID);

        args.Handled = true;
    }

    private void OnRevertIncorporealizeActionEvent(Entity<PolymorphedEntityComponent> ent, ref RevertIncorporealizeActionEvent args)
        => _polymorph.Revert((ent, ent));
}