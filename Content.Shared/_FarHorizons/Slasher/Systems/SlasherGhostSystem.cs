using Content.Shared._FarHorizons.Slasher.Components;
using Content.Shared.Actions;
using Content.Shared.Eye;
using Robust.Server.GameObjects;

namespace Content.Shared._FarHorizons.Slasher.Systems;

public sealed class SlasherGhostSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedVisibilitySystem _visibility = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherGhostComponent, MapInitEvent>(OnMapInit, after:[typeof(VisibilitySystem)]);
        SubscribeLocalEvent<SlasherGhostComponent, ComponentStartup>(OnGhostStartup);
        SubscribeLocalEvent<SlasherGhostComponent, ComponentShutdown>(OnGhostShutdown);
    }

    private void OnMapInit(Entity<SlasherGhostComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.RevertIncorporealizeActionEntity, ent.Comp.RevertIncorporealizeAction);
        Dirty(ent);
    }

    private void OnGhostStartup(EntityUid uid, SlasherGhostComponent component, ComponentStartup args)
    {
        var visibility = EnsureComp<VisibilityComponent>(uid);

        _visibility.AddLayer((uid, visibility), (int) VisibilityFlags.Ghost, false);
        _visibility.RemoveLayer((uid, visibility), (int) VisibilityFlags.Normal, false);
        _visibility.RefreshVisibility(uid, visibilityComponent: visibility);

        _eye.RefreshVisibilityMask(uid);
    }

    private void OnGhostShutdown(EntityUid uid, SlasherGhostComponent component, ComponentShutdown args)
    {
        if (Terminating(uid))
            return;

        if (TryComp(uid, out VisibilityComponent? visibility))
        {
            _visibility.RemoveLayer((uid, visibility), (int) VisibilityFlags.Ghost, false);
            _visibility.AddLayer((uid, visibility), (int) VisibilityFlags.Normal, false);
            _visibility.RefreshVisibility(uid, visibilityComponent: visibility);
        }

        _eye.RefreshVisibilityMask(uid);
    }

}
