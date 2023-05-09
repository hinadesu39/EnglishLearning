using FileServiceDomain;
using FileServiceDomain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileServiceInfrastrucure
{
    public class FSRepository : IFSRepository
    {
        private readonly FSDBContext _dbContext;

        public FSRepository(FSDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<UploadedItem?> FindFileAsync(long fileSize, string sha256Hash)
        {
            return _dbContext.UploadItems.FirstOrDefaultAsync(u => u.FileSizeInBytes == fileSize
            && u.FileSHA256Hash == sha256Hash);
        }
    }
}
