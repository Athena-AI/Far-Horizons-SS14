using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.PowerArmor;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PowerArmorModuleComponent : Component
{
    [DataField]
    public TimeSpan InstallTime = TimeSpan.FromSeconds(5);

    [DataField]
    public bool canBeToggled = true;

    [ViewVariables, AutoNetworkedField]
    public bool isEnabled = false;

    [DataField]
    public float IdlePowerDrain = 0f;

    [DataField]
    public float ActivePowerDrain = 0f;

    [DataField]
    public int ComplexityCost = 1;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class PowerArmorPassiveModuleComponent : Component
{   
    [DataField("comps")]
    public ComponentRegistry? Components;  
}