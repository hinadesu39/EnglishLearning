using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchServiceDomain.Entities
{
    public record Episode(Guid Id, string CnName, string EngName, string PlainSubtitle, Guid AlbumId);
}
