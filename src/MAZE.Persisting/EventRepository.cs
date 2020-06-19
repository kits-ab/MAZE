using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using GenericDataStructures;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using GameId = System.String;

namespace MAZE
{
    public class EventRepository : IDisposable
    {
        private readonly IOptions<EventStoreSettings> _eventStoreSettingsOptions;
        private readonly IEventStoreConnection _eventStoreConnection;

        public EventRepository(IOptions<EventStoreSettings> eventStoreSettingsOptions)
        {
            _eventStoreSettingsOptions = eventStoreSettingsOptions;
            var eventStoreSettings = eventStoreSettingsOptions.Value;

            var connectionSettings = ConnectionSettings.Create()
                .DisableServerCertificateValidation()
                .UseDebugLogger()
                .Build();

            _eventStoreConnection = EventStoreConnection.Create(connectionSettings, new Uri($"tcp://{eventStoreSettings.UserName}:{HttpUtility.UrlEncode(eventStoreSettings.Password)}@{eventStoreSettings.FullyQualifiedDomainName}:{eventStoreSettings.Port}"));
            _eventStoreConnection.ConnectAsync().Wait();
        }

        public void Dispose()
        {
            _eventStoreConnection.Close();
        }

        public async Task<Result<(IEnumerable<Event> Events, long Version), ReadGameError>> GetEventsAndVersionAsync(GameId gameId)
        {
            var readEvents = await _eventStoreConnection.ReadStreamEventsForwardAsync(gameId, 0, 4096, false, GetCredentials());

            if (readEvents == null || readEvents.Status == SliceReadStatus.StreamNotFound)
            {
                return ReadGameError.NotFound;
            }

            return (readEvents.Events.Select(resolvedEvent =>
            {
                var eventData = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
                var eventDataType = typeof(Event).Assembly.GetType(resolvedEvent.Event.EventType);
                if (eventDataType == null)
                {
                    throw new InvalidOperationException($"Encountered event of unknown type: {resolvedEvent.Event.EventType}");
                }

                var @event = JsonConvert.DeserializeObject(eventData, eventDataType);

                if (@event == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize event of type: {resolvedEvent.Event.EventType}");
                }

                return (Event)@event;
            }).ToList(), readEvents.LastEventNumber);
        }

        public async Task<Result<IEnumerable<Event>, ReadGameError>> GetEventsAsync(GameId id)
        {
            var result = await GetEventsAndVersionAsync(id);
            return result.Map(eventsAndVersion => new Result<IEnumerable<Event>, ReadGameError>(eventsAndVersion.Events), readGameError => readGameError);
        }

        public async Task AddEventAsync(GameId gameId, Event @event, long expectedVersion)
        {
            var eventPayload = CreateEventData(@event);
            await _eventStoreConnection.AppendToStreamAsync(gameId, expectedVersion, GetCredentials(), eventPayload);
        }

        public async Task AddEventsAsync(GameId gameId, IEnumerable<Event> events)
        {
            var eventPayload = events.Select(CreateEventData).ToList();
            await _eventStoreConnection.AppendToStreamAsync(gameId, ExpectedVersion.NoStream, eventPayload, GetCredentials());
        }

        private static EventData CreateEventData(Event @event)
        {
            var data = JsonConvert.SerializeObject(@event);
            const string metadata = "{}";
            return new EventData(Guid.NewGuid(), @event.GetType().FullName, true, Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(metadata));
        }

        private UserCredentials GetCredentials()
        {
            var eventStoreSettings = _eventStoreSettingsOptions.Value;
            return new UserCredentials(eventStoreSettings.UserName, eventStoreSettings.Password);
        }
    }
}
