using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaEncoderDomain.Events
{
    public record EncodingItemFailedEvent(Guid Id, string SourceSystem, string ErrorMessage) : INotification;
}
