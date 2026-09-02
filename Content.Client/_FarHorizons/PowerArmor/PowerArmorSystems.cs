using Content.Client.Clothing;
using Content.Client.Items.Systems;
using Content.Client.Toggleable;
using Content.Shared._FarHorizons.PowerArmor;
using Content.Shared.Clothing.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Client._FarHorizons.PowerArmor;

public sealed partial class PowerArmorSystem : SharedPowerArmorSystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private ClientClothingSystem _clothing = default!;
    [Dependency] private ItemSystem _item = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PowerArmorPartComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    protected override void OnPartInserted(Entity<PowerArmorPartComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        base.OnPartInserted(ent, ref args);
        var powerArmor = args.Container.Owner;

        if(!HasComp<PowerArmorComponent>(powerArmor) 
        || !_sprite.TryGetLayer(ent.Owner, ent.Comp.PartType, out var spriteData, false) 
        || spriteData.ActualState is null) return;

        _sprite.LayerSetVisible(powerArmor, ent.Comp.PartType, true);
        _sprite.LayerSetRsi(powerArmor, ent.Comp.PartType, spriteData.ActualState.RSI, spriteData.ActualState.StateId);
        if(ent.Comp.PartType == PowerArmorVisualLayers.Head)
            _sprite.LayerSetRsi(powerArmor, "light", spriteData.ActualState.RSI, new Robust.Client.Graphics.RSI.StateId($"{spriteData.ActualState.StateId}-light"));

        if(TryComp<ClothingComponent>(powerArmor, out var clothingComp) && spriteData.ActualState.StateId.Name != null)
        {
            var slot = ent.Comp.PartType == PowerArmorVisualLayers.Head ? "head" : "outerClothing";
            _clothing.SetLayerRSI(clothingComp, slot , $"enum.PowerArmorVisualLayers.{ent.Comp.PartType}", spriteData.ActualState.RSI.Path.ToString(), spriteData.ActualState.StateId.Name);
            _clothing.SetLayerVisibility(clothingComp, slot, $"enum.PowerArmorVisualLayers.{ent.Comp.PartType}", true);
            
            if (TryComp<ToggleableVisualsComponent>(powerArmor, out var toggleComp)
                && toggleComp.ClothingVisuals.TryGetValue(slot, out var clothingVisuals)
                && clothingVisuals.TryFirstOrDefault(out var targetLayer))
            {
                targetLayer.RsiPath = spriteData.ActualState.RSI.Path.ToString();
                targetLayer.State = $"{spriteData.ActualState.StateId}-light";
            }

            _item.VisualsChanged(powerArmor);
        }
    }

    protected override void OnPartEjected(Entity<PowerArmorPartComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        base.OnPartEjected(ent, ref args);
        var powerArmor = args.Container.Owner;
        if(!HasComp<PowerArmorComponent>(powerArmor)) return;

        _sprite.LayerSetVisible(powerArmor, ent.Comp.PartType, false);
        if(TryComp<ClothingComponent>(powerArmor, out var clothingComp))
        {
            var slot = ent.Comp.PartType == PowerArmorVisualLayers.Head ? "head" : "outerClothing";
            _clothing.SetLayerVisibility(clothingComp, slot, $"enum.PowerArmorVisualLayers.{ent.Comp.PartType}", false);
            
            if (TryComp<ToggleableVisualsComponent>(powerArmor, out var toggleComp)
                && toggleComp.ClothingVisuals.TryGetValue(slot, out var clothingVisuals)
                && clothingVisuals.TryFirstOrDefault(out var targetLayer))
            {
                targetLayer.RsiPath = null;
                targetLayer.State = null;
            }
            
            _item.VisualsChanged(powerArmor);
        }
    }
    
    private void OnAppearanceChange(Entity<PowerArmorPartComponent> ent, ref AppearanceChangeEvent args)
    {
        if(!_appearance.TryGetData<NetEntity>(ent.Owner, PowerArmorPartVisuals.PowerArmor, out var powerArmorNetEntity)) 
            return;
        
        var powerArmor = GetEntity(powerArmorNetEntity);

        if(_appearance.TryGetData<bool>(ent.Owner, PowerArmorPartVisuals.Visible, out var visibility))
        {
            _sprite.LayerSetVisible(powerArmor, ent.Comp.PartType, visibility);   
            if(TryComp<ClothingComponent>(powerArmor, out var clothingComp))
            {
                var slot = ent.Comp.PartType == PowerArmorVisualLayers.Head ? "head" : "outerClothing";
                _clothing.SetLayerVisibility(clothingComp, slot, $"enum.PowerArmorVisualLayers.{ent.Comp.PartType}", visibility);
                _item.VisualsChanged(powerArmor);
            }
        }
    }
}