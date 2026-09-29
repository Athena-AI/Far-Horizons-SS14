
using Content.Shared._FarHorizons.Vehicles.Components;
using Robust.Client.GameObjects;

namespace Content.Client._FarHorizons.Vehicles;
public sealed partial class VehicleSystems
{    
    
    [SubscribeLocalEvent]
    private void OnAppearanceChange(EntityUid uid, VehicleEquipmentComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;
        
        if (_appearance.TryGetData(uid, EquipmentVisuals.Hidden, out bool hidden))
            _sprite.SetVisible((uid, args.Sprite), !hidden);
    }
}