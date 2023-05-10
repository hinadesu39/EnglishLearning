using ListeningDomain.Events;
using MediatR;
using Zack.EventBus;

namespace ListeningAdmin.WebAPI.EventHandlers
{
    public class EpisodeDeletedEventHandler : INotificationHandler<EpisodeDeletedEvent>
    {
        private readonly IEventBus eventBus;

        public EpisodeDeletedEventHandler(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public Task Handle(EpisodeDeletedEvent notification, CancellationToken cancellationToken)
        {
            var id = notification.Id;
            eventBus.Publish("ListeningEpisode.Deleted", new { Id = id });
            return Task.CompletedTask;
        }
    }
}
