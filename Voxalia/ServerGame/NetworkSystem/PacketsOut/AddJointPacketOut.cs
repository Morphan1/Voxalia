using Voxalia.ServerGame.JointSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
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
                Data = new byte[len + 12];
                Data[0] = 1;
                ((JointSlider)joint).Direction.ToBytes().CopyTo(Data, len);

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
                Data = new byte[len + 12 + 1];
                Data[0] = 3;
                ((JointPullPush)joint).Axis.ToBytes().CopyTo(Data, len);
                Data[len + 12] = (byte)(((JointPullPush)joint).Mode ? 1 : 0);
            }
            else if (joint is JointForceWeld)
            {
                Data = new byte[len];
                Data[0] = 4;
            }
            else if (joint is JointSpinner)
            {
                Data = new byte[len + 12];
                Data[0] = 5;
                ((JointSpinner)joint).Direction.ToBytes().CopyTo(Data, len);
            }
            else if (joint is JointTwist)
            {
                Data = new byte[len + 12 + 12];
                Data[0] = 6;
                ((JointTwist)joint).AxisOne.ToBytes().CopyTo(Data, len);
                ((JointTwist)joint).AxisTwo.ToBytes().CopyTo(Data, len + 12);
            }
            else if (joint is JointWeld)
            {
                Data = new byte[len];
                Data[0] = 7;
            }
            else if (joint is JointVehicleMotor)
            {
                Data = new byte[len + 12 + 1];
                Data[0] = 8;
                ((JointVehicleMotor)joint).Direction.ToBytes().CopyTo(Data, len);
                Data[len + 12] = (byte)(((JointVehicleMotor)joint).IsSteering ? 1 : 0);
            }
            else if (joint is JointLAxisLimit)
            {
                Data = new byte[len + 12 + 12 + 12 + 4 + 4];
                Data[0] = 9;
                ((JointLAxisLimit)joint).CPos1.ToBytes().CopyTo(Data, len);
                ((JointLAxisLimit)joint).CPos2.ToBytes().CopyTo(Data, len + 12);
                ((JointLAxisLimit)joint).Axis.ToBytes().CopyTo(Data, len + 12 + 12);
                Utilities.FloatToBytes(((JointLAxisLimit)joint).Min).CopyTo(Data, len + 12 + 12 + 12);
                Utilities.FloatToBytes(((JointLAxisLimit)joint).Max).CopyTo(Data, len + 12 + 12 + 12 + 4);
            }
            else if (joint is JointSwivelHinge)
            {
                Data = new byte[len + 12 + 12];
                Data[0] = 10;
                ((JointSwivelHinge)joint).WorldHinge.ToBytes().CopyTo(Data, len);
                ((JointSwivelHinge)joint).WorldTwist.ToBytes().CopyTo(Data, len + 12);
            }
            Utilities.LongToBytes(joint.One.EID).CopyTo(Data, 1);
            Utilities.LongToBytes(joint.Two.EID).CopyTo(Data, 1 + 8);
            Utilities.LongToBytes(joint.JID).CopyTo(Data, 1 + 8 + 8);
        }
    }
}
