using MediaEncoderDomain.Entities;
using MediaEncoderInfrastructure;
using Microsoft.EntityFrameworkCore;
using Zack.EventBus;

namespace MediaEncoder.WebAPI.EventHandlers
{
    [EventName("MediaEncoding.Created")]
    public class MediaEncodingCreatedHandler : DynamicIntegrationEventHandler
    {
        private readonly IEventBus eventBus;
        private readonly MEDbContext dbContext;

        public MediaEncodingCreatedHandler(IEventBus eventBus, MEDbContext dbContext)
        {
            this.eventBus = eventBus;
            this.dbContext = dbContext;
        }

        public override async Task HandleDynamic(string eventName, dynamic eventData)
        {
            Guid mediaId = Guid.Parse(eventData.MediaId);
            Uri mediaUrl = new Uri(eventData.MediaUrl);
            string sourceSystem = eventData.SourceSystem;
            //mediaUrl是一个Uri对象，它表示一个URL。
            //mediaUrl.Segments返回一个字符串数组，其中包含URL的各个部分。
            //例如，对于URLhttp://example.com/path/to/file.txt，
            //Segments属性返回的数组为{ "/", "path/", "to/", "file.txt" }
            string fileName = mediaUrl.Segments.Last();
            string outputFormat = eventData.OutputFormat;
            //保证幂等性，如果这个路径对应的操作已经存在，则直接返回
            bool exists = await dbContext.EncodingItems
                .AnyAsync(e => e.SourceUrl == mediaUrl && e.OutputFormat == outputFormat);
            if (exists)
            {
                return;
            }
            var encodeItem = EncodingItem.Create(mediaId, fileName, mediaUrl, outputFormat, sourceSystem);
            dbContext.Add(encodeItem);
            await dbContext.SaveChangesAsync();
        }
    }
}
