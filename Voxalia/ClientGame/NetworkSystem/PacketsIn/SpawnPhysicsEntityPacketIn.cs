using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared.Collision;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.ClientGame.WorldSystem;

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
            Entity e = TheClient.TheRegion.GetEntity(eID);
            if (e != null)
            {
                TheClient.TheRegion.Despawn(e);
            }
            if (type == 2)
            {
                NetStringManager strings = TheClient.Network.Strings;
                ModelEntity me = new ModelEntity(strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(data, start, 4))), TheClient.TheRegion);
                byte moder = data[start + 4];
                me.mode = (ModelCollisionMode)moder;
                me.scale = halfsize;
                ce = me;
            }
            else if (type == 3)
            {
                Material mat = (Material)Utilities.BytesToUshort(Utilities.BytesPartial(data, start, 2));
                byte dat = data[start + 2];
                byte tpa = data[start + 3];
                byte damage = data[start + 4];
                BlockItemEntity bie = new BlockItemEntity(TheClient.TheRegion, mat, dat, tpa, (BlockDamage)damage);
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
                    bi[i]._BlockMaterialInternal = Utilities.BytesToUshort(Utilities.BytesPartial(data, start + i * 2, 2));
                    bi[i].BlockData = data[start + bi.Length * 2 + i];
                    bi[i].BlockPaint = data[start + bi.Length * 3 + i];
                }
                BGETraceMode tm = (BGETraceMode)data[start + bi.Length * 4];
                BlockGroupEntity bge = new BlockGroupEntity(TheClient.TheRegion, tm, bi, xwidth, ywidth, zwidth, Location.FromBytes(data, start + bi.Length * 4 + 1 + 4));
                bge.Color = System.Drawing.Color.FromArgb(Utilities.BytesToInt(Utilities.BytesPartial(data, start + bi.Length * 4 + 1, 4)));
                bge.scale = Location.FromBytes(data, start + bi.Length * 4 + 1 + 4 + 12);
                SysConsole.Output(OutputType.INFO, "Scaled to " + bge.scale);
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
            else if (type == 7)
            {
                int itsbyte = Utilities.BytesToInt(Utilities.BytesPartial(data, start, 4));
                BlockInternal bi = BlockInternal.FromItemDatum(itsbyte);
                ce = new StaticBlockEntity(TheClient.TheRegion, bi.Material, bi.BlockPaint);
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "Unknown physent type " + type);
                return false;
            }
            float bounce = Utilities.BytesToFloat(Utilities.BytesPartial(data, data.Length - (4 + 1 + 1), 4));
            byte flags = data[data.Length - 2];
            bool Visible = (flags & 1) == 1;
            bool genShadow = (flags & 2) == 2;
            int solidity = (data[data.Length - 1] & (2 | 4 | 8 | 16));
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
            else if (solidity == 8)
            {
                ce.CGroup = CollisionUtil.Player;
            }
            else if (solidity == (2 | 8))
            {
                ce.CGroup = CollisionUtil.Water;
            }
            else if (solidity == (2 | 4 | 8))
            {
                ce.CGroup = CollisionUtil.WorldSolid;
            }
            else if (solidity == 16)
            {
                ce.CGroup = CollisionUtil.Character;
            }
            ce.Visible = Visible;
            ce.GenBlockShadows = genShadow;
            ce.SetMass(mass);
            ce.SetPosition(pos);
            ce.SetVelocity(vel);
            ce.SetOrientation(ang);
            ce.SetAngularVelocity(angvel);
            ce.EID = eID;
            ce.SetFriction(fric);
            ce.SetBounciness(bounce);
            TheClient.TheRegion.SpawnEntity(ce);
            if (ce.GenBlockShadows)
            {
                Chunk ch = TheClient.TheRegion.GetChunk(TheClient.TheRegion.ChunkLocFor(pos));
                if (ch != null)
                {
                    ch.CreateVBO();
                }
            }
            return true;
        }
    }
}
