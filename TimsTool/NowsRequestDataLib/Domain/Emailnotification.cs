using DataLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class EmailNotification
    {
        public string fromAddress { get; set; }
        public string toAddress { get; set; }
        public string bodyText { get; set; }
        public string subjectText { get; set; }
        public Guid? templateId { get; set; }
    }
}
