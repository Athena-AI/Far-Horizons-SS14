using System.Linq;
using Content.Shared.Body;
using Content.Shared._FarHorizons.Traits;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.Traits.Assorted;
using Content.Shared.GameTicking;

namespace Content.Shared._FarHorizons.Body;

public sealed partial class MovementOrganSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private InventorySystem _inventory = default!;

    private const float NoLegsModifier = 0.1f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MovementOrganExpectedToMoveComponent, RefreshMovementSpeedModifiersEvent>(OnMovementModifierRefresh);
        SubscribeLocalEvent<MovementOrganExpectedToMoveComponent, PlayerSpawnCompleteEvent>((uid, _, _) => SyncAndRefresh(uid));
        SubscribeLocalEvent<MovementOrganExpectedToMoveComponent, TraitsApplied>((uid, _, _) => SyncAndRefresh(uid));
        SubscribeLocalEvent<MovementOrganComponent, OrganGotRemovedEvent>((_, ref args) => SyncAndRefresh(args.Target));
        SubscribeLocalEvent<MovementOrganComponent, OrganGotInsertedEvent>((_, ref args) => SyncAndRefresh(args.Target));
    }

    private void SyncAndRefresh(EntityUid target)
    {
        if (TerminatingOrDeleted(target)) return;

        SyncLegsParalyzed(target);
        _movementSpeed.RefreshMovementSpeedModifiers(target);
    }

    private List<MovementOrganComponent> GetLegs(BodyComponent body) =>
        body.Organs!.ContainedEntities
            .Select(CompOrNull<MovementOrganComponent>)
            .Where(p => p != null)
            .Select(p => p!)
            .ToList();

    private bool IsParalyzed(EntityUid uid, int legCount) =>
        legCount <= 1 ||
        (TryComp<HumanoidCharacterProfileComponent>(uid, out var hcp)
        && hcp.Profile != null
        && hcp.Profile.TraitPreferences.Contains("WheelchairBound"));

    private void SyncLegsParalyzed(EntityUid uid)
    {
        if (!HasComp<MovementOrganExpectedToMoveComponent>(uid)
            || !TryComp<BodyComponent>(uid, out var body)
            || body.Organs == null || body.Organs.Count == 0)
            return;

        if (IsParalyzed(uid, GetLegs(body).Count))
        {
            EnsureComp<LegsParalyzedComponent>(uid);
        }
        else if (TryComp<LegsParalyzedComponent>(uid, out var paralyzed)
                && paralyzed.LifeStage < ComponentLifeStage.Stopping)
        {
            RemCompDeferred<LegsParalyzedComponent>(uid);
        }
    }

    private void OnMovementModifierRefresh(Entity<MovementOrganExpectedToMoveComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (TerminatingOrDeleted(ent) || MetaData(ent).EntityLifeStage < EntityLifeStage.MapInitialized) return;
        if (!TryComp<BodyComponent>(ent, out var body) || body.Organs == null || body.Organs.Count == 0) return;

        var allLegs = GetLegs(body);
        var shoesEquipped = _inventory.TryGetSlotEntity(ent, "shoes", out _);

        var walk = allLegs.Sum(p => p.ShoesNegate && shoesEquipped ? 1 : p.WalkSpeedModifier);
        var sprint = allLegs.Sum(p => p.ShoesNegate && shoesEquipped ? 1 : p.SprintSpeedModifier);

        var expected = Math.Max(1, ent.Comp.ExpectedAmount);
        var totalWalk = walk / expected;
        var totalSprint = sprint / expected;

        if (IsParalyzed(ent, allLegs.Count))
        {
            totalWalk = NoLegsModifier;
            totalSprint = NoLegsModifier;
        }

        args.ModifySpeed(totalWalk, totalSprint);
    }
}