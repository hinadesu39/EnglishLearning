using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListeningDomain.ValueObjects
{
    public record Sentence(TimeSpan StartTime, TimeSpan EndTime, string Value);
}
