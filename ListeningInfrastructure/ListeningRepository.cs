using ListeningDomain;
using ListeningDomain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ListeningInfrastructure
{
    public class ListeningRepository : IListeningRepository
    {
        public readonly ListeningDbContext Context;

        public ListeningRepository(ListeningDbContext context)
        {
            Context = context;
        }

        public async Task<Album?> GetAlbumByIdAsync(Guid albumId)
        {
            return await Context.FindAsync<Album>(albumId);

        }

        public async Task<Album[]> GetAlbumsByCategoryIdAsync(Guid categoryId)
        {
            return await Context.Albums.Where(a => a.CategoryId == categoryId).OrderBy(a => a.SequenceNumber).ToArrayAsync();
        }

        public async Task<Category[]> GetCategoriesAsync()
        {
            return await Context.Categories.OrderBy(a => a.SequenceNumber).ToArrayAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            return await Context.FindAsync<Category>(categoryId);
        }

        public async Task<Episode?> GetEpisodeByIdAsync(Guid episodeId)
        {
            return await Context.FindAsync<Episode>(episodeId);
        }

        public async Task<Episode[]> GetEpisodesByAlbumIdAsync(Guid albumId)
        {
            return await Context.Episodes.Where(e => e.AlbumId == albumId).ToArrayAsync();
        }

        public async Task<int> GetMaxSeqOfAlbumsAsync(Guid categoryId)
        {
            int? maxSeq = await Context.Albums
                .Where(a => a.CategoryId == categoryId)
                .Select(a => (int?)a.SequenceNumber)
                .MaxAsync();
            return maxSeq ?? 0;
        }

        public async Task<int> GetMaxSeqOfCategoriesAsync()
        {
            int? maxSeq = await Context.Categories
                .Select(a => (int?)a.SequenceNumber)
                .MaxAsync();
            return maxSeq ?? 0;
        }

        public async Task<int> GetMaxSeqOfEpisodesAsync(Guid albumId)
        {
            int? maxSeq = await Context.Episodes
                .Where(e => e.AlbumId == albumId)
                .Select (a => (int?)a.SequenceNumber)
                .MaxAsync();
            return maxSeq ?? 0;
        }
    }
}
