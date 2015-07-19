using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace P2pProxy.UPNP
{
    class AliasAttribute : Attribute
    {
        public string Name { get; set; }

        public AliasAttribute(string name)
        {
            Name = name;
        }
    }
}
