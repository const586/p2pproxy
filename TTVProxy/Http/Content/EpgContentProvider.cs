using System;
using System.IO;
using TTVApi;
using P2pProxy.Http.Server;

namespace P2pProxy.Http.Content
{
    public class EpgContentProvider : ContentProvider
    {
        private readonly P2pProxyDevice _device;
 
        public EpgContentProvider(P2pProxyDevice device)
        {
            _device = device;
            _device.Web.AddRouteUrl("/epg/", SendResponse, HttpMethod.Get);
        }
        public override string GetPlaylist(MyWebRequest req)
        {
            int id = 0;
            if (req.Parameters.ContainsKey("id"))
                id = int.Parse(req.Parameters["id"]);
            return new StreamReader(new TranslationEpg(id).Execute(_device.Proxy.SessionState.session, TypeResult.Xml)).ReadToEnd();
        }

        public override void Play(MyWebRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
