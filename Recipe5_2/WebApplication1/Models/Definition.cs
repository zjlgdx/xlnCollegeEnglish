using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recipe5_2
{
    class Definition
    {
        /// <summary>
        /// 词性
        /// </summary>
        public string PartofSpeech { get; set; }
        public string EnglishParaphrase { get; set; }
        public string ChineseParaphrase { get; set; }
        public string Phrase { get; set; }
        public ICollection<Example> Examples { get; set; }
    }

    class Example
    {
        public string Sentence { get; set; }
        public string Translate { get; set; }
    }
}
