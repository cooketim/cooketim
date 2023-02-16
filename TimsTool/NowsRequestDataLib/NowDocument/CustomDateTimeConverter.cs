using Newtonsoft.Json.Converters;

namespace NowsRequestDataLib.NowDocument
{
    public class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public CustomDateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }

        public CustomDateTimeConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
