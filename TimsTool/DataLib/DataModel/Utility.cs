using System;
using System.Collections.Generic;
using System.Text;

namespace DataLib
{
    [Serializable]
    public class Utility
    {
        public DataPatch DataPatch { get; set; }
        public Guid? ResultDefinitionSynonymCollectionId { get; set; }
        public Guid? ResultPromptSynonymCollectionId { get; set; }
        public string IdMap { get; set; }
        public List<string> PublicationTags { get; set; }
    }
}
