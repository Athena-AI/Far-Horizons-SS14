using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;

namespace Content.Shared.Silicons.Borgs;

public abstract partial class SharedBorgSystem
{
    public List<BorgHand> GetModuleItems(Container ModuleContainer)
    {
        List<BorgHand> borgHands = new();
        foreach(var module in ModuleContainer.ContainedEntities)
        {
            if(!TryComp<ItemBorgModuleComponent>(module, out var ibmComp))
                continue;

            foreach(var hand in ibmComp.Hands)
                borgHands.Add(hand);
        }
        return borgHands;
    }
}
