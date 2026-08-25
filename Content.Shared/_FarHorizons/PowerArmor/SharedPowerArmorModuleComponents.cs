using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.PowerArmor;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, ImplicitDataDefinitionForInheritors]
public abstract partial class PowerArmorModuleComponent : Component
{
    [DataField]
    public TimeSpan InstallTime = TimeSpan.FromSeconds(5);

    [ViewVariables, AutoNetworkedField]
    public bool isActive = false;

    [DataField]
    public float PowerDrain = 1.0f;

    [DataField]
    public int ComplexityCost = 1;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]

public sealed partial class PowerArmorPassiveModuleComponent : PowerArmorModuleComponent
{   
    [DataField("comps")]
    public ComponentRegistry? Components;  
}