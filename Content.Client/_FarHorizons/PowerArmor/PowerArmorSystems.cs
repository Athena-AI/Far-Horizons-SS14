
using Content.Client.Clothing;
using Content.Client.Items.Systems;
using Content.Shared._FarHorizons.PowerArmor;
using Content.Shared.Item;
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
        SubscribeLocalEvent<PowerArmorComponent, EntInsertedIntoContainerMessage>(OnInserted, before: [typeof(ClientClothingSystem)]);
        SubscribeLocalEvent<PowerArmorComponent, EntRemovedFromContainerMessage>(OnRemoved, before: [typeof(ClientClothingSystem)]);
        SubscribeLocalEvent<PowerArmorComponent, VisualsChangedEvent>(OnUpdate, before: [typeof(ClientClothingSystem)]);
    }

    private void OnInserted(Entity<PowerArmorComponent> ent, ref EntInsertedIntoContainerMessage args) 
        => HandleReparent(ent, args.Entity);

    private void OnRemoved(Entity<PowerArmorComponent> ent, ref EntRemovedFromContainerMessage args) 
        => HandleReparent(ent, args.Entity, true);

    private void HandleReparent(Entity<PowerArmorComponent> ent, EntityUid part, bool remove=false)
    {
        if (!TryComp<PowerArmorPartComponent>(part, out var papComp))
            return;

        if(!remove && _sprite.TryGetLayer(part, papComp.PartType, out var spriteData, false))
        {
            _sprite.LayerSetVisible(ent.Owner, papComp.PartType, true);
            _sprite.LayerSetRsi(ent.Owner, papComp.PartType, spriteData.RSI, spriteData.State);
        }
        else
        {
            _sprite.LayerSetVisible(ent.Owner, papComp.PartType, false);
        }

        _item.VisualsChanged(ent.Owner);
    }

    private void OnUpdate(Entity<PowerArmorComponent> ent, ref VisualsChangedEvent args)
    {
        
    }
}