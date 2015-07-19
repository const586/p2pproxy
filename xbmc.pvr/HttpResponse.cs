using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginProxy;

namespace xbmc.pvr
{
    class HttpResponse : IRequestData
    {
        public Stream Stream;
        public Dictionary<string, string> Headers { get; private set; }

        public HttpResponse()
        {
            Headers = new Dictionary<string, string>();
        }

        public Stream GetStream()
        {
            return Stream;
        }

        public void SetResultState(ushort value)
        {
            ResultState = value;
        }

        public ushort ResultState { get; private set; }
    }
}
