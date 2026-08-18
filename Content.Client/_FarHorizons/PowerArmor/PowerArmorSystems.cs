
using Content.Client.Clothing;
using Content.Client.Items.Systems;
using Content.Shared._FarHorizons.PowerArmor;
using Content.Shared.Destructible;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;

namespace Content.Client._FarHorizons.PowerArmor;

public sealed partial class PowerArmorSystem : SharedPowerArmorSystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private ItemSystem _item = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PowerArmorPartComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<PowerArmorPartComponent, EntGotRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnInserted(Entity<PowerArmorPartComponent> ent, ref EntGotInsertedIntoContainerMessage args) 
        => HandleReparent(ent, args.Container.Owner);

    private void OnRemoved(Entity<PowerArmorPartComponent> ent, ref EntGotRemovedFromContainerMessage args) 
        => HandleReparent(ent, args.Container.Owner, true);

    private void HandleReparent(Entity<PowerArmorPartComponent> ent, EntityUid powerArmor, bool remove=false)
    {
        
        if(!remove && _sprite.TryGetLayer(ent.Owner, ent.Comp.PartType, out var spriteData, false) && !ent.Comp.isBroken)
        {
            _sprite.LayerSetVisible(powerArmor, ent.Comp.PartType, true);
            _sprite.LayerSetRsi(powerArmor, ent.Comp.PartType, spriteData.RSI, spriteData.State);
        }
        else
        {
            _sprite.LayerSetVisible(powerArmor, ent.Comp.PartType, false);
        }
    }
}