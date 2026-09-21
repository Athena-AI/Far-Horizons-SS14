using System.Linq;
using Content.Server._FarHorizons.Banking;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Events;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared._FarHorizons.Factions;
using Content.Shared.Chat;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mind;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Server.Starlight.SecureTerminal;

namespace Content.Server._Starlight.Economy;

public sealed partial class SalarySystem : SharedSalarySystem
{
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private IPlayerRolesManager _playerRolesManager = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _time = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private IChatManager _chat = default!;
    [Dependency] private RoleSystem _roles = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private ISharedFactionManager _factions = default!; // Far Horizons
    [Dependency] private BankingSystem _banking = default!; // Far Horizons

    private float _delayAccumulator = 0f;
    private readonly Stopwatch _stopwatch = new();
    private readonly Dictionary<ICommonSession, TimeSpan> _lastSalary = [];
    private SalariesPrototype? _salaries;

    private const string Standart = "standart";

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundStartingEvent>(ev => _lastSalary.Clear());
        _salaries = _prototypes.Index<SalariesPrototype>(Standart);
        base.Initialize();
    }
    public override void Update(float frameTime)
    {
        if (_salaries == null) return;

        _delayAccumulator += frameTime;
        if (_delayAccumulator > 2)
        {
            _delayAccumulator = 0;
            _stopwatch.Restart();

            var query = _playerRolesManager.Players.GetEnumerator();
            while (query.MoveNext() && _stopwatch.Elapsed < TimeSpan.FromMilliseconds(0.1))
            {
                if (!_lastSalary.TryGetValue(query.Current.Session, out var lastTime))
                {
                    _lastSalary.Add(query.Current.Session, _time.CurTime);
                    continue;
                }
                if (!_entityManager.TryGetComponent<MobStateComponent>(query.Current.Session.AttachedEntity, out var state) 
                    || state.CurrentState == MobState.Critical 
                    || state.CurrentState == MobState.ActiveCritical // Far Horizons
                    || state.CurrentState == MobState.Dead)
                    continue;
                if (_time.CurTime - lastTime > TimeSpan.FromMinutes(15)
                    && _mind.TryGetMind(query.Current.Session.UserId, out var mind))
                {
                    PaySalary(query.Current.Session, (mind.Value.Owner, mind.Value.Comp));

                    _lastSalary[query.Current.Session] = _time.CurTime;
                }
            }
        }
    }

    private int CalculateSalaryWithBonuses(int baseSalary, ICommonSession session, string source)
    {
        var bonusMultiplier = _defaultBonusMultiplier;

        var sourceModifier = GetStationSalaryModifier("Everyone") + GetStationSalaryModifier(source);
        var multiplier = Math.Max(0.2f, 1f + sourceModifier); // Minimum income is 20% of the base salary
        bonusMultiplier = Math.Max(0f, bonusMultiplier); // Bonus has to be positive
        return (int)Math.Ceiling(baseSalary * bonusMultiplier * multiplier);
    }

    private float GetStationSalaryModifier(string source)
    {
        var modifier = 0f;
        var query = _entityManager.EntityQueryEnumerator<SecureCommandTerminalStationComponent>();
        while (query.MoveNext(out _, out var comp))
            modifier += comp.SalaryModifiers.GetValueOrDefault(source);

        return modifier;
    }

    internal int PaySalary(ICommonSession session)
    {
        if (!_mind.TryGetMind(session.UserId, out var mind))
            return 0;

        return PaySalary(session, (mind.Value.Owner, mind.Value.Comp));
    }

    private int PaySalary(ICommonSession session, Entity<MindComponent?> mind)
    {
        // Far Horizons start
        var senderProto = _roles.MindGetFaction(mind.Value.Owner) ?? _factions.GetCurrentFaction() ?? _factions.GetDefaultFaction();
        var sender = _prototypes.Index(senderProto).Name;
        // Far Horizons end
        
        if (!_playerResources.TryGetResource(session, "credits", out _))
            return 0;

        var total = 0;
        var roles = _roles.MindGetAllRoleInfo(mind);
        foreach (var role in roles)
        {
            if (!_salaries.Jobs.TryGetValue(role.Prototype, out var salary))
                continue;

            var sender = _salaries.Sender.GetValueOrDefault(role.Prototype, "NanoTrasen");
            var amount = CalculateSalaryWithBonuses(salary, session, sender);
            _playerResources.TryUpdateResource(session, "credits", amount);

            var message = Loc.GetString("economy-chat-salary-message", ("amount", amount), ("sender", sender));
            var wrappedMessage = Loc.GetString("economy-chat-salary-wrapped-message", ("amount", amount), ("sender", sender), ("senderColor", "#2384CE"));
            _chat.ChatMessageToOne(ChatChannel.Notifications, message, wrappedMessage, default, false, session.Channel, Color.FromHex("#57A3F7"));
            total += amount;
        }

        return total;
    }

    internal void Donate(ICommonSession session, int amount)
    {
        var playerData = _playerRolesManager.GetPlayerData(session);
        if (playerData == null)
            return;

        playerData.Balance += amount;
        if (session.AttachedEntity != null)
            _banking.ChangeBalance(session.AttachedEntity.Value, amount); // Far Horizons

        // We need to make a prototype
        var i = _random.Next(0, 20);
        var message = Loc.GetString($"economy-chat-donate-{i}-message", ("amount", amount));
        var wrappedMessage = Loc.GetString($"economy-chat-donate-{i}-wrapped-message", ("amount", amount));
        _chat.ChatMessageToOne(ChatChannel.Notifications, message, wrappedMessage, default, false, session.Channel, Color.FromHex("#57A3F7"));
    }
}