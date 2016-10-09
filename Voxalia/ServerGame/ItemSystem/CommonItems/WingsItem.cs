using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.JointSystem;
using Voxalia.Shared;
using BEPUutilities;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    class WingsItem : GenericItem
    {
        public WingsItem()
        {
            Name = "wings";
        }

        public  bool DontLandOnPlanes(BroadPhaseEntry bpe)
        {
            if (bpe is EntityCollidable)
            {
                EntityCollidable ec = bpe as EntityCollidable;
                if (ec.Entity != null && (ec.Entity.Tag is PlayerEntity || ec.Entity.Tag is PlaneEntity))
                {
                    return false;
                }
            }
            return true;
        }

        public override void Tick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = entity as PlayerEntity;
            RayCastResult rh;
            if (player.Wings != null && player.Wings.DriverSeat.Sitter != player)
            {
                CloseWings(player);
            }
            if (player.TheRegion.SpecialCaseRayTrace(player.GetPosition(), -Location.UnitZ, 1, MaterialSolidity.FULLSOLID, DontLandOnPlanes, out rh))
            {
                CloseWings(player);
            }
            else if (player.Upward && player.Wings == null && !player.IsFlying)
            {
                OpenWings(player);
            }
        }

        public void OpenWings(PlayerEntity player)
        {
            PlaneEntity plane = new PlaneEntity("planeifound", player.TheRegion); // TODO: Player-wings model!
            plane.SetMass(100);
            plane.SetPosition(player.GetPosition());
            player.TheRegion.SpawnEntity(plane);
            plane.SetOrientation(Quaternion.CreateFromAxisAngle(Vector3.UnitX, -90.0 * Utilities.PI180));
            plane.DriverSeat.Accept(player);
            player.Wings = plane;
            // JUST IN CASE: Enforce the correct default orientation!
            // TODO: Make this not needed!
            player.TheRegion.TheWorld.Schedule.ScheduleSyncTask(() =>
            {
                plane.SetOrientation(Quaternion.CreateFromAxisAngle(Vector3.UnitX, -90.0 * Utilities.PI180));
            }, 0.05);
        }

        public void CloseWings(PlayerEntity player)
        {
            if (player.Wings != null)
            {
                player.TheRegion.DespawnEntity(player.Wings);
                player.Wings = null;
            }
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = entity as PlayerEntity;
            CloseWings(player);
        }
    }
}
