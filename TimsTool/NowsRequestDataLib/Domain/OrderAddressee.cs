using Newtonsoft.Json;
using System;


namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class OrderAddressee
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        [JsonProperty(PropertyName = "address")]
        public NowAddress Address { get; }

        public OrderAddressee(string name, NowAddress nowAddress)
        {
            Name = name;
            Address = nowAddress;
        }

        public static OrderAddressee BuildForNoSubscriber()
        {
            var nowAddress = new NowAddress
                (
                    "No Subscriber - line1", "No Subscriber - line2", "No Subscriber - line3", "No Subscriber - line4", "No Subscriber - line5", "No Subscriber - post Code", "No Subscriber - email address1", "No Subscriber - email address2"
                );
            return new OrderAddressee("No Subscriber", nowAddress);
        }
    }
}
