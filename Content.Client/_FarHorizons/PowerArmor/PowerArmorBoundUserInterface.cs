using Content.Shared._FarHorizons.PowerArmor;
using Robust.Client.UserInterface;

namespace Content.Client._FarHorizons.PowerArmor;

public sealed class PowerArmorBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private PowerArmorMenu? _menu;
    public PowerArmorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {}

    protected override void Open()
    {
        base.Open();
        _menu = this.CreateWindow<PowerArmorMenu>();
        _menu.SetEntity(Owner);

        _menu.OnUninstallPart += (layer, part) =>
            SendMessage(new UninstallArmorPartMessage(layer, part));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if(_menu == null)
            return;
    }
}