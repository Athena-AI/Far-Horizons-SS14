
using Content.Shared.DoAfter;
using Content.Shared.Silicons.Laws;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Silicons.Laws.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SiliconLawSetterComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<SiliconLawsetPrototype>? Lawset = default!;

    [DataField]
    public float SettingTime = 3f;

    [DataField]
    public bool ResetSubversion = true;

    [DataField]
    public SoundSpecifier? LawUploadSound = default;
}

[RegisterComponent]
public sealed partial class LawboardSlotComponent : Component
{
    [DataField(required: true)]
    public string LawboardSlotId = string.Empty;
}

[Serializable, NetSerializable]
public sealed partial class SetLawsetDoAfter : SimpleDoAfterEvent
{
    public readonly ProtoId<SiliconLawsetPrototype> Lawset;
    public readonly bool ResetSubversion;
    public SoundSpecifier? LawUploadSound = new SoundPathSpecifier("/Audio/Misc/cryo_warning.ogg");

    public SetLawsetDoAfter(ProtoId<SiliconLawsetPrototype> lawset, bool resetSubversion, SoundSpecifier? lawUploadSound)
    {
        Lawset = lawset;
        ResetSubversion = resetSubversion;
        LawUploadSound = lawUploadSound;
    }
}