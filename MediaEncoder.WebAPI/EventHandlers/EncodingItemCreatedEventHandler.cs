using MediaEncoderDomain.Events;
using MediatR;
using Zack.EventBus;

namespace MediaEncoder.WebAPI.EventHandlers
{
    class EncodingItemCreatedEventHandler : INotificationHandler<EncodingItemCreatedEvent>
    {
        private readonly IEventBus eventBus;

        public EncodingItemCreatedEventHandler(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public Task Handle(EncodingItemCreatedEvent notification, CancellationToken cancellationToken)
        {
            eventBus.Publish("MediaEncoding.Created", notification);
            return Task.CompletedTask;
        }
    }
}
