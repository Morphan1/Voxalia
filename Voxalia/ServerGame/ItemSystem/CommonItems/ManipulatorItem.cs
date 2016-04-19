using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    class ManipulatorItem: GenericItem
    {
        public ManipulatorItem()
        {
            Name = "manipulator";
        }

        public override void Click(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                return; // TODO: non-player support?
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.Manipulator_Grabbed != null)
            {
                return;
            }
            Location eye = player.GetEyePosition();
            CollisionResult cr = player.TheRegion.Collision.RayTrace(eye, eye + player.ForwardVector() * 50, player.IgnoreThis);
            if (!cr.Hit || cr.HitEnt == null || cr.HitEnt.Mass <= 0)
            {
                return;
            }
            PhysicsEntity target = (PhysicsEntity)cr.HitEnt.Tag;
            player.Manipulator_Grabbed = target;
            player.Manipulator_Distance = (float)(eye - target.GetPosition()).Length();
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                return; // TODO: non-player support?
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.Manipulator_Grabbed = null;
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                return; // TODO: non-player support?
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.Manipulator_Grabbed = null;
        }

        public override void Tick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                return; // TODO: non-player support?
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.Manipulator_Grabbed == null)
            {
                return;
            }
            if (!player.Manipulator_Grabbed.IsSpawned)
            {
                player.Manipulator_Grabbed = null;
                return;
            }
            if (player.ItemUp)
            {
                player.Manipulator_Distance = Math.Min(player.Manipulator_Distance + (float)(player.TheRegion.Delta * 2f), 50f);
            }
            if (player.ItemDown)
            {
                player.Manipulator_Distance = Math.Max(player.Manipulator_Distance - (float)(player.TheRegion.Delta * 2f), 2f);
            }
            Location goal = player.GetEyePosition() + player.ForwardVector() * player.Manipulator_Distance;
            player.Manipulator_Grabbed.SetVelocity((goal - player.Manipulator_Grabbed.GetPosition()) * 5f);
        }
    }
}
