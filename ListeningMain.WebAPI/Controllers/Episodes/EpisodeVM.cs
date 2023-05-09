using CommonHelper;
using ListeningDomain.Entities;
using ListeningDomain.ValueObjects;

namespace ListeningMain.WebAPI.Controllers.Episodes
{
    public record EpisodeVM(Guid Id, MultilingualString Name,Guid AlbumId, Uri AudioUrl, double DurationInSecond, IEnumerable<SentenceVM>? Sentences)
    {
        public static EpisodeVM? create(Episode? episode,bool loadSubtitle) 
        {
            if (episode == null)
            {
                return null;
            }
            List<SentenceVM> sentenceVMs = new();
            if (loadSubtitle)
            {
                var sentences = episode.ParseSubtitle();
                foreach (Sentence s in sentences)
                {
                    SentenceVM vm = new SentenceVM(s.StartTime.TotalSeconds, s.EndTime.TotalSeconds, s.Value);
                    sentenceVMs.Add(vm);
                }
            }
            return new EpisodeVM(episode.Id, episode.Name, episode.AlbumId, episode.AudioUrl, episode.DurationInSecond, sentenceVMs);
        }

        public static EpisodeVM[] create(Episode[] episodes,bool loadSubtitle)
        {
            return episodes.Select(e => create(e,loadSubtitle)!).ToArray();   
        }
    }
}
