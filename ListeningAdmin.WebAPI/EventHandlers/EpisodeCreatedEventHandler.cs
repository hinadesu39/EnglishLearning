using ListeningDomain.Events;
using ListeningDomain.ValueObjects;
using MediatR;
using Zack.EventBus;

namespace ListeningAdmin.WebAPI.EventHandlers
{
    public class EpisodeCreatedEventHandler : INotificationHandler<EpisodeCreatedEvent>
    {
        private readonly IEventBus eventBus;

        public EpisodeCreatedEventHandler(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public Task Handle(EpisodeCreatedEvent notification, CancellationToken cancellationToken)
        {
            //把领域事件转发为集成事件，让别的服务听到
            var episode = notification.value;
            var sentences = episode.ParseSubtitle();
            eventBus.Publish("ListeningEpisode.Created", new { Id = episode.Id, episode.Name, Sentences = sentences, episode.AlbumId, episode.Subtitle, episode.SubtitleType });//发布集成事件，实现搜索索引、记录日志等功能
            return Task.CompletedTask;
        }
    }
}
