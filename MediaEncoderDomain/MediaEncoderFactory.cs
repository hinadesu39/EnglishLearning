using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaEncoderDomain
{
    public class MediaEncoderFactory
    {
        private readonly IEnumerable<IMediaEncoder> encoders;
        public MediaEncoderFactory(IEnumerable<IMediaEncoder> encoders)
        {
            this.encoders = encoders;
        }

        public IMediaEncoder? Create(string outputFormat)
        {
            foreach (var encoder in encoders)
            {
                if (encoder.Accept(outputFormat))
                {
                    return encoder;
                }
            }
            return null;
        }
    }
}
