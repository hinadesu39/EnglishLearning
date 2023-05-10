using CommonHelper;

namespace ListeningAdmin.WebAPI
{
    public record EncodingEpisodeInfo(Guid Id, MultilingualString Name, 
        Guid AlbumId, double DurationInSecond, 
        string Subtitle, string SubtitleType, string Status);
}
