using System;
using System.Collections.Generic;
using System.Text;

namespace DataLib
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreChangesAttribute : Attribute
    {
        public IgnoreChangesAttribute() { }
    }
}
