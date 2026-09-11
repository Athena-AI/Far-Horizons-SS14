namespace Content.Shared.Clothing;

public sealed partial class ClothingSpeedModifierSystem
{
    public bool TrySetSpeedModifier(EntityUid ent, EntityUid wearer, float modifier, ClothingSpeedModifierComponent? component = null) 
    => TrySetSpeedModifier(ent, wearer, modifier, modifier, component);

    public bool TrySetSpeedModifier(EntityUid ent, EntityUid wearer, float walkspeedMod, float sprintspeedMod, ClothingSpeedModifierComponent? component = null)
    {
        if(!Resolve(ent, ref component))
            return false;

        component.SprintModifier = sprintspeedMod;
        component.WalkModifier = walkspeedMod;
        Dirty<ClothingSpeedModifierComponent>((ent, component));
        _movementSpeed.RefreshMovementSpeedModifiers(wearer);
        return true;
    }
}