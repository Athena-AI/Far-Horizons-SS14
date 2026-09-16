using Content.Client.Stylesheets.Palette;
using Content.Client.UserInterface.Controls;
using Content.Shared.Starlight;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._Starlight.Medical.Limbs;

[UsedImplicitly]
public sealed partial class CyberLimbSystemItemsRadial(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
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

        if (!EntMan.TryGetComponent<LimbItemStorageComponent>(Owner, out var lisComp) || !EntMan.TryGetComponent<LimbItemDeployerComponent>(Owner, out var lidComp))
            return;

        var models = ConvertToButtons((Owner, lisComp, lidComp));

        _menu.SetButtons(models);
    }

    private IEnumerable<RadialMenuOptionBase> ConvertToButtons(Entity<LimbItemStorageComponent, LimbItemDeployerComponent> ent)
    {
        var buttons = new List<RadialMenuOptionBase>();

        var ToggleOption = new RadialMenuActionOption<NetEntity>(SendLimbToggle, EntMan.GetNetEntity(ent.Owner))
        {
            IconSpecifier = RadialMenuIconSpecifier.With(
                new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/Spare/poweronoff.svg.192dpi.png"))
            ),
            ToolTip = ent.Comp2.Toggled ? "Undeploy Limb" : "Deploy Limb",
            BackgroundColor = ent.Comp2.Toggled ? _selectedOptionBackground : null,
            HoverBackgroundColor = ent.Comp2.Toggled ? _selectedOptionHoverBackground : null
        };
        buttons.Add(ToggleOption);

        foreach (var item in ent.Comp1.ItemEntities)
        {
            if (!EntMan.TryGetComponent<MetaDataComponent>(item.Key, out var metadata)
            || metadata.EntityPrototype is not { } proto)
                continue;

            var option = new RadialMenuActionOption<NetEntity>(SendLimbItemToggle, EntMan.GetNetEntity(item.Key))
            {
                IconSpecifier = RadialMenuIconSpecifier.With((EntProtoId) proto.ID),
                ToolTip = metadata.EntityName,
                BackgroundColor = item.Value ? _selectedOptionBackground : null,
                HoverBackgroundColor = item.Value ? _selectedOptionHoverBackground : null,
                KeepOpen = true
            };
            buttons.Add(option);
        }

        return buttons;
    }

    private void SendLimbToggle(NetEntity _) 
        => SendPredictedMessage(new LimbToggleMessage());

    private void SendLimbItemToggle(NetEntity item) 
        => SendPredictedMessage(new LimbItemToggleMessage(item));
}
