using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Entity pe1 = TheClient.GetEntity(EID1);
            Entity pe2 = TheClient.GetEntity(EID2);
            if (pe1 == null || pe2 == null)
            {
                SysConsole.Output(OutputType.WARNING, "Invalid EIDs " + EID1 + " and/or " + EID2);
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
                TheClient.AddJoint(jbs);
                return true;
            }
            else if (type == 1)
            {
                if (data.Length != len)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                JointSlider js = new JointSlider((PhysicsEntity)pe1, (PhysicsEntity)pe2);
                js.JID = JID;
                TheClient.AddJoint(js);
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
                TheClient.AddJoint(jd);
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
                TheClient.AddJoint(jpp);
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
                TheClient.AddJoint(jfw);
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
                JointSpinner jbs = new JointSpinner((PhysicsEntity)pe1, (PhysicsEntity)pe2, dir);
                jbs.JID = JID;
                TheClient.AddJoint(jbs);
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
