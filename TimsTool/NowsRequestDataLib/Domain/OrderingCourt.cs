using Newtonsoft.Json;

namespace NowsRequestDataLib.Domain
{
    public class OrderingCourt
    {
        [JsonProperty(PropertyName = "ljaCode")]
        public string LjaCode { get; private set; }

        [JsonProperty(PropertyName = "ljaName")]
        public string LjaName { get; private set; }

        [JsonProperty(PropertyName = "welshLjaName")]
        public string WelshLjaName { get; private set; }

        [JsonProperty(PropertyName = "courtCentreName")]
        public string CourtCentreName { get; private set; }

        [JsonProperty(PropertyName = "welshCourtCentreName")]
        public string WelshCourtCentreName { get; private set; }

        [JsonProperty(PropertyName = "address")]
        public NowAddress Address { get; }

        [JsonProperty(PropertyName = "welshAddress")]
        public NowAddress WelshAddress { get; }

        [JsonProperty(PropertyName = "welshCourtCentre")]
        public bool WelshCourtCentre { get; set; }

        public OrderingCourt(bool isCrown, bool welshCourtCentre)
        {
            LjaCode = isCrown ? null : "MERS";
            LjaName = isCrown ? null : "Merseyside";
            WelshLjaName = isCrown ? null : "Welsh ~ Merseyside";
            CourtCentreName = isCrown ? "Liverpool Crown Court" : "Liverpool Magistrates' Court";
            WelshCourtCentreName = "Welsh ~ " + CourtCentreName;

            Address = new NowAddress("Liverpool Combined Court Centre", "Derby Square", "Liverpool", "Merseyside", null, "L2 1XA", null, null);
            WelshAddress = new NowAddress("Welsh~Liverpool Combined Court Centre", "Welsh~Derby Square", "Welsh~Liverpool", "Welsh~Merseyside", null, "L2 1XA", null, null);
            WelshCourtCentre = welshCourtCentre;
        }
    }
}
