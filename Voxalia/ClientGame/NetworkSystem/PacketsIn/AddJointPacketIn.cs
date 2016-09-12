using Voxalia.Shared;
using Voxalia.ClientGame.JointSystem;
using Voxalia.ClientGame.EntitySystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    class AddJointPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            int len = 1 + 8 + 8 + 8 + 24 + 4 + 4 + 4 + 4 + 24 + 4 + 4 + 4 + 4;
            if (data.Length < len)
            {
                SysConsole.Output(OutputType.WARNING, "Joint packet: Bad initial length!");
                return false;
            }
            byte type = data[0];
            long EID1 = Utilities.BytesToLong(Utilities.BytesPartial(data, 1, 8));
            long EID2 = Utilities.BytesToLong(Utilities.BytesPartial(data, 1 + 8, 8));
            long JID = Utilities.BytesToLong(Utilities.BytesPartial(data, 1 + 8 + 8, 8));
            if (TheClient.TheRegion.GetJoint(JID) != null)
            {
                // If we already know this joint, just keep on going.
                return true;
            }
            Entity pe1 = TheClient.TheRegion.GetEntity(EID1);
            Entity pe2 = TheClient.TheRegion.GetEntity(EID2);
            if (pe1 == null)
            {
                SysConsole.Output(OutputType.WARNING, "Joint Packet: Invalid EID-1 " + EID1);
                return false;
            }
            if (pe2 == null)
            {
                SysConsole.Output(OutputType.WARNING, "Joint Packet: Invalid EID-2 " + EID2);
                return false;
            }
            Location pos1 = Location.FromDoubleBytes(data, 1 + 8 + 8 + 8);
            float qx1 = Utilities.BytesToFloat(Utilities.BytesPartial(data, 1 + 8 + 8 + 8 + 24, 4));
            float qy1 = Utilities.BytesToFloat(Utilities.BytesPartial(data, 1 + 8 + 8 + 8 + 24 + 4, 4));
            float qz1 = Utilities.BytesToFloat(Utilities.BytesPartial(data, 1 + 8 + 8 + 8 + 24 + 4 + 4, 4));
            float qw1 = Utilities.BytesToFloat(Utilities.BytesPartial(data, 1 + 8 + 8 + 8 + 24 + 4 + 4 + 4, 4));
            BEPUutilities.Quaternion quat1 = new BEPUutilities.Quaternion(qx1, qy1, qz1, qw1);
            pe1.SetPosition(pos1);
            pe1.SetOrientation(quat1);
            Location pos2 = Location.FromDoubleBytes(data, 1 + 8 + 8 + 8 + 24 + 4 + 4 + 4 + 4);
            float qx2 = Utilities.BytesToFloat(Utilities.BytesPartial(data, 1 + 8 + 8 + 8 + 24 + 24 + 4 + 4 + 4 + 4, 4));
            float qy2 = Utilities.BytesToFloat(Utilities.BytesPartial(data, 1 + 8 + 8 + 8 + 24 + 4 + 24 + 4 + 4 + 4 + 4, 4));
            float qz2 = Utilities.BytesToFloat(Utilities.BytesPartial(data, 1 + 8 + 8 + 8 + 24 + 4 + 4 + 24 + 4 + 4 + 4 + 4, 4));
            float qw2 = Utilities.BytesToFloat(Utilities.BytesPartial(data, 1 + 8 + 8 + 8 + 24 + 4 + 4 + 4 + 24 + 4 + 4 + 4 + 4, 4));
            BEPUutilities.Quaternion quat2 = new BEPUutilities.Quaternion(qx2, qy2, qz2, qw2);
            pe1.SetPosition(pos1);
            pe1.SetOrientation(quat1);
            pe2.SetPosition(pos2);
            pe2.SetOrientation(quat2);
            if (type == 0)
            {
                if (data.Length != len + 24)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location pos = Location.FromDoubleBytes(data, len);
                JointBallSocket jbs = new JointBallSocket((PhysicsEntity)pe1, (PhysicsEntity)pe2, pos);
                jbs.JID = JID;
                TheClient.TheRegion.AddJoint(jbs);
                return true;
            }
            else if (type == 1)
            {
                if (data.Length != len + 24)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location dir = Location.FromDoubleBytes(data, len);
                JointSlider js = new JointSlider((PhysicsEntity)pe1, (PhysicsEntity)pe2, dir);
                js.JID = JID;
                TheClient.TheRegion.AddJoint(js);
                return true;
            }
            else if (type == 2)
            {
                if (data.Length != len + 4 + 4 + 24 + 24)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                float min = Utilities.BytesToFloat(Utilities.BytesPartial(data, len, 4));
                float max = Utilities.BytesToFloat(Utilities.BytesPartial(data, len + 4, 4));
                Location ent1pos = Location.FromDoubleBytes(data, len + 4 + 4);
                Location ent2pos = Location.FromDoubleBytes(data, len + 4 + 4 + 24);
                JointDistance jd = new JointDistance((PhysicsEntity)pe1, (PhysicsEntity)pe2, min, max, ent1pos, ent2pos);
                jd.JID = JID;
                TheClient.TheRegion.AddJoint(jd);
                return true;
            }
            else if (type == 3)
            {
                if (data.Length != len + 24 + 1)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location axis = Location.FromDoubleBytes(data, len);
                bool mode = data[len + 24] == 1;
                JointPullPush jpp = new JointPullPush((PhysicsEntity)pe1, (PhysicsEntity)pe2, axis, mode);
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
                if (data.Length != len + 24)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location dir = Location.FromDoubleBytes(data, len);
                JointSpinner js = new JointSpinner((PhysicsEntity)pe1, (PhysicsEntity)pe2, dir);
                js.JID = JID;
                TheClient.TheRegion.AddJoint(js);
                return true;
            }
            else if (type == 6)
            {
                if (data.Length != len + 24 + 24)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location a1 = Location.FromDoubleBytes(data, len);
                Location a2 = Location.FromDoubleBytes(data, len + 12);
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
            else if (type == 8)
            {
                if (data.Length != len + 24 + 1)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location dir = Location.FromDoubleBytes(data, len);
                bool issteering = data[len + 24] == 1;
                JointVehicleMotor jm = new JointVehicleMotor((PhysicsEntity)pe1, (PhysicsEntity)pe2, dir, issteering);
                jm.JID = JID;
                TheClient.TheRegion.AddJoint(jm);
                return true;
            }
            else if (type == 9)
            {
                if (data.Length != len + 24 + 24 + 24 + 4 + 4)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location cpos1 = Location.FromDoubleBytes(data, len);
                Location cpos2 = Location.FromDoubleBytes(data, len + 24);
                Location axis = Location.FromDoubleBytes(data, len + 24 + 24);
                float min = Utilities.BytesToFloat(Utilities.BytesPartial(data, len + 24 + 24 + 24, 4));
                float max = Utilities.BytesToFloat(Utilities.BytesPartial(data, len + 24 + 24 + 24 + 4, 4));
                JointLAxisLimit jlal = new JointLAxisLimit((PhysicsEntity)pe1, (PhysicsEntity)pe2, min, max, cpos1, cpos2, axis);
                jlal.JID = JID;
                TheClient.TheRegion.AddJoint(jlal);
                return true;
            }
            else if (type == 10)
            {
                if (data.Length != len + 24 + 24)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                Location hinge = Location.FromDoubleBytes(data, len);
                Location twist = Location.FromDoubleBytes(data, len + 24);
                JointSwivelHinge jlal = new JointSwivelHinge((PhysicsEntity)pe1, (PhysicsEntity)pe2, hinge, twist);
                jlal.JID = JID;
                TheClient.TheRegion.AddJoint(jlal);
                return true;
            }
            else if (type == 11)
            {
                if (data.Length != len + 4)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                float height = Utilities.BytesToFloat(Utilities.BytesPartial(data, len, 4));
                ConstWheelStepUp cwsu = new ConstWheelStepUp((PhysicsEntity)pe1, height);
                cwsu.JID = JID;
                TheClient.TheRegion.AddJoint(cwsu);
                return true;
            }
            else if (type == 12)
            {
                if (data.Length != len + 4 + 1)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                ConnectorBeam cb = new ConnectorBeam();
                cb.One = pe1;
                cb.Two = pe2;
                cb.color = System.Drawing.Color.FromArgb(Utilities.BytesToInt(Utilities.BytesPartial(data, len, 4)));
                cb.JID = JID;
                cb.type = (BeamType)data[len + 4];
                TheClient.TheRegion.AddJoint(cb);
                return true;
            }
            else if (type == 13)
            {
                if (data.Length != len)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                JointFlyingDisc jfd = new JointFlyingDisc((PhysicsEntity)pe1);
                jfd.JID = JID;
                TheClient.TheRegion.AddJoint(jfd);
                return true;
            }
            else if (type == 14)
            {
                if (data.Length != len)
                {
                    SysConsole.Output(OutputType.WARNING, "Joint packet: Bad length!");
                    return false;
                }
                JointNoCollide jnc = new JointNoCollide((PhysicsEntity)pe1, (PhysicsEntity)pe2);
                jnc.JID = JID;
                TheClient.TheRegion.AddJoint(jnc);
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
