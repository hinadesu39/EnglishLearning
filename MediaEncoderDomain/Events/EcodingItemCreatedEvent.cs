using MediaEncoderDomain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaEncoderDomain.Events
{
    public record EncodingItemCreatedEvent(EncodingItem Value) : INotification;
}
