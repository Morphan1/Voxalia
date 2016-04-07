using System;
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
            bool upw = val.HasFlag(KeysPacketData.UPWARD);
            bool downw = val.HasFlag(KeysPacketData.DOWNWARD);
            bool click = val.HasFlag(KeysPacketData.CLICK);
            bool aclick = val.HasFlag(KeysPacketData.ALTCLICK);
            bool use = val.HasFlag(KeysPacketData.USE);
            bool ileft = val.HasFlag(KeysPacketData.ITEMLEFT);
            bool iright = val.HasFlag(KeysPacketData.ITEMRIGHT);
            float yaw = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 2, 4));
            float pitch = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 2 + 4, 4));
            float x = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 2 + 4 + 4, 4));
            float y = Utilities.BytesToFloat(Utilities.BytesPartial(data, 8 + 2 + 4 + 4 + 4, 4));
            Vector2 tmove = new Vector2(x, y);
            if (tmove.LengthSquared() > 1f)
            {
                tmove.Normalize();
            }
            if (Player.Upward != upw || Player.Downward != downw || Player.Click != click || Player.AltClick != aclick
                || Player.Use != use || Math.Abs(Player.Direction.Yaw - yaw) > 0.05 || Math.Abs(Player.Direction.Pitch - pitch) > 0.05
                || Math.Abs(tmove.X - x) > 0.05 || Math.Abs(tmove.Y - y) > 0.05)
            {
                Player.NoteDidAction();
            }
            Player.Upward = upw;
            Player.Downward = downw;
            Player.Click = click;
            Player.AltClick = aclick;
            Player.Use = use;
            Player.ItemLeft = ileft;
            Player.ItemRight = iright;
            Player.LastKPI = Player.TheRegion.GlobalTickTime;
            Player.Direction.Yaw = yaw;
            Player.Direction.Pitch = pitch;
            Player.XMove = tmove.X;
            Player.YMove = tmove.Y;
            Player.Network.SendPacket(new YourPositionPacketOut(Player.TheRegion.GlobalTickTime, tid,
                Player.GetPosition(), Player.GetVelocity(), new Location(0, 0, 0), Player.CBody.StanceManager.CurrentStance, Player.pup));
            return true;
        }
    }

    public enum KeysPacketData : ushort // TODO: Network enum?
    {
        UPWARD = 1,
        CLICK = 2,
        ALTCLICK = 4,
        DOWNWARD = 8,
        USE = 16,
        ITEMLEFT = 32,
        ITEMRIGHT = 64
    }
}
