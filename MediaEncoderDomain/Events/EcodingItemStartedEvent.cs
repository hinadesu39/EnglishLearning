using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaEncoderDomain.Events
{
    public record EncodingItemStartedEvent(Guid Id, string SourceSystem) : INotification;
}
