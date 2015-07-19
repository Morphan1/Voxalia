using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUutilities;

namespace Voxalia.ServerGame.EntitySystem
{
    public class BlockItemEntity : PhysicsEntity, EntityUseable
    {
        public Material Mat;
        public byte Dat;

        public BlockItemEntity(World tworld, Material mat, byte dat, Location pos)
            : base(tworld, true)
        {
            SetMass(5);
            CGroup = CollisionUtil.Item;
            Dat = dat;
            if (dat == 0)
            {
                Shape = new BoxShape(1, 1, 1);
                SetPosition(pos.GetBlockLocation() + new Location(0.5));
            }
            else
            {
                List<Vector3> vecs = BlockShapeRegistry.BSD[Dat].GetVertices(new Vector3(0, 0, 0), false, false, false, false, false, false);
                int[] ints = new int[vecs.Count];
                for (int i = 0; i < vecs.Count; i++)
                {
                    ints[i] = i;
                }
                Vector3 offs;
                Shape = new MobileMeshShape(vecs.ToArray(), ints, new AffineTransform(new Vector3(0.95f, 0.95f, 0.95f),
                    Quaternion.Identity, Vector3.Zero), MobileMeshSolidity.DoubleSided, out offs);
                Location offset = Location.FromBVector(offs);
                SetPosition(pos.GetBlockLocation() + offset);
            }
            Mat = mat;
        }

        public bool pActive = false;

        public double deltat = 0;

        public override void Tick()
        {
            if (Body.ActivityInformation.IsActive || (pActive && !Body.ActivityInformation.IsActive))
            {
                pActive = Body.ActivityInformation.IsActive;
                TheWorld.SendToAll(new PhysicsEntityUpdatePacketOut(this));
            }
            if (!pActive && GetMass() > 0)
            {
                deltat += TheWorld.Delta;
                if (deltat > 2.0)
                {
                    TheWorld.SendToAll(new PhysicsEntityUpdatePacketOut(this));
                }
            }
            base.Tick();
        }

        // TODO: If settled (deactivated) for too long (minutes?), or loaded in via chunkload, revert to a block

        public bool Use(Entity user)
        {
            if (user is PlayerEntity)
            {
                ((PlayerEntity)user).Items.GiveItem(new ItemStack("block", Mat.ToString(), TheServer, 1, "", Mat.GetName(),
                    Mat.GetDescription(), Color.White.ToArgb(), "cube", false) { Datum = (ushort)Mat });
                TheWorld.DespawnEntity(this);
                return true;
            }
            return false;
        }
    }
}
