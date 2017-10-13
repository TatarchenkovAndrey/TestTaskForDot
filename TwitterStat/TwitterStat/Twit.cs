using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TwitterStat
{
    class Twit
    {
        [JsonProperty("text")]
        public virtual string Text { get; set; }
    }
}
