using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ServerGame.NetworkSystem
{
    public class NetStringManager
    {
        public Server TheServer;

        public NetStringManager(Server tserver)
        {
            TheServer = tserver;
        }

        public List<string> Strings = new List<string>();

        public int IndexForString(string str)
        {
            int i = Strings.IndexOf(str);
            if (i < 0 || i >= Strings.Count)
            {
                Strings.Add(str);
                TheServer.SendToAll(new NetStringPacketOut(str));
                return Strings.Count - 1;
            }
            else
            {
                return i;
            }
        }

        public string StringForIndex(int ind)
        {
            if (ind < 0 || ind >= Strings.Count)
            {
                return "";
            }
            return Strings[ind];
        }
    }
}
