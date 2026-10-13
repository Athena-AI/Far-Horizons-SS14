using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._FarHorizons.Chemistry;

[TestFixture]
public sealed class InjectorRegressionTests : InteractionTest
{
    private static readonly EntProtoId TargetPrototype = "MobHuman";

    [Test]
    public async Task MedipenInjectsTarget()
    {
        await SpawnTarget(TargetPrototype);
        var solutions = SEntMan.System<SharedSolutionContainerSystem>();
        var targetSolution = GetInjectableSolution(solutions, STarget!.Value);
        var initialVolume = targetSolution.Volume;

        await Client.WaitPost(() =>
        {
            var clientSolutions = CEntMan.System<SharedSolutionContainerSystem>();
            Assert.That(clientSolutions.TryGetInjectableSolution(ToClient(STarget.Value), out _, out _), Is.True,
                "The client must identify a humanoid's injectable solution to predict medipen use.");
        });

        await InteractUsing("EmergencyMedipen");

        Assert.That(targetSolution.Volume, Is.EqualTo(initialVolume + FixedPoint2.New(20)),
            "An emergency medipen should inject its complete solution into a humanoid target.");
    }

    [Test]
    public async Task CorpsHypoInjectsInsertedVial()
    {
        await SpawnTarget(TargetPrototype);
        var solutions = SEntMan.System<SharedSolutionContainerSystem>();
        var itemSlots = SEntMan.System<ItemSlotsSystem>();
        var corpsHypo = await PlaceInHands("CorpsHypo");
        var serverHypo = ToServer(corpsHypo);

        await Server.WaitPost(() =>
        {
            Assert.That(itemSlots.TryGetSlot(serverHypo, "vial_slot", out var vialSlot));
            Assert.That(vialSlot.Item, Is.Not.Null);
            Assert.That(solutions.TryGetSolution(vialSlot.Item!.Value, "beaker", out var vialSolution, out _));
            Assert.That(solutions.TryAddReagent(vialSolution!.Value, "Epinephrine", FixedPoint2.New(5)));
        });
        await RunTicks(1);

        var targetSolution = GetInjectableSolution(solutions, STarget!.Value);
        var initialVolume = targetSolution.Volume;
        await Interact();

        Assert.That(targetSolution.Volume, Is.EqualTo(initialVolume + FixedPoint2.New(5)),
            "The corpsman hypospray should inject solution from its inserted vial.");
    }

    private static Solution GetInjectableSolution(SharedSolutionContainerSystem solutions, EntityUid target)
    {
        Assert.That(solutions.TryGetInjectableSolution(target, out _, out var solution));
        return solution!;
    }
}
