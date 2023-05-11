using MediaEncoderDomain;
using MediaEncoderDomain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaEncoderInfrastructure
{
    public class MediaEncoderRepository : IMediaEncoderRepository
    {
        private readonly MEDbContext dbContext;

        public MediaEncoderRepository(MEDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<EncodingItem[]> FindAsync(ItemStatus status)
        {
            return dbContext.EncodingItems.Where(e => e.Status == ItemStatus.Ready).ToArrayAsync();
        }

        public Task<EncodingItem?> FindCompletedOneAsync(string fileHash, long fileSize)
        {
            return dbContext.EncodingItems.FirstOrDefaultAsync(e => e.FileSHA256Hash == fileHash && e.FileSizeInBytes == fileSize && e.Status == ItemStatus.Completed);
        }
    }
}
