using ListeningAdmin.WebAPI.Hubs;
using ListeningDomain;
using ListeningDomain.Entities;
using ListeningInfrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Zack.EventBus;

namespace ListeningAdmin.WebAPI.EventHandlers
{

    //收听转码服务发出的集成事件
    //把状态通过SignalR推送给客户端，从而显示“转码进度”
    [EventName("MediaEncoding.Started")]
    [EventName("MediaEncoding.Failed")]
    [EventName("MediaEncoding.Duplicated")]
    [EventName("MediaEncoding.Completed")]
    public class MediaEncodingStatusChangeIntegrationHandler : DynamicIntegrationEventHandler
    {
        private readonly ListeningDbContext dbContext;
        private readonly IListeningRepository repository;
        private readonly EncodingEpisodeHelper encHelper;
        private readonly IHubContext<EpisodeEncodingStatusHub> hubContext;

        public MediaEncodingStatusChangeIntegrationHandler(ListeningDbContext dbContext,
            EncodingEpisodeHelper encHelper,
            IHubContext<EpisodeEncodingStatusHub> hubContext, IListeningRepository repository)
        {
            this.dbContext = dbContext;
            this.encHelper = encHelper;
            this.hubContext = hubContext;
            this.repository = repository;
        }
        public override async Task HandleDynamic(string eventName, dynamic eventData)
        {

            string sourceSystem = eventData.sourceSystem;
            if(sourceSystem != "Listening")
            {
                return;
            }
            Guid id = Guid.Parse(eventData.Id);//EncodingItem的Id就是Episode 的Id
            switch (eventName)
            {
                case "MediaEncoding.Started":
                    await encHelper.UpdateEpisodeStatusAsync(id, "Started");
                    await hubContext.Clients.All.SendAsync("OnMediaEncodingStarted", id);//通知前端刷新
                    break;
                case "MediaEncoding.Failed":
                    await encHelper.UpdateEpisodeStatusAsync(id, "Failed");
                    await hubContext.Clients.All.SendAsync("OnMediaEncodingFailed", id);//通知前端刷新
                    break;
                case "MediaEncoding.Duplicated":
                    await encHelper.UpdateEpisodeStatusAsync(id, "Completed");
                    await hubContext.Clients.All.SendAsync("OnMediaEncodingCompleted", id);//通知前端刷新
                    break;
                case "MediaEncoding.Completed":
                    await encHelper.UpdateEpisodeStatusAsync(id, "Completed");
                    await hubContext.Clients.All.SendAsync("OnMediaEncodingCompleted", id);//通知前端刷新
                    Uri OutPutUrl = new Uri(eventData.OutputUrl);
                    //从redis中取出待转码的episode
                    var encItem = await encHelper.GetEncodingEpisodeAsync(id);
                    Guid albumId = encItem.AlbumId;
                    int maxSeq = await repository.GetMaxSeqOfAlbumsAsync(albumId);
                    Episode episode = Episode.Create(id, maxSeq + 1, encItem.Name, albumId, OutPutUrl,
                    encItem.DurationInSecond, encItem.SubtitleType, encItem.Subtitle);
                    //真正存到数据库当中
                    await dbContext.SaveChangesAsync();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventName));
            }
        }
    }
}
