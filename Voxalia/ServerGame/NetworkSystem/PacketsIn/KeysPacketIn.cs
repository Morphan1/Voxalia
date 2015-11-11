using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.NetworkSystem.PacketsIn
{
    class KeysPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 2 + 4 + 4)
            {
                return false;
            }
            long tid = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            KeysPacketData val = (KeysPacketData)Utilities.BytesToUshort(Utilities.BytesPartial(data, 8, 2));
            Player.Forward = val.HasFlag(KeysPacketData.FORWARD);
            Player.Backward = val.HasFlag(KeysPacketData.BACKWARD);
            Player.Leftward = val.HasFlag(KeysPacketData.LEFTWARD);
            Player.Rightward = val.HasFlag(KeysPacketData.RIGHTWARD);
            Player.Upward = val.HasFlag(KeysPacketData.UPWARD);
            Player.Walk = val.HasFlag(KeysPacketData.WALK);
            Player.Click = val.HasFlag(KeysPacketData.CLICK);
            Player.AltClick = val.HasFlag(KeysPacketData.ALTCLICK);
            Player.Network.SendPacket(new YourPositionPacketOut(tid, Player.GetPosition(), Player.GetVelocity(), new Location(0, 0, 0), Player.CBody.StanceManager.CurrentStance));
            Player.Direction.Yaw = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 2, 4));
            Player.Direction.Pitch = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 2 + 4, 4));
            return true;
        }
    }

    public enum KeysPacketData : ushort // TODO: Network enum?
    {
        FORWARD = 1,
        BACKWARD = 2,
        LEFTWARD = 4,
        RIGHTWARD = 8,
        UPWARD = 16,
        WALK = 32,
        CLICK = 64,
        ALTCLICK = 128
    }
}
