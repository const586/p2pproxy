using System;
using System.Net.Sockets;

namespace P2pProxy.Broadcasting.VLC
{
    public class VlcStopError : VlcError
    {
        public VlcStopError()
            : base(VlcBroadcaster.STOPERR)
        {

        }
    }
    public class VlcError : Exception
    {
        public VlcError(string message)
            : base(message)
        {

        }
    }
    public class VlcConnectError : VlcError
    {
        public VlcConnectError()
            : base(SocketError.ConnectionRefused.ToString())
        {

        }
    }
}