using ListeningDomain.Events;
using MediatR;
using Zack.EventBus;

namespace ListeningAdmin.WebAPI.EventHandlers
{
    public class EpisodeUpdatedEventHandler : INotificationHandler<EpisodeUpdatedEvent>
    {
        private readonly IEventBus eventBus;

        public EpisodeUpdatedEventHandler(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public Task Handle(EpisodeUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var episode = notification.value;
            
            if (episode.IsVisible)
            {
                var sentences = episode.ParseSubtitle();
                eventBus.Publish("ListeningEpisode.Updated", new { Id = episode.Id, episode.Name, Sentences = sentences, episode.AlbumId, episode.Subtitle, episode.SubtitleType });
            }
            else
            {
                //被隐藏
                eventBus.Publish("ListeningEpisode.Hidden", new { Id = episode.Id });
            }
            return Task.CompletedTask;
        }
    }
}
