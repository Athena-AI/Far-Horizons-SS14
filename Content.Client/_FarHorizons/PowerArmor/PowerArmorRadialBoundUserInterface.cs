using Content.Client.Stylesheets.Palette;
using Content.Client.UserInterface.Controls;
using Content.Shared._FarHorizons.PowerArmor;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Changeling.UI;

[UsedImplicitly]
public sealed partial class PowerArmorRadialBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private SimpleRadialMenu? _menu;
    private static readonly Color _selectedOptionBackground = Palettes.Green.Element.WithAlpha(128);
    private static readonly Color _selectedOptionHoverBackground = Palettes.Green.HoveredElement.WithAlpha(128);

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SimpleRadialMenu>();
        Update();
        _menu.OpenOverMouseScreenPosition();
    }

    public override void Update()
    {
        if (_menu == null)
            return;

        if (!EntMan.TryGetComponent<PowerArmorComponent>(Owner, out var PAComp))
            return;

        var models = ConvertToButtons(PAComp.Modules);

        _menu.SetButtons(models);
    }

    private IEnumerable<RadialMenuOptionBase> ConvertToButtons(List<EntityUid> Modules)
    {
        var buttons = new List<RadialMenuOptionBase>();
        foreach (var module in Modules)
        {
            if (!EntMan.TryGetComponent<PowerArmorModuleComponent>(module, out var PAMComp)
            || !PAMComp.canBeToggled
            || !EntMan.TryGetComponent<MetaDataComponent>(module, out var metadata))
                continue;

            var option = new RadialMenuActionOption<NetEntity>(SendModuleToggle, EntMan.GetNetEntity(module))
            {
                IconSpecifier = RadialMenuIconSpecifier.With(module),
                ToolTip = metadata.EntityName,
                BackgroundColor = PAMComp.isEnabled ? _selectedOptionBackground : null,
                HoverBackgroundColor = PAMComp.isEnabled ? _selectedOptionHoverBackground : null
            };
            buttons.Add(option);
        }

        return buttons;
    }

    private void SendModuleToggle(NetEntity moduleEntity) 
        => SendPredictedMessage(new PowerArmorToggleModuleMessage(moduleEntity));
}
