using Content.Shared._FarHorizons.Banking;
using Content.Shared._FarHorizons.Banking.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Verbs;

namespace Content.Shared.Hands.EntitySystems;

public abstract partial class SharedHandsSystem
{
    [Dependency] private SharedBankingSystem _banking = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    private void OfferCreditsVerb(EntityUid uid, HandsComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess ||
            !args.CanInteract ||
            args.User == args.Target ||
            args.Using is null ||
            !TryComp<CredstickComponent>(args.Using, out var credstick) ||
            _banking.FindCredstick(args.Target) is not {} targetCredstick ||
            HasComp<CredstickTransferSenderComponent>(uid) ||
            HasComp<CredstickTransferReceiverComponent>(args.Target))
            return;

        args.Verbs.Add(new Verb()
        {
            Act = () => OpenOfferDialog((args.Using.Value, credstick), args.Target, args.User),
            DoContactInteraction = true,
            Text = Loc.GetString("credstick-offer-transfer"),
            IconEntity = GetNetEntity(args.Using)
        });
    }

    private void OpenOfferDialog(Entity<CredstickComponent> ent, EntityUid target, EntityUid user)
    {
        ent.Comp.TransferSource = user;
        ent.Comp.TransferTarget = target;
        _ui.OpenUi(ent.Owner, ent.Comp.TransferUiKey, user);
    }
}