using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLib
{
    [Serializable]
    public sealed class IdMap
    {
        public List<IdMapItem> Map { get; set; }

        [JsonIgnore]
        public bool IsChanged { get; set; }

        private int GetNextSequence(IdType idType)
        {
            var res = Map.Where(x => x.IdType == idType).Count() == 0 ? 0: Map.Where(x => x.IdType == idType).Max(x => x.FileNum);
            return res+1;
        }

        public int GetOrAddFileNum(Guid primaryId, IdType mapType)
        {
            var res = Map.FirstOrDefault(x => x.PrimaryId == primaryId && x.IdType == mapType);
            if (res == null)
            {
                var fileNum = GetNextSequence(mapType);
                res = new IdMapItem() { PrimaryId = primaryId, IdType = mapType, FileNum = fileNum };
                Map.Add(res);
                IsChanged = true;
            }
           
            return res.FileNum;
        }

        public int PurgeMap(IdType mapType)
        {
            return Map.RemoveAll(x=>x.IdType == mapType);
        }
    }

    [Serializable]
    public sealed class IdMapItem
    {
        public Guid PrimaryId { get; set; }
        public IdType IdType { get; set; }
        public int FileNum { get; set; }
    }

    [Serializable]
    public enum IdType
    {
        FixedList = 0,
        Now = 1,
        ResultDefinitionRule = 2,
        ResultDefinition = 3,
        NowRequirement = 4,
        NowSubscription = 5
    }
}