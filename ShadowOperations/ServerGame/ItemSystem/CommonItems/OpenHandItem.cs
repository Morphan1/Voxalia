using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.ServerGame.JointSystem;

namespace ShadowOperations.ServerGame.ItemSystem.CommonItems
{
    class OpenHandItem: BaseItemInfo
    {
        public OpenHandItem()
            : base()
        {
            Name = "open_hand";
        }

        public override void PrepItem(EntitySystem.PlayerEntity player, ItemStack item)
        {
        }

        public override void AltClick(EntitySystem.PlayerEntity player, ItemStack item)
        {
            if (player.GrabJoint != null)
            {
                player.TheServer.DestroyJoint(player.GrabJoint);
            }
            player.GrabJoint = null;
        }

        public override void Click(EntitySystem.PlayerEntity player, ItemStack item)
        {
            Location end = player.GetEyePosition() + player.ForwardVector() * 2;
            CollisionResult cr = player.TheServer.Collision.CuboidLineTrace(new Location(0.1, 0.1, 0.1), player.GetEyePosition(), end, player.IgnoreThis);
            if (cr.Hit)
            {
                PhysicsEntity pe = (PhysicsEntity)cr.HitEnt.Tag;
                if (pe.GetMass() > 0 && pe.GetMass() < 200)
                {
                    Grab(player, pe, cr.Position);
                }
                else
                {
                    // If (HandHold) { Grab(player, pe, cr.Position); }
                }
            }
        }

        public void Grab(PlayerEntity player, PhysicsEntity pe, Location hit)
        {
            AltClick(player, null);
            JointBallSocket jbs = new JointBallSocket(player, pe, hit);
            player.TheServer.AddJoint(jbs);
            player.GrabJoint = jbs;

        }

        public override void ReleaseClick(PlayerEntity player, ItemStack item)
        {
        }

        public override void Use(EntitySystem.PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchFrom(PlayerEntity player, ItemStack item)
        {
            AltClick(player, item);
        }

        public override void SwitchTo(PlayerEntity player, ItemStack item)
        {
        }

        public override void Tick(PlayerEntity player, ItemStack item)
        {
        }
    }
}
