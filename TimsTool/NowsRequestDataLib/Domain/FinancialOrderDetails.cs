using DataLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NowsRequestDataLib.Domain
{
    [Serializable]
    public class FinancialOrderDetails
    {
        [JsonProperty(PropertyName = "enforcementPhoneNumber")]
        public string EnforcementPhoneNumber { get;  }

        [JsonProperty(PropertyName = "enforcementAddress")]
        public NowAddress EnforcementAddress { get; }

        [JsonProperty(PropertyName = "accountingDivisionCode")]
        public string AccountingDivisionCode { get; }

        [JsonProperty(PropertyName = "accountPaymentReference")]
        public string AccountPaymentReference { get; }

        [JsonProperty(PropertyName = "bacsBankName")]
        public string BacsBankName { get; }

        [JsonProperty(PropertyName = "bacsSortCode")]
        public string BacsSortCode { get; }

        [JsonProperty(PropertyName = "bacsAccountNumber")]
        public string BacsAccountNumber { get; }

        [JsonProperty(PropertyName = "totalAmountImposed")]
        public string TotalAmountImposed { get; }

        [JsonProperty(PropertyName = "totalBalance")]
        public string TotalBalance { get; }

        [JsonProperty(PropertyName = "paymentTerms")]
        public string PaymentTerms { get; }

        public FinancialOrderDetails()
        {
            EnforcementPhoneNumber = "0300 790 9901";
            EnforcementAddress = new NowAddress
            (
                "Logan House", "Stuart Street", "Luton", "Bedfordshire", null, "LU1 5BL", "courtsrus@hmcts.net", null
            );
            AccountingDivisionCode = "CDE77";
            AccountPaymentReference = "govpay281907-A7H";
            BacsBankName = "NatWest Bank";
            BacsSortCode = "22-22-23";
            BacsAccountNumber = "67873245";
            TotalAmountImposed = "2790.89";
            TotalBalance = "1876.89";
            PaymentTerms = string.Empty;
        }
    }
}
