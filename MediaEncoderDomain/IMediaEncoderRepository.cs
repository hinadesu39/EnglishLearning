using MediaEncoderDomain.Entities;

namespace MediaEncoderDomain
{
    public interface IMediaEncoderRepository
    {
        Task<EncodingItem?> FindCompletedOneAsync(string fileHash, long fileSize);
        Task<EncodingItem[]> FindAsync(ItemStatus status);
    }
}