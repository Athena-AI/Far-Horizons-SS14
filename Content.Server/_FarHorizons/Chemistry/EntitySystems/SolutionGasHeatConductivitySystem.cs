using Content.Server.Atmos.EntitySystems;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Atmos;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Chemistry;

public sealed partial class SolutionGasHeatConductivitySystem : EntitySystem
{
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private SolutionContainerSystem _solutionContainerSystem = default!;

    [SubscribeLocalEvent]
    private void OnAtmosUpdate(Entity<SolutionGasHeatConductivityComponent> ent, ref AtmosExposedUpdateEvent args)
    {
        if (!_solutionContainerSystem.TryGetSolution(ent.Owner, ent.Comp.Solution, out var solution) || solution.Value.Comp.Solution.Volume == 0)
            return;

        var heatDifferenceKelvin = args.GasMixture.Temperature - solution.Value.Comp.Solution.Temperature;
        var solutionCapacity = solution.Value.Comp.Solution.GetHeatCapacity(_proto);
        var gasCapacity = _atmosphereSystem.GetHeatCapacity(args.GasMixture, true);
        if (gasCapacity <= 0 || solutionCapacity <= 0)
            return;

        var equalizeJoules = heatDifferenceKelvin * (gasCapacity * solutionCapacity / (gasCapacity + solutionCapacity));

        var transferJoules = heatDifferenceKelvin * ent.Comp.WattsPerKelvin * AtmosphereSystem.ExposedUpdateDelay;
        transferJoules = Math.Clamp(transferJoules, -Math.Abs(equalizeJoules), Math.Abs(equalizeJoules));

        _solutionContainerSystem.AddThermalEnergy(solution.Value, transferJoules);
        args.GasMixture.Temperature -= transferJoules / gasCapacity;

        _atmosphereSystem.GetTileMixture(ent.Owner, excite: true);
    }
}
