using Nest;
using SearchServiceDomain;
using SearchServiceDomain.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace SearchServiceInfrastructure
{
    public class SearchRepository : ISearchRepository
    {
        private readonly IElasticClient elasticClient;

        public SearchRepository(IElasticClient elasticClient)
        {
            this.elasticClient = elasticClient;
        }
        public async Task UpsertAsync(Episode episode)
        {
            var response = await elasticClient.IndexAsync(episode, idx => idx.Index("episodes").Id(episode.Id));
            if (!response.IsValid)
            {
                throw new ApplicationException(response.DebugInformation);
            }
        }
        public Task DeleteAsync(Guid episodeId)
        {
            return elasticClient.DeleteAsync(new DeleteRequest("episodes", episodeId));
        }

        public async Task<SearchEpisodesResponse> SearchEpisodes(string keyWord, int pageIndex, int PageSize)
        {
            int from = PageSize * (pageIndex - 1);

            //定义查询，只要CnName，EngName，PlainSubtitle中有一个符合就符合
            Func<QueryContainerDescriptor<Episode>, QueryContainer> query = (q) =>
              q.Match(mq => mq.Field(f => f.CnName).Query(keyWord))
              || q.Match(mq => mq.Field(f => f.EngName).Query(keyWord))
              || q.Match(mq => mq.Field(f => f.PlainSubtitle).Query(keyWord));

            //定义PlainSubtitle字段高亮
            Func<HighlightDescriptor<Episode>, IHighlight> highlightSelector = h => h
             .Fields(fs => fs.Field(f => f.PlainSubtitle));

            var result = await this.elasticClient.SearchAsync<Episode>(s => s.Index("episodes").From(from)
                .Size(PageSize).Query(query).Highlight(highlightSelector));
            if (!result.IsValid)
            {
                throw result.OriginalException;
            }

            List<Episode> episodes = new List<Episode>();
            foreach (var hit in result.Hits)
            {
                string highlightedSubtitle;
                //如果没有预览内容，则显示前50个字
                if (hit.Highlight.ContainsKey("plainSubtitle"))
                {
                    highlightedSubtitle = string.Join("\r\n", hit.Highlight["plainSubtitle"]);
                }
                else
                {
                    highlightedSubtitle = hit.Source.PlainSubtitle.Cut(50);
                }
                var episode = hit.Source with { PlainSubtitle = highlightedSubtitle };
                episodes.Add(episode);
            }
            return new SearchEpisodesResponse(episodes, result.Total);

        }


    }
}