using NowsRequestDataLib.NowDocument;
using NowsRequestDataLib.Domain;

namespace NowsRequestDataLib
{
    public class NowData
    { 
        public NowDocumentRequest NowDocumentRequest { get; set; }
        public string NowDocumentRequestFileName { get; set; }
        public string NowDocumentRequestFileNameSuffix { get; set; }
        public string NowDocumentRequestErrorsFileName { get; set; }

        public HearingResults HearingResults { get; set; }
        public string HearingResultsFileName { get; set; }
        public string HearingResultsFileNameSuffix { get; set; }
    }
}
