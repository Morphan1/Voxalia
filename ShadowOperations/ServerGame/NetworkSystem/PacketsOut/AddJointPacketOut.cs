using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.JointSystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    public class AddJointPacketOut : AbstractPacketOut
    {
        public AddJointPacketOut(InternalBaseJoint joint)
        {
            ID = 12;
            int len = 1 + 8 + 8 + 8;
            if (joint is JointBallSocket)
            {
                Data = new byte[len + 12];
                Data[0] = 0;
                ((JointBallSocket)joint).Position.ToBytes().CopyTo(Data, len);
            }
            else if (joint is JointSlider)
            {
                Data = new byte[len];
                Data[0] = 1;
            }
            else if (joint is JointDistance)
            {
                Data = new byte[len + 4 + 4 + 12 + 12];
                Data[0] = 2;
                Utilities.FloatToBytes(((JointDistance)joint).Min).CopyTo(Data, len);
                Utilities.FloatToBytes(((JointDistance)joint).Max).CopyTo(Data, len + 4);
                ((JointDistance)joint).Ent1Pos.ToBytes().CopyTo(Data, len + 4 + 4);
                ((JointDistance)joint).Ent2Pos.ToBytes().CopyTo(Data, len + 4 + 4 + 12);
            }
            else if (joint is JointPullPush)
            {
                Data = new byte[len + 4];
                Data[0] = 3;
                Utilities.FloatToBytes(((JointPullPush)joint).Strength).CopyTo(Data, len);
            }
            else if (joint is JointForceWeld)
            {
                Data = new byte[len];
                Data[0] = 4;
            }
            Utilities.LongToBytes(joint.One.EID).CopyTo(Data, 1);
            Utilities.LongToBytes(joint.Two.EID).CopyTo(Data, 1 + 8);
            Utilities.LongToBytes(joint.JID).CopyTo(Data, 1 + 8 + 8);
        }
    }
}
