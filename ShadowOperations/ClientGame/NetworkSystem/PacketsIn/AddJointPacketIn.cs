using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.JointSystem;
using ShadowOperations.ClientGame.EntitySystem;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    class AddJointPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            int len = 1 + 8 + 8 + 8;
            if (data.Length < len)
            {
                return false;
            }
            byte type = data[0];
            long EID1 = Utilities.BytesToLong(Utilities.BytesPartial(data, 1, 8));
            long EID2 = Utilities.BytesToLong(Utilities.BytesPartial(data, 1 + 8, 8));
            long JID = Utilities.BytesToLong(Utilities.BytesPartial(data, 1 + 8 + 8, 8));
            PhysicsEntity pe1 = (PhysicsEntity)TheClient.GetEntity(EID1);
            PhysicsEntity pe2 = (PhysicsEntity)TheClient.GetEntity(EID2);
            if (pe1 == null || pe2 == null)
            {
                SysConsole.Output(OutputType.WARNING, "Invalid EIDs " + EID1 + " and/or " + EID2);
                return false;
            }
            if (type == 0)
            {
                if (data.Length != len + 12)
                {
                    return false;
                }
                Location pos = Location.FromBytes(data, len);
                JointBallSocket jbs = new JointBallSocket(pe1, pe2, pos);
                jbs.JID = JID;
                TheClient.AddJoint(jbs);
                return true;
            }
            else if (type == 1)
            {
                if (data.Length != len)
                {
                    return false;
                }
                JointSlider js = new JointSlider(pe1, pe2);
                js.JID = JID;
                TheClient.AddJoint(js);
                return true;
            }
            else if (type == 2)
            {
                if (data.Length != len + 4 + 4 + 12 + 12)
                {
                    return false;
                }
                float min = Utilities.BytesToFloat(Utilities.BytesPartial(data, len, 4));
                float max = Utilities.BytesToFloat(Utilities.BytesPartial(data, len + 4, 4));
                Location ent1pos = Location.FromBytes(data, len + 4 + 4);
                Location ent2pos = Location.FromBytes(data, len + 4 + 4 + 12);
                JointDistance jd = new JointDistance(pe1, pe2, min, max, ent1pos, ent2pos);
                jd.JID = JID;
                TheClient.AddJoint(jd);
                return true;
            }
            else if (type == 3)
            {
                if (data.Length != len + 4)
                {
                    return false;
                }
                float stren = Utilities.BytesToFloat(Utilities.BytesPartial(data, len, 4));
                JointPullPush jpp = new JointPullPush(pe1, pe2, stren);
                jpp.JID = JID;
                TheClient.AddJoint(jpp);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
