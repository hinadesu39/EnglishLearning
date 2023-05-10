using ListeningDomain.Entities;
using StackExchange.Redis;

namespace ListeningAdmin.WebAPI
{
    public class EncodingEpisodeHelper
    {
        private readonly IConnectionMultiplexer redisConn;

        public EncodingEpisodeHelper(IConnectionMultiplexer redisConn)
        {
            this.redisConn = redisConn;
        }

        //一个kv对中保存这个albumId下所有的转码中的episodeId
        private static string GetKeyForEncodingEpisodeIdsOfAlbum(Guid albumId)
        {
            return $"Listening.EncodingEpisodeIdsOfAlbum.{albumId}";
        }
        private static string GetStatusKeyForEpisode(Guid episodeId)
        {
            string redisKey = $"Listening.EncodingEpisode.{episodeId}";
            return redisKey;
        }
        /// <summary>
        /// 增加待转码的任务的详细信息
        /// </summary>
        /// <param name="albumId"></param>
        /// <param name="episode"></param>
        /// <returns></returns>
        public async Task AddEncodingEpisodeAsync(Guid episodeId, EncodingEpisodeInfo episode)
        {
            string redisKeyForEpisode = GetStatusKeyForEpisode(episodeId);
            string keyForEncodingEpisodeIdsOfAlbum = GetKeyForEncodingEpisodeIdsOfAlbum(episode.AlbumId);

            //StringSetAsync 用于存储字符串值，而 SetAddAsync 用于向集合中添加元素。
            var db = redisConn.GetDatabase();
            await db.StringSetAsync(redisKeyForEpisode, episode.ToJsonString());//保存转码任务详细信息，供完成后插入数据库          
            await db.SetAddAsync(keyForEncodingEpisodeIdsOfAlbum, episodeId.ToString());//保存这个album下所有待转码的episodeId
        }

        /// <summary>
        /// 获取这个albumId下所有转码任务
        /// </summary>
        /// <param name="albumId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Guid>> GetEncodingEpisodeIdsAsync(Guid albumId)
        {
            string keyForEncodingEpisodeIdsOfAlbum = GetKeyForEncodingEpisodeIdsOfAlbum(albumId);
            var db = redisConn.GetDatabase();
            //SetMembersAsync 方法从 Redis 数据库中查询指定键下的集合中的所有元素
            var values = await db.SetMembersAsync(keyForEncodingEpisodeIdsOfAlbum);
            return values.Select(v => Guid.Parse(v));
        }

        /// <summary>
        /// 删除一个Episode任务
        /// </summary>
        /// <param name="db"></param>
        /// <param name="episodeId"></param>
        /// <param name="albumId"></param>
        /// <returns></returns>
        public async Task RemoveEncodingEpisodeAsync(Guid episodeId, Guid albumId)
        {
            string redisKeyForEpisode = GetStatusKeyForEpisode(episodeId);
            string keyForEncodingEpisodeIdsOfAlbum = GetKeyForEncodingEpisodeIdsOfAlbum(albumId);
            var db = redisConn.GetDatabase();
            await db.SetRemoveAsync(keyForEncodingEpisodeIdsOfAlbum, episodeId.ToString());
            await db.KeyDeleteAsync(redisKeyForEpisode);

        }

        /// <summary>
        /// 修改Episode的转码状态
        /// </summary>
        /// <param name="db"></param>
        /// <param name="episodeId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task UpdateEpisodeStatusAsync(Guid episodeId, string status)
        {
            string redisKeyForEpisode = GetStatusKeyForEpisode(episodeId);
            var db = redisConn.GetDatabase();
            string json = await db.StringGetAsync(redisKeyForEpisode);
            EncodingEpisodeInfo episode = json.ParseJson<EncodingEpisodeInfo>()!;
            //with 表达式用于基于现有对象创建一个新对象，同时更改指定的属性值。
            //它通常用于不可变记录类型，可以让你轻松地创建一个新的记录实例，而无需显式地设置所有属性值。
            episode = episode with { Status = status };
            await db.StringSetAsync(redisKeyForEpisode, episode.ToJsonString());
        }

        /// <summary>
        /// 获得Episode的转码状态
        /// </summary>
        /// <param name="db"></param>
        /// <param name="episodeId"></param>
        /// <returns></returns>
        public async Task<EncodingEpisodeInfo> GetEncodingEpisodeAsync(Guid episodeId)
        {
            string redisKeyForEpisode = GetStatusKeyForEpisode(episodeId);
            var db = redisConn.GetDatabase();
            string json = await db.StringGetAsync(redisKeyForEpisode);
            EncodingEpisodeInfo episode = json.ParseJson<EncodingEpisodeInfo>()!;
            return episode;
        }
    }
}
