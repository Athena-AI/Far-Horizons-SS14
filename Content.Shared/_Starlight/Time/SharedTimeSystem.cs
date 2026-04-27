using Content.Shared.Examine;
using Content.Shared.GameTicking;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._Starlight.Time
{
    public sealed partial class SharedTimeSystem : EntitySystem
    {
        [Dependency] private IGameTiming _timing = default!;
        
        private DateTime _date = DateTime.UtcNow.AddYears(300);

        private TimeSpan _roundStart;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeNetworkEvent<TickerLobbyStatusEvent>(LobbyStatus);
        }

        private void LobbyStatus(TickerLobbyStatusEvent ev)
        {
            _roundStart = ev.RoundStartTimeSpan;
        }

        public (TimeSpan Time, string Date) GetStationTime()
        {
            var scaledTimeSinceStart = _timing.CurTime.Subtract(_roundStart).Multiply(4);
            var stationTime = scaledTimeSinceStart.Add(TimeSpan.FromHours(12));

            var totalDays = (int) stationTime.TotalDays;
            stationTime = stationTime.Subtract(TimeSpan.FromDays(totalDays));

            var newDate = _date.AddDays(totalDays);

            // ISO 8601 (YYYY-MM-DD or YYYYMMDD)
            return (stationTime, newDate.ToString("yyyy-MM-dd"));
        }

        public string GetDate()
        {
            // ISO 8601 (YYYY-MM-DD or YYYYMMDD)
            return Date.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// Gets the ellapsed time of the round, useful for paperwork.
        /// </summary>
        public TimeSpan GetShiftDuration()
        {
            return _timing.CurTime - _roundStart;
        }
    }
}

/// <summary>
/// Dispatched to request the canonical round-start date from the server.
/// </summary>
[Serializable, NetSerializable]
public sealed class RequestRoundDateEvent : EntityEventArgs
{
}

/// <summary>
/// Dispatched by the server to inform clients of the canonical round-start date.
/// </summary>
[Serializable, NetSerializable]
public sealed class RoundDateSetEvent(DateTime date) : EntityEventArgs
{
    public DateTime Date { get; } = date;
}
