using System;
using Voxalia.ClientGame.UISystem;
using Voxalia.Shared.Files;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    class MessagePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length < 1)
            {
                return false;
            }
            TextChannel tc = (TextChannel)data[0];
            TheClient.WriteMessage(tc, FileHandler.encoding.GetString(data, 1, data.Length - 1));
            return true;
        }
    }
}
