
using Content.Shared._FarHorizons.PowerArmor;
using Content.Shared.Destructible;

namespace Content.Server._FarHorizons.PowerArmor;

public sealed partial class PowerArmorSystem : SharedPowerArmorSystem
{
    public override void Initialize()
    { 
        base.Initialize();
        SubscribeLocalEvent<PowerArmorPartComponent, BreakageEventArgs>(OnBreak);
    }

    public void OnBreak(Entity<PowerArmorPartComponent> ent, ref BreakageEventArgs args)
    {
        ent.Comp.isBroken = true;
        Dirty(ent);
    }
}