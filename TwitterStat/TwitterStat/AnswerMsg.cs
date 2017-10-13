using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace TwitterStat
{
    class AnswerMsg
    {
        public virtual char Letter { get; set; }
        public virtual int Count { get; set; }

        public virtual string Rate(int messageValue)
        {
            var result= $"{(double) Count / messageValue:0.#####}";
            return result;
        }
    }
}
