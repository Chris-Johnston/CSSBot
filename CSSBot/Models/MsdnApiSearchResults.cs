using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot
{
    [JsonObject]
    public class MsdnApiSearchResults
    {
        [JsonProperty("results")]
        public IReadOnlyList<MsdnApiResult> Results { get; set; }
    }
}
