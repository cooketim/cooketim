using Newtonsoft.Json;
using System;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class NowSolicitor
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        [JsonProperty(PropertyName = "address")]
        public NowAddress Address { get; }

        public NowSolicitor(string name, NowAddress address)
        {
            Name = name;
            Address = address;
        }
    }
}
