using System;

namespace ExportExcel
{
    public class ResultDefinitionWordSynonym
    {
        public Guid ResultDefinitionId { get; set; }

        public string ResultDefinitionLabel { get; set; }

        public string ResultDefinitionShortCode { get; set; }

        public ResultDefinitionWordGroup WordGroup { get; set; }
    }

    public class ResultPromptWordSynonym
    {
        public Guid ResultPromptId { get; set; }

        public string ResultPromptLabel { get; set; }

        public ResultPromptWord ResultPromptWord { get; set; }
    }

    public class ResultPromptWord
    {
        public Guid? ResultPromptWordId { get; set; }

        public string Word { get; set; }

        public string Synonyms { get; set; }
    }
}
