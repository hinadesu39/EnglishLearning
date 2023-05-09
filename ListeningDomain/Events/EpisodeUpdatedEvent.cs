using ListeningDomain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListeningDomain.Events
{
    public record EpisodeUpdatedEvent(Episode value) : INotification;
  
}
