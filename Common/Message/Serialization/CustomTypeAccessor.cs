using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FastMember;

namespace DarkMultiPlayerCommon.Message.Serialization
{
    public class CustomTypeAccessor:TypeAccessor
    {
        public override object this[object target, string name]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
