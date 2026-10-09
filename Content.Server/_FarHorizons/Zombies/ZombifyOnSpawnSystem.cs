using Content.Server.Humanoid.Systems;
using Content.Server.Zombies;

namespace Content.Server._FarHorizons.Zombies;

public sealed partial class ZombifyOnSpawn : EntitySystem
{
    [Dependency] private ZombieSystem _zombie = default!;

    [SubscribeLocalEvent (after:[typeof(RandomHumanoidAppearanceSystem)])]
    private void OnMapInit(Entity<ZombifyOnSpawnComponent> ent, ref MapInitEvent _)
    {
        if(!ent.Comp.AllowSpreading)
            EnsureComp<NonSpreaderZombieComponent>(ent);
        _zombie.ZombifyEntity(ent);
    } 
}