using Voxalia.Shared;
using Voxalia.ClientGame.JointSystem;
using Voxalia.ClientGame.EntitySystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    class AddJointPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            int len = 1 + 8 + 8 + 8;
            if (data.Length < len)
            {
                SysConsole.Output(OutputType.WARNING, "Joint packet: Bad initial length!");
                return false;
            }
            byte type = data[0];
            long EID1 = Utilities.BytesToLong(Utilities.BytesPartial(data, 1, 8));
            long EID2 = Utilities.BytesToLong(Utilities.BytesPartial(data, 1 + 8, 8));
            long JID = Utilities.BytesToLong(Utilities.BytesPartial(data, 1 + 8 + 8, 8));
            Entity pe1 = TheClient.TheRegion.GetEntity(EID1);
            Entity pe2 = TheClient.TheRegion.GetEntity(EID2);
            if (pe1 == null || pe2 == null)
            {
                SysConsole.Output(OutputType.WARNING, "Invalid EID(s) " + EID1 + " and/or " + EID2);
                return false;
            }
            if (type == 0)
            {
                if (data.Length != len + 12)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location pos = Location.FromBytes(data, len);
                JointBallSocket jbs = new JointBallSocket((PhysicsEntity)pe1, (PhysicsEntity)pe2, pos);
                jbs.JID = JID;
                TheClient.TheRegion.AddJoint(jbs);
                return true;
            }
            else if (type == 1)
            {
                if (data.Length != len + 12)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location dir = Location.FromBytes(data, len);
                JointSlider js = new JointSlider((PhysicsEntity)pe1, (PhysicsEntity)pe2, dir);
                js.JID = JID;
                TheClient.TheRegion.AddJoint(js);
                return true;
            }
            else if (type == 2)
            {
                if (data.Length != len + 4 + 4 + 12 + 12)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                float min = Utilities.BytesToFloat(Utilities.BytesPartial(data, len, 4));
                float max = Utilities.BytesToFloat(Utilities.BytesPartial(data, len + 4, 4));
                Location ent1pos = Location.FromBytes(data, len + 4 + 4);
                Location ent2pos = Location.FromBytes(data, len + 4 + 4 + 12);
                JointDistance jd = new JointDistance((PhysicsEntity)pe1, (PhysicsEntity)pe2, min, max, ent1pos, ent2pos);
                jd.JID = JID;
                TheClient.TheRegion.AddJoint(jd);
                return true;
            }
            else if (type == 3)
            {
                if (data.Length != len + 4)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                float stren = Utilities.BytesToFloat(Utilities.BytesPartial(data, len, 4));
                JointPullPush jpp = new JointPullPush((PhysicsEntity)pe1, (PhysicsEntity)pe2, stren);
                jpp.JID = JID;
                TheClient.TheRegion.AddJoint(jpp);
                return true;
            }
            else if (type == 4)
            {
                if (data.Length != len)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                JointForceWeld jfw = new JointForceWeld(pe1, pe2);
                jfw.JID = JID;
                TheClient.TheRegion.AddJoint(jfw);
                return true;
            }
            else if (type == 5)
            {
                if (data.Length != len + 12)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location dir = Location.FromBytes(data, len);
                JointSpinner js = new JointSpinner((PhysicsEntity)pe1, (PhysicsEntity)pe2, dir);
                js.JID = JID;
                TheClient.TheRegion.AddJoint(js);
                return true;
            }
            else if (type == 6)
            {
                if (data.Length != len + 12 + 12)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location a1 = Location.FromBytes(data, len);
                Location a2 = Location.FromBytes(data, len + 12);
                JointTwist jt = new JointTwist((PhysicsEntity)pe1, (PhysicsEntity)pe2, a1, a2);
                jt.JID = JID;
                TheClient.TheRegion.AddJoint(jt);
                return true;
            }
            else if (type == 7)
            {
                if (data.Length != len)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                JointWeld jw = new JointWeld((PhysicsEntity)pe1, (PhysicsEntity)pe2);
                jw.JID = JID;
                TheClient.TheRegion.AddJoint(jw);
                return true;
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "Unknown joint type " + type);
                return false;
            }
        }
    }
}
