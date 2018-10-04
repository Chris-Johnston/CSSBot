using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot
{
    [JsonObject]
    public class MsdnApiResult
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("itemType")]
        public string ItemType { get; set; }
        [JsonProperty("itemKind")]
        public string ItemKind { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }

    }
}
