using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUutilities;

namespace Voxalia.ServerGame.NetworkSystem.PacketsIn
{
    class KeysPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8 + 2 + 4 + 4 + 4 + 4)
            {
                return false;
            }
            long tid = Utilities.BytesToLong(Utilities.BytesPartial(data, 0, 8));
            KeysPacketData val = (KeysPacketData)Utilities.BytesToUshort(Utilities.BytesPartial(data, 8, 2));
            Player.Upward = val.HasFlag(KeysPacketData.UPWARD);
            Player.Downward = val.HasFlag(KeysPacketData.DOWNWARD);
            Player.Click = val.HasFlag(KeysPacketData.CLICK);
            Player.AltClick = val.HasFlag(KeysPacketData.ALTCLICK);
            Player.Use = val.HasFlag(KeysPacketData.USE);
            Player.Network.SendPacket(new YourPositionPacketOut(Player.TheRegion.GlobalTickTime, tid,
                Player.GetPosition(), Player.GetVelocity(), new Location(0, 0, 0), Player.CBody.StanceManager.CurrentStance, Player.pup));
            Player.LastKPI = Player.TheRegion.GlobalTickTime;
            Player.Direction.Yaw = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 2, 4));
            Player.Direction.Pitch = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 2 + 4, 4));
            float x = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 2 + 4 + 4, 4));
            float y = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 2 + 4 + 4 + 4, 4));
            Vector2 tmove = new Vector2(x, y);
            if (tmove.LengthSquared() > 1f)
            {
                tmove.Normalize();
            }
            Player.XMove = tmove.X;
            Player.YMove = tmove.Y;
            return true;
        }
    }

    public enum KeysPacketData : ushort // TODO: Network enum?
    {
        UPWARD = 1,
        CLICK = 2,
        ALTCLICK = 4,
        DOWNWARD = 8,
        USE = 16
    }
}
