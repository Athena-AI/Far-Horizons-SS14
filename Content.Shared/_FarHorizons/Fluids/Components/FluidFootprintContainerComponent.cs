using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Fluids.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class FluidFootprintContainerComponent : Component
{
    [ViewVariables, AutoNetworkedField] public List<Color> ColorPalette = new();
    [ViewVariables, AutoNetworkedField] public List<ProtoId<FootprintTypePrototype>> ProtoPalette = new();
    [ViewVariables, AutoNetworkedField] public List<FootprintData> Footprints = new();
    [DataField] public EntProtoId? CleanEffect;
    [DataField] public int FootprintLimit = 100; // Plain hard limit. With 8x8 footprints, assuming all drawn at 100% scale - that's 6k pixels, more than enough to cover 1k pixels of a single tile

    // I compress the data to the best of my ability without losing anything.
    // If I did math right, that's around 75%-85% savings on memory and network data as compared to doing this naively
    public void AddFootprint(
        Vector2 position,
        Angle angle,
        ProtoId<FootprintTypePrototype> footprint,
        float size,
        Color color,
        bool flip,
        float opacity)
    {
        if (Footprints.Count > FootprintLimit)
            return;

        if (!ProtoPalette.Contains(footprint))
            ProtoPalette.Add(footprint);
        
        var protoId = (byte)ProtoPalette.IndexOf(footprint);

        if (!ColorPalette.Contains(color))
            ColorPalette.Add(color);
        
        var colorId = (byte)ColorPalette.IndexOf(color);

        // Normalize coordinates between -0.25 and 1.25. Coordinate space of a tile is 0 to 1, and this leaves just a small wiggle room to avoid errors
        var packedPosX = (ushort)(Math.Clamp((position.X + 0.25f) / 1.5f, 0f, 1f) * ushort.MaxValue);
        var packedPosY = (ushort)(Math.Clamp((position.Y + 0.25f) / 1.5f, 0f, 1f) * ushort.MaxValue);

        var packedAngle = (ushort)(angle.Theta / MathF.Tau * ushort.MaxValue);

        // Normalize size between 0 and 3
        var packedSize = (ushort)(Math.Clamp(size / 3f, 0f, 1f) * ushort.MaxValue);

        var packedOpacity = (byte)Math.Clamp((int)(opacity * 255f), 0, 255);

        var result = new FootprintData(packedPosX, packedPosY, packedAngle, protoId, colorId, packedSize, packedOpacity, flip);
        Footprints.Add(result);
    }

    public (
        Vector2 Position,
        Angle Angle,
        ProtoId<FootprintTypePrototype> Footprint,
        float Size,
        Color Color,
        bool Flip,
        float Opacity
        )
        Unpack(FootprintData data)
    {
        var posX = (data.PosX / (float)ushort.MaxValue * 1.5f) - 0.25f;
        var posY = (data.PosY / (float)ushort.MaxValue * 1.5f) - 0.25f;
        var angle = new Angle(data.Angle / (float)ushort.MaxValue * MathF.Tau);
        var size = data.Size / (float)ushort.MaxValue * 3f;
        var footprint = ProtoPalette[data.ProtoId];
        var color = ColorPalette[data.ColorId];
        var opacity = data.Opacity / 255f;

        return (new Vector2(posX, posY), angle, footprint, size, color, data.Flip, opacity);
    }
}

[Serializable, NetSerializable]
public readonly record struct FootprintData // 12 bytes baybee
(
    ushort PosX,
    ushort PosY,
    ushort Angle,
    byte ProtoId,
    byte ColorId,
    ushort Size,
    byte Opacity,
    bool Flip
);