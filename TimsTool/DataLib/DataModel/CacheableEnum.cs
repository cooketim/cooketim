using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DataLib
{
    [Serializable]
    public enum CacheableEnum
    {
        [Description("Not Cached")]
        notCached = 0,

        [Description("Cached With Storage")]
        cacheWithStorage = 1,

        [Description("Cached Without Storage")]
        cacheWithoutStorage = 2
    }
}