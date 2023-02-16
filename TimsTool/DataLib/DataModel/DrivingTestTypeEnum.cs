using System;
using System.ComponentModel;

namespace DataLib
{
    [Serializable]
    public enum DrivingTestTypeEnum
    {
        [Description("No Test Required")]
        noTest = 0,

        [Description("Ordinary Test Required")]
        ordinaryTest = 1,

        [Description("Extended Test Required")]
        extendedTest = 4
    }
}