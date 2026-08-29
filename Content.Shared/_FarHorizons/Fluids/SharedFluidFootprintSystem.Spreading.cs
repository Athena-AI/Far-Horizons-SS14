using System.Numerics;
using Content.Shared._FarHorizons.Fluids.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Fluids.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Fluids;

public abstract partial class SharedFluidFootprintSystem
{
    [SubscribeLocalEvent]
    private void OnEndCollide(Entity<FluidFootprintSpreaderComponent> ent, ref EndCollideEvent args)
    {
        if (TerminatingOrDeleted(ent) || TerminatingOrDeleted(args.OtherEntity) ||
            (TryComp<BuckleComponent>(ent, out var buckle) && buckle.BuckledTo != null) ||
            HasComp<KnockedDownComponent>(ent) ||
            !TryComp<FluidFootprintSourceComponent>(args.OtherEntity, out var source) ||
            !TryComp<PuddleComponent>(args.OtherEntity, out var puddle) ||
            !Solution.ResolveSolution(args.OtherEntity, puddle.SolutionName, ref puddle.Solution) ||
            puddle.Solution == null)
            return;
        
        var solution = puddle.Solution.Value.Comp.Solution;
        if (!solution.FootprintEligible(ProtoMan)) return;
        
        var numFootprints = ResolveNumFootprints((args.OtherEntity, source, puddle));

        if (numFootprints <= 0)
            return;

        var activeSpreader = EnsureComp<ActiveFluidFootprintSpreaderComponent>(ent);
        if (numFootprints <= activeSpreader.RemainingFootprints)
            return;
        
        var removeQt = ent.Comp.TakeSolutionUnits / solution.Contents.Count;
        Solution.RemoveEachReagent(puddle.Solution.Value, removeQt);

        var color = solution.GetColor(ProtoMan);

        activeSpreader.RemainingFootprints = numFootprints;
        activeSpreader.StopAt = _timing.CurTime + source.StopAfter;
        activeSpreader.LastPosition = TransformSys.GetMapCoordinates(ent);
        activeSpreader.Color = color;
        activeSpreader.OpacityStep = 1f / (numFootprints + 1);
    } 

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted) return;

        var query = EntityQueryEnumerator<ActiveFluidFootprintSpreaderComponent, FluidFootprintSpreaderComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var activeSpreader, out var spreader, out var xform))
        {
            if (_timing.CurTime < activeSpreader.NextStep)
                continue;
            
            activeSpreader.NextStep = _timing.CurTime + spreader.FootprintRate;

            if (activeSpreader.StopAt <= _timing.CurTime ||
                activeSpreader.RemainingFootprints <= 0)
            {
                RemCompDeferred<ActiveFluidFootprintSpreaderComponent>(uid);
                continue;
            }

            var currentPos = TransformSys.GetMapCoordinates(uid, xform);
            var lastPos = activeSpreader.LastPosition ?? currentPos;

            if (currentPos.MapId != lastPos.MapId)
            {
                activeSpreader.LastPosition = currentPos;
                continue;
            }

            var distance = (currentPos.Position - lastPos.Position).Length();
            if (distance < 0.1f)
                continue;
            
            var stepSpacing = spreader.StepSpacing;
            var totalSteps = Math.Min(activeSpreader.RemainingFootprints, (int) Math.Floor(distance / stepSpacing));

            if (totalSteps <= 0)
                continue;
            
            activeSpreader.LastPosition = currentPos;
            
            var pathVector = currentPos.Position - lastPos.Position;
            var angle = pathVector.ToAngle();

            var ev = new BootFootprintModifyEvent(spreader.Footprint);
            RaiseLocalEvent(uid, ref ev);
            var footprint = ev.Footprint;

            for (var i = 1; i <= totalSteps; i++)
            {
                var t = i / (float) totalSteps;
                var interpolatedPos = Vector2.Lerp(lastPos.Position, currentPos.Position, t);

                var mapCoords = new MapCoordinates(interpolatedPos, currentPos.MapId);
                DoStep((uid, spreader, activeSpreader), mapCoords, angle, footprint);

                activeSpreader.RemainingFootprints--;
                if (activeSpreader.RemainingFootprints <= 0)
                    break;
            }
        }
    }

    public void DoStep(Entity<FluidFootprintSpreaderComponent, ActiveFluidFootprintSpreaderComponent> ent, MapCoordinates mapCoords, Angle angle, ProtoId<FootprintTypePrototype> footprintType)
    {
        if (!_map.TryFindGridAt(mapCoords, out var gridUid, out var gridComp))
            return;
        
        var proto = ProtoMan.Index(ent.Comp1.Footprint);
        var finalPos = mapCoords.Position;

        if (proto.Alternating)
        {
            var moveDir = angle.ToVec();
            var leftPerpendicular = new Vector2(-moveDir.Y, moveDir.X).Normalized();

            var sideMultiplier = ent.Comp2.Left ? 1f : -1f;
            var offsetAmount = ent.Comp1.LateralOffset;

            finalPos += leftPerpendicular * (offsetAmount * sideMultiplier);
        }

        mapCoords = new MapCoordinates(finalPos, mapCoords.MapId);
        
        var gridLocalCoords = TransformSys.ToCoordinates(gridUid, mapCoords);

        var tileIndices = _map.TileIndicesFor((gridUid, gridComp), gridLocalCoords);
        var tile = ResolveFootprintTile((gridUid, gridComp), tileIndices);

        var tileCenter = _map.GridTileToLocal(gridUid, gridComp, tileIndices);
        var relativePos = gridLocalCoords.Position - tileCenter.Position;

        var flipped = proto.Alternating && !ent.Comp2.Left;

        if (tile != null)
        {
            tile!.Value.Comp.AddFootprint(relativePos, angle, footprintType, ent.Comp1.FootprintSize, ent.Comp2.Color, flipped, ent.Comp2.Opacity);
            UpdateSprite(tile.Value);
            Dirty(tile.Value);
        }
        
        var opacity = MathF.Max(0f, ent.Comp2.Opacity - ent.Comp2.OpacityStep);
        ent.Comp2.Opacity = opacity;
        if (proto.Alternating)
            ent.Comp2.Left = !ent.Comp2.Left;
    }

    public int ResolveNumFootprints(Entity<FluidFootprintSourceComponent, PuddleComponent> ent)
    {
        if (ent.Comp2.Solution == null) return 0;

        var solution = ent.Comp2.Solution.Value.Comp.Solution;

        if (solution.Volume < ent.Comp1.MinUnitsForFootPrint)
            return 0;
        
        var volumeBonus = (int)((solution.Volume - ent.Comp1.MinUnitsForFootPrint) * ent.Comp1.BonusPerUnit);
        return ent.Comp1.MinFootprints + volumeBonus;
    }
}