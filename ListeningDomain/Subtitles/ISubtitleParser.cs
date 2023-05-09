using ListeningDomain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListeningDomain.Subtitles
{
    interface  ISubtitleParser
    {
        /// <summary>
        /// 本解析器是否能够解析typeName这个类型的字幕
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        bool Accept(string typeName);

        /// <summary>
        /// 解析这个字幕subtitle
        /// </summary>
        /// <param name="subtitle"></param>
        /// <returns></returns>
        IEnumerable<Sentence> Parse(string subtitle);
    }
}
