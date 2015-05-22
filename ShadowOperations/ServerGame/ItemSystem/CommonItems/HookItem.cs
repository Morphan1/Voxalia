using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.JointSystem;

namespace ShadowOperations.ServerGame.ItemSystem.CommonItems
{
    class HookItem : BaseItemInfo
    {
        public HookItem()
        {
            Name = "hook";
        }

        public override void Click(PlayerEntity player, ItemStack item)
        {
            if (player.LastClick + 0.2 > player.TheServer.GlobalTickTime)
            {
                return;
            }
            Location eye = player.GetEyePosition();
            Location adj = player.ForwardVector() * 20f;
            CollisionResult cr = player.TheServer.Collision.CuboidLineTrace(new Location(0.1f), eye, eye + adj, player.IgnoreThis);
            if (!cr.Hit)
            {
                return;
            }
            RemoveHook(player);
            PhysicsEntity pe = (PhysicsEntity)cr.HitEnt.Tag;
            float len = (float)(cr.Position - player.GetCenter()).Length();
            JointDistance jd = new JointDistance(player, pe, len - 0.1f, len, player.GetCenter(), cr.Position);
            player.Hooks.Add(new HookInfo() { JD = jd, Hit = pe });
            player.TheServer.AddJoint(jd);
        }

        public override void AltClick(PlayerEntity player, ItemStack item)
        {
            if (player.LastAltClick + 0.2 > player.TheServer.GlobalTickTime)
            {
                return;
            }
            RemoveHook(player);
        }

        void RemoveHook(PlayerEntity player)
        {
            if (player.Hooks.Count > 0)
            {
                player.TheServer.DestroyJoint(player.Hooks[0].JD);
                player.Hooks.RemoveAt(0);
            }
        }

        public override void PrepItem(PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchFrom(PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchTo(PlayerEntity player, ItemStack item)
        {
        }

        public override void Use(PlayerEntity player, ItemStack item)
        {
        }
    }

    public class HookInfo
    {
        public PhysicsEntity Hit;
        public JointDistance JD;
    }
}
