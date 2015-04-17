using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsIn
{
    class KeysPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 2 + 4 + 4)
            {
                return false;
            }
            KeysPacketData val = (KeysPacketData)Utilities.BytesToUshort(Utilities.BytesPartial(data, 0, 2));
            Player.Forward = val.HasFlag(KeysPacketData.FORWARD);
            Player.Backward = val.HasFlag(KeysPacketData.BACKWARD);
            Player.Leftward = val.HasFlag(KeysPacketData.LEFTWARD);
            Player.Rightward = val.HasFlag(KeysPacketData.RIGHTWARD);
            Player.Upward = val.HasFlag(KeysPacketData.UPWARD);
            Player.Downward = val.HasFlag(KeysPacketData.DOWNWARD);
            SysConsole.Output(OutputType.INFO, "Pos: " + Player.GetPosition() + ", vel: " + Player.GetVelocity());
            Player.Network.SendPacket(new YourPositionPacketOut(Player.GetPosition(), Player.GetVelocity()));
            Player.Direction.X = Utilities.BytesToFloat(Utilities.BytesPartial(data, 2, 4));
            Player.Direction.Y = Utilities.BytesToFloat(Utilities.BytesPartial(data, 2 + 4, 4));
            return true;
        }
    }

    public enum KeysPacketData : ushort
    {
        FORWARD = 1,
        BACKWARD = 2,
        LEFTWARD = 4,
        RIGHTWARD = 8,
        UPWARD = 16,
        DOWNWARD = 32
    }
}
