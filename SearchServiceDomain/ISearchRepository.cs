using SearchServiceDomain.Entities;

namespace SearchServiceDomain
{
    public interface ISearchRepository
    {
        public Task UpsertAsync(Episode episode);
        public Task DeleteAsync(Guid episodeId);
        public Task<SearchEpisodesResponse> SearchEpisodes(string keyWord, int pageIndex, int PageSize);
    }
}