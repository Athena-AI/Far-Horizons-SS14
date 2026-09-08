using Content.Client.Stylesheets.Palette;
using Content.Client.UserInterface.Controls;
using Content.Shared.Clothing.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Clothing;

[UsedImplicitly]
public sealed partial class ToggleableClothingRadialMultipleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
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

        if (!EntMan.TryGetComponent<ToggleableClothingMultipleComponent>(Owner, out var toggleable))
            return;

        var models = ConvertToButtons((Owner, toggleable));

        _menu.SetButtons(models);
    }

    private IEnumerable<RadialMenuOptionBase> ConvertToButtons(Entity<ToggleableClothingMultipleComponent> ent)
    {
        var buttons = new List<RadialMenuOptionBase>();

        foreach (var clothing in ent.Comp.ClothingUids)
        {
            if (!EntMan.TryGetComponent<MetaDataComponent>(clothing.Value, out var metadata))
                continue;

            var option = new RadialMenuActionOption<string>(SendModuleToggle, clothing.Key)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(clothing.Value),
                ToolTip = metadata.EntityName,
                BackgroundColor = ent.Comp.isActiveList.GetValueOrDefault(clothing.Key) ? _selectedOptionBackground : null,
                HoverBackgroundColor = ent.Comp.isActiveList.GetValueOrDefault(clothing.Key) ? _selectedOptionHoverBackground : null
            };
            buttons.Add(option);
        }

        return buttons;
    }

    private void SendModuleToggle(string slot) 
        => SendPredictedMessage(new ClothingSlotToggle(slot));
}
