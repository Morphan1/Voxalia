using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class SpawnPhysicsEntityPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            int len = 4 + 12 + 12 + 16 + 12 + 8 + 4 + 12 + 1 + 4 + 1;
            if (data.Length < len)
            {
                return false;
            }
            byte type = data[4 + 12 + 12 + 16 + 12 + 8 + 4 + 12];
            float mass = Utilities.BytesToFloat(Utilities.BytesPartial(data, 0, 4));
            Location pos = Location.FromBytes(data, 4);
            Location vel = Location.FromBytes(data, 4 + 12);
            BEPUutilities.Quaternion ang = Utilities.BytesToQuaternion(data, 4 + 12 + 12);
            Location angvel = Location.FromBytes(data, 4 + 12 + 12 + 16);
            long eID = Utilities.BytesToLong(Utilities.BytesPartial(data, 4 + 12 + 12 + 16 + 12, 8));
            float fric = Utilities.BytesToFloat(Utilities.BytesPartial(data, 4 + 12 + 12 + 16 + 12 + 8, 4));
            Location halfsize = Location.FromBytes(data, 4 + 12 + 12 + 16 + 12 + 8 + 4);
            PhysicsEntity ce;
            int start = len - (4 + 1);
            if (type == 1)
            {
                ce = new OtherPlayerEntity(TheClient.TheRegion, halfsize);
            }
            else if (type == 2)
            {
                NetStringManager strings = TheClient.Network.Strings;
                ModelEntity me = new ModelEntity(strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start, 4))), TheClient.TheRegion);
                byte moder = data[start + 4];
                me.mode = (ModelCollisionMode)moder;
                ce = me;
            }
            else if (type == 3)
            {
                Material mat = (Material)Utilities.BytesToUshort(Utilities.BytesPartial(data, start, 2));
                byte dat = data[start + 2];
                BlockItemEntity bie = new BlockItemEntity(TheClient.TheRegion, mat, dat);
                ce = bie;
            }
            else if (type == 4)
            {
                int xwidth = (int)halfsize.X;
                int ywidth = (int)halfsize.Y;
                int zwidth = (int)halfsize.Z;
                BlockInternal[] bi = new BlockInternal[xwidth * ywidth * zwidth];
                for (int i = 0; i < bi.Length; i++)
                {
                    bi[i].BlockMaterial = Utilities.BytesToUshort(Utilities.BytesPartial(data, start + i * 2, 2));
                    bi[i].BlockData = data[start + bi.Length * 2 + i];
                }
                BGETraceMode tm = (BGETraceMode)data[start + bi.Length * 3];
                BlockGroupEntity bge = new BlockGroupEntity(TheClient.TheRegion, tm, bi, xwidth, ywidth, zwidth);
                ce = bge;
            }
            else if (type == 5)
            {
                int col = Utilities.BytesToInt(Utilities.BytesPartial(data, start, 4));
                ce = new GlowstickEntity(TheClient.TheRegion, col);
            }
            else if (type == 6)
            {
                ce = new GrenadeEntity(TheClient.TheRegion, true);
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "Unknown physent type " + type);
                return false;
            }
            float bounce = Utilities.BytesToFloat(Utilities.BytesPartial(data, data.Length - 5, 4));
            bool Visible = (data[data.Length - 1] & 1) == 1;
            int solidity = (data[data.Length - 1] & (2|4|8));
            if (solidity == 2)
            {
                ce.CGroup = CollisionUtil.Solid;
            }
            else if (solidity == 4)
            {
                ce.CGroup = CollisionUtil.NonSolid;
            }
            else if (solidity == (2 | 4))
            {
                ce.CGroup = CollisionUtil.Item;
            }
            else if (solidity == (8))
            {
                ce.CGroup = CollisionUtil.Player;
            }
            else if (solidity == (4 | 8))
            {
                ce.CGroup = CollisionUtil.Trigger;
            }
            else if (solidity == (2 | 8))
            {
                ce.CGroup = CollisionUtil.Solid; // PlaceHolder
            }
            else if (solidity == (2 | 4 | 8))
            {
                ce.CGroup = CollisionUtil.Solid; // PlaceHolder
            }
            ce.Visible = Visible;
            ce.SetMass(mass);
            ce.SetPosition(pos);
            ce.SetVelocity(vel);
            ce.SetOrientation(ang);
            ce.SetAngularVelocity(angvel);
            ce.EID = eID;
            ce.SetFriction(fric);
            ce.SetBounciness(bounce);
            TheClient.TheRegion.SpawnEntity(ce);
            return true;
        }
    }
}
