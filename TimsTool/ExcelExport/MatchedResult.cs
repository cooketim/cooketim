using DataLib;
using System;

namespace ExportExcel
{
    public class MatchedResult
    {
        public string ProvidedCode { get; set; }
        public string ProvidedCJSCode { get; set; }
        public string MatchedOn { get; set; }
        public ResultDefinition ResultDefinition { get; set; }
    }
}
