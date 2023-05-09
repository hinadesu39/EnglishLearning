using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelper
{
    public class RabbitMQConsumer
    {
        private readonly IOptions<IntegrationEventRabbitMQOptions> option;
    }
}
