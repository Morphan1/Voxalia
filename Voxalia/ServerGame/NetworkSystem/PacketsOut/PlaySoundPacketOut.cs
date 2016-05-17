using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class PlaySoundPacketOut : AbstractPacketOut
    {
        public PlaySoundPacketOut(Server tserver, string sound, float vol, float pitch, Location pos)
        {
            ID = ServerToClientPacket.PLAY_SOUND;
            Data = new byte[4 + 4 + 4 + 12];
            Utilities.IntToBytes(tserver.Networking.Strings.IndexForString(sound)).CopyTo(Data, 0);
            Utilities.FloatToBytes(vol).CopyTo(Data, 4);
            Utilities.FloatToBytes(pitch).CopyTo(Data, 4 + 4);
            pos.ToBytes().CopyTo(Data, 4 + 4 + 4);
        }
    }
}
