using System;
using System.Collections.Generic;

namespace DataLib
{
    [Serializable]
    public class Combo
    {
        public string DataClass { get; set; }
        public string ValueString { get; set; }
        public string CodeString { get; set; }
        public string EmailAddressString { get; set; }
    }
}
