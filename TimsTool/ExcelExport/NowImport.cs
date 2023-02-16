using System.Collections.Generic;

namespace ExportExcel
{
    public class NowImport
    {
        public int RowIndex { get; set; }
        public string Now { get; set; }
        public string NowTitle { get; set; }
        public string NowMatchedTitles { get; set; }
        public bool NowMatchesAreExact { get; set; }
        public string DuplicatedTitles { get; set; }
        public List<string> Results { get; set; }
        public bool ForceCreate { get; set; }
        public string SelectedForImport { get; set; }
        public List<MatchedResult> MatchedResults { get; set; }
        public List<string> UnmatchedResults { get; set; }
    }
}
