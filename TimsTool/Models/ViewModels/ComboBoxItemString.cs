using System;
using System.Collections.Generic;
using System.Text;

namespace Models.ViewModels
{
    public class ComboBoxItemString
    {
        public string ValueString { get; set; }
        public string CodeString { get; set; }
        public string EmailAddressString { get; set; }

        public string CodeValueString
        {
            get
            {
                if (!string.IsNullOrEmpty(CodeString))
                {
                    return string.Format("{0} - {1}", CodeString, ValueString);
                }
                return ValueString;
            }
        }

        public bool CheckStatus { get; set; }

        public ComboBoxItemString Copy()
        {
            var res = (ComboBoxItemString)this.MemberwiseClone();
            res.CheckStatus = false;
            return res;
        }
    } 
}
