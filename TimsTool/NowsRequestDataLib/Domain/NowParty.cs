using Newtonsoft.Json;
using System;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class NowParty
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; }

        [JsonProperty(PropertyName = "middleName")]
        public string MiddleName { get; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; }

        [JsonProperty(PropertyName = "address")]
        public NowAddress Address { get; }

        [JsonProperty(PropertyName = "dateOfBirth")]
        public DateTime? DateOfBirth { get; }

        [JsonProperty(PropertyName = "mobileNumber")]
        public string MobileNumber { get; private set; }

        [JsonProperty(PropertyName = "landlineNumber")]
        public string PhoneNumber { get; private set; }

        public NowParty(string name, NowAddress address, DateTime? dateOfBirth, string mobileNumber, string phoneNumber)
        {
            Name = name;
            Address = address;
            DateOfBirth = dateOfBirth;
            MobileNumber = mobileNumber;
            PhoneNumber = phoneNumber;
        }
    }
}
