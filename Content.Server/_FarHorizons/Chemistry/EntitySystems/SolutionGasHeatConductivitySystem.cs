using Content.Server.Atmos.EntitySystems;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Atmos;

namespace Content.Server._FarHorizons.Chemistry;

public sealed partial class SolutionGasHeatConductivitySystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private SolutionContainerSystem _solutionContainerSystem = default!;

    [SubscribeLocalEvent]
    private void AtmosUpdate(Entity<SolutionGasHeatConductivityComponent> ent, ref AtmosExposedUpdateEvent args)
    {
        if (!_solutionContainerSystem.TryGetSolution(ent.Owner, ent.Comp.Solution, out var solution) || solution.Value.Comp.Solution.Volume == 0)
            return;

        var heatDifferenceKelvin = args.GasMixture.Temperature - solution.Value.Comp.Solution.Temperature;
        if (Math.Abs(heatDifferenceKelvin) < 0.5)
            return;

        var thermalEnergyTransferWatts = heatDifferenceKelvin * ent.Comp.WattsPerKelvin;
        var thermalEnergyTransferJoules = thermalEnergyTransferWatts * AtmosphereSystem.ExposedUpdateDelay;

        _solutionContainerSystem.AddThermalEnergy(solution.Value, thermalEnergyTransferJoules);
        var gasHeatCapacity = _atmosphereSystem.GetHeatCapacity(args.GasMixture, true);
        args.GasMixture.Temperature += gasHeatCapacity == 0 ? 0 : -thermalEnergyTransferJoules / gasHeatCapacity;

        _atmosphereSystem.GetTileMixture(ent.Owner, excite: true);
    }
}
