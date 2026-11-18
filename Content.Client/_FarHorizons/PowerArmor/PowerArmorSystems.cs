using System.Linq;
using Content.Client.Clothing;
using Content.Client.Items.Systems;
using Content.Client.Toggleable;
using Content.Shared._FarHorizons.PowerArmor;
using Content.Shared.Clothing.Components;
using Content.Shared.Humanoid;
using Content.Shared.Inventory.Events;
using Content.Shared.PowerCell.Components;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Client._FarHorizons.PowerArmor;

public sealed partial class PowerArmorSystem : SharedPowerArmorSystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private ClientClothingSystem _clothing = default!;
    [Dependency] private ItemSystem _item = default!;
    [Dependency] private IPlayerManager _player = default!;

    private static readonly TimeSpan _alertUpdateDelay = TimeSpan.FromSeconds(0.5f);
    private TimeSpan _nextAlertUpdate;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_player.LocalEntity is not { } localPlayer)
            return;

        var curTime = _timing.CurTime;

        if (curTime < _nextAlertUpdate)
            return;

        _nextAlertUpdate = curTime + _alertUpdateDelay;

        if(!TryComp<PowerArmorUserComponent>(localPlayer, out var PAUComp))
            return;
            
        UpdateBatteryAlert((localPlayer, PAUComp));
    }

    private void UpdateBatteryAlert(Entity<PowerArmorUserComponent> ent)
    {
        if(!TryComp<PowerArmorComponent>(ent.Comp.Wearing, out var PAComp)
            || !TryComp<PowerCellSlotComponent>(ent.Comp.Wearing, out var powerCell))
            return;

        if (!_powerCell.TryGetBatteryFromSlot((ent.Comp.Wearing, powerCell), out var battery))
        {
            _alerts.ShowAlert(ent.Owner, ent.Comp.NoBatteryAlert);
            return;
        }

        var chargeLevel = (short)MathF.Round(_battery.GetChargeLevel(battery.Value.AsNullable()) * 10f);

        if (chargeLevel == 0 && _powerCell.HasDrawCharge((ent.Comp.Wearing, null, powerCell)))
        {
            chargeLevel = 1;
        }

        _alerts.ShowAlert(ent.Owner, ent.Comp.BatteryAlert, chargeLevel);
    }

    protected override void OnPartInserted(Entity<PowerArmorPartComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        base.OnPartInserted(ent, ref args);
        var powerArmor = args.Container.Owner;

        if(!HasComp<PowerArmorComponent>(powerArmor) 
        || !_sprite.TryGetLayer(ent.Owner, ent.Comp.PartType, out var spriteData, false) 
        || spriteData.ActualState is null) return;

        if (_sprite.LayerMapTryGet(powerArmor, ent.Comp.PartType, out _, false))
        {
            _sprite.LayerSetVisible(powerArmor, ent.Comp.PartType, true);
            _sprite.LayerSetRsi(powerArmor, ent.Comp.PartType, spriteData.ActualState.RSI, spriteData.ActualState.StateId);
        }
        if (ent.Comp.PartType == PowerArmorVisualLayers.Head && _sprite.LayerMapTryGet(powerArmor, "light", out _, false))
            _sprite.LayerSetRsi(powerArmor, "light", spriteData.ActualState.RSI, new Robust.Client.Graphics.RSI.StateId($"{spriteData.ActualState.StateId}-light"));
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
    
    [SubscribeLocalEvent]
    private void OnClothingEquipped(Entity<PowerArmorComponent> ent, ref GotEquippedEvent args)
    {
        if(!TryComp<ClothingComponent>(ent.Owner, out var clothingComp))
            return;         
        var race = CompOrNull<HumanoidProfileComponent>(args.EquipTarget)?.Species;

        foreach (var slot in ent.Comp.Parts)
        {
            if (slot.Value == null
                || !TryComp<PowerArmorPartComponent>(slot.Value, out var partComp)
                || partComp.RsiPath == null)
                continue;

            var partType = slot.Key.ToString();
            string? speciesKey = null;
            if (race != null)
            {
                var wanted = $"{partType}-{race}";
                speciesKey = partComp.PartVisuals.Keys
                    .FirstOrDefault(k => string.Equals(k, wanted, StringComparison.OrdinalIgnoreCase));
            }

            var key = speciesKey ?? partType;

            if (!partComp.PartVisuals.TryGetValue(key, out var states))
                continue; 

            var baseState = states.FirstOrDefault(x => x.Shader == null);
            if (baseState == null)
                continue;

            var unshadedState = states.FirstOrDefault(x => x.Shader == "unshaded");
            unshadedState ??= baseState; 

            _clothing.SetLayerRSI(clothingComp, args.Slot, $"enum.PowerArmorVisualLayers.{partType}", partComp.RsiPath, baseState.State);
            _clothing.SetLayerVisibility(clothingComp, args.Slot, $"enum.PowerArmorVisualLayers.{partType}", true);

            if (TryComp<ToggleableVisualsComponent>(ent.Owner, out var toggleComp)
                && toggleComp.ClothingVisuals.TryGetValue(args.Slot, out var clothingVisuals)
                && clothingVisuals.FirstOrDefault() is { } targetLayer)
            {
                targetLayer.RsiPath = partComp.RsiPath;
                targetLayer.State = unshadedState.State;
            }
        }

        _item.VisualsChanged(ent.Owner);
    }

    [SubscribeLocalEvent]
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