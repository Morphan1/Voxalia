using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using BEPUutilities;
using Voxalia.ServerGame.JointSystem;

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
            player.Manipulator_Distance = (double)(eye - target.GetPosition()).Length();
            player.Manipulator_Beam = new ConnectorBeam() { type = BeamType.MULTICURVE };
            player.Manipulator_Beam.One = player;
            player.Manipulator_Beam.Two = target;
            player.Manipulator_Beam.color = Colors.BLUE;
            player.TheRegion.AddJoint(player.Manipulator_Beam);
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                return; // TODO: non-player support?
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.Manipulator_Grabbed = null;
            if (player.Manipulator_Beam != null)
            {
                player.TheRegion.DestroyJoint(player.Manipulator_Beam);
                player.Manipulator_Beam = null;
            }
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                return; // TODO: non-player support?
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.Manipulator_Grabbed = null;
            player.Flags &= ~YourStatusFlags.NO_ROTATE;
            player.AttemptedDirectionChange = Location.Zero;
            player.SendStatus();
            if (player.Manipulator_Beam != null)
            {
                player.TheRegion.DestroyJoint(player.Manipulator_Beam);
                player.Manipulator_Beam = null;
            }
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
                player.Manipulator_Distance = Math.Min(player.Manipulator_Distance + (double)(player.TheRegion.Delta * 2f), 50f);
            }
            if (player.ItemDown)
            {
                player.Manipulator_Distance = Math.Max(player.Manipulator_Distance - (double)(player.TheRegion.Delta * 2f), 2f);
            }
            Location goal = player.GetEyePosition() + player.ForwardVector() * player.Manipulator_Distance;
            player.Manipulator_Grabbed.SetVelocity((goal - player.Manipulator_Grabbed.GetPosition()) * 5f);
            if (player.Flags.HasFlag(YourStatusFlags.NO_ROTATE))
            {
                // TODO: Better method for easy rotation
                Quaternion quat = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (double)player.AttemptedDirectionChange.Pitch * 0.1f)
                    * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (double)player.AttemptedDirectionChange.Yaw * 0.1f);
                player.Manipulator_Grabbed.SetOrientation(player.Manipulator_Grabbed.GetOrientation() * quat);
                player.Manipulator_Grabbed.SetAngularVelocity(Location.Zero);
                player.AttemptedDirectionChange = Location.Zero;
            }
        }

        public override void AltClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                return; // TODO: non-player support?
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (!player.Flags.HasFlag(YourStatusFlags.NO_ROTATE))
            {
                player.Flags |= YourStatusFlags.NO_ROTATE;
                player.SendStatus();
                player.AttemptedDirectionChange = Location.Zero;
            }
        }

        public override void ReleaseAltClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                return; // TODO: non-player support?
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.Flags &= ~YourStatusFlags.NO_ROTATE;
            player.AttemptedDirectionChange = Location.Zero;
            player.SendStatus();
        }
    }
}
