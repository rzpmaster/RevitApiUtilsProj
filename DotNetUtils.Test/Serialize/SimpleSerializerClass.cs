using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.Test.Serialize
{
    public class SimpleSerializerClass
    {
        public string StringProperty { get; set; }

        public bool BoolProperty { get; set; }

        public override bool Equals(object obj)
        {
            if (obj==null||obj.GetType()!=GetType())
            {
                return false;
            }

            SimpleSerializerClass other = obj as SimpleSerializerClass;

            return other.StringProperty == StringProperty && other.BoolProperty == BoolProperty;
        }

        public override int GetHashCode()
        {
            return (StringProperty+ BoolProperty).GetHashCode();
        }
    }
}
