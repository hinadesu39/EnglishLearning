using CommonHelper;
using ListeningDomain.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ListeningDomain
{
    public class ListeningDomainService
    {
        private readonly IListeningRepository listeningRepository;

        public ListeningDomainService(IListeningRepository listeningRepository)
        {
            this.listeningRepository = listeningRepository;
        }


        public async Task<Category> AddCategory(MultilingualString name, Uri coverUrl)
        {
            int maxSeq = await listeningRepository.GetMaxSeqOfCategoriesAsync();
            var id = Guid.NewGuid();
            return Category.Create(id,maxSeq + 1,name,coverUrl);   
        }

        public async Task SortCategory(Guid[] sortedCategoryIds)
        {
            var categories = await listeningRepository.GetCategoriesAsync();
            var idsInDB = categories.Select(c => c.Id);
            if (!idsInDB.OrderBy(a => a).SequenceEqual(sortedCategoryIds.OrderBy(a => a)))
            {
                throw new Exception($"提交的待排序Id中必须是categoryId下所有的Id");
            }

            int seqNum = 1;
            //一个in语句一次性取出来更快，不过在非性能关键节点，业务语言比性能更重要
            foreach (Guid categoryId in sortedCategoryIds)
            {
                var category = await listeningRepository.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    throw new Exception($"categoryId={categoryId}不存在");
                }
                category.ChangeSequenceNumber(seqNum);//顺序改序号
                seqNum++;
            }
        }

        public async Task<Album> AddAlbumAsync(Guid categoryId, MultilingualString title)
        {
            int maxSeq = await listeningRepository.GetMaxSeqOfAlbumsAsync(categoryId);
            Guid id = Guid.NewGuid();
            return Album.Create(id, maxSeq + 1, title, categoryId);
        }

        public async Task SortAlbumsAsync(Guid categoryId, Guid[] sortedAlbumIds)
        {
            var albums = await listeningRepository.GetAlbumsByCategoryIdAsync(categoryId);
            var idsInDB = albums.Select(a => a.Id);
            if (!idsInDB.OrderBy(a=>a).SequenceEqual(sortedAlbumIds.OrderBy(a=>a)))
            {
                throw new Exception($"提交的待排序Id中必须是categoryId={categoryId}分类下所有的Id");
            }

            int seqNum = 1;
            //一个in语句一次性取出来更快，不过在非性能关键节点，业务语言比性能更重要
            foreach (Guid albumId in sortedAlbumIds)
            {
                var album = await listeningRepository.GetAlbumByIdAsync(albumId);
                if (album == null)
                {
                    throw new Exception($"albumId={albumId}不存在");
                }
                album.ChangeSequenceNumber(seqNum);//顺序改序号
                seqNum++;
            }
        }

        public async Task<Episode> AddEpisodeAsync(MultilingualString name,
            Guid albumId, Uri audioUrl, double durationInSecond,
            string subtitleType, string subtitle)
        {
            int maxSeq = await listeningRepository.GetMaxSeqOfEpisodesAsync(albumId);
            var id = Guid.NewGuid();
            
            Episode episode = Episode.Create(id, maxSeq + 1, name, albumId,
                audioUrl,durationInSecond, subtitleType, subtitle);
            return episode;
        }

        public async Task SortEpisodesAsync(Guid albumId, Guid[] sortedEpisodeIds)
        {
            var episodes = await listeningRepository.GetEpisodesByAlbumIdAsync(albumId);
            var idsInDB = episodes.Select(e => e.Id);
            if (!idsInDB.OrderBy(a => a).SequenceEqual(sortedEpisodeIds.OrderBy(a => a)))
            {
                throw new Exception($"提交的待排序Id中必须是albumId={albumId}分类下所有的Id");
            }
            int seqNum = 1;
            foreach (Guid episodeId in sortedEpisodeIds)
            {
                var episode = await listeningRepository.GetEpisodeByIdAsync(episodeId);
                if (episode == null)
                {
                    throw new Exception($"episodeId={episodeId}不存在");
                }
                episode.ChangeSequenceNumber(seqNum);//顺序改序号
                seqNum++;
            }
        }
    }
}