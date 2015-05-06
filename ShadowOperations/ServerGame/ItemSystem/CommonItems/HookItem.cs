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
            Location dir = player.GetAngles();
            Location adj = Utilities.ForwardVector_Deg(dir.X, dir.Y) * 20f;
            CollisionResult cr = player.TheServer.Collision.CuboidLineTrace(new Location(0.1f), eye, eye + adj, player.IgnoreThis);
            if (!cr.Hit)
            {
                return;
            }
            RemoveHook(player);
            PhysicsEntity pe = (PhysicsEntity)cr.HitEnt.Tag;
            CubeEntity ce = new CubeEntity(new Location(0.01f, 0.01f, 0.01f), player.TheServer, 1f);
            ce.Solid = false;
            ce.Visible = false;
            ce.SetPosition(cr.Position);
            CubeEntity ce2 = new CubeEntity(new Location(0.01f, 0.01f, 0.01f), player.TheServer, 1f);
            ce2.Solid = false;
            ce2.Visible = false;
            ce2.SetPosition(player.GetCenter());
            JointBallSocket jbs = new JointBallSocket(pe, ce, cr.Position);
            JointSlider js = new JointSlider(ce, ce2);
            float len = (float)(cr.Position - player.GetEyePosition()).Length();
            JointDistance jd = new JointDistance(ce, ce2, len - 0.01f, len);
            JointBallSocket jbs2 = new JointBallSocket(ce2, player, player.GetCenter());
            player.Hooks.Add(new HookInfo() { One = ce, Two = ce2, JD = jd, Hit = pe });
            player.TheServer.SpawnEntity(ce);
            player.TheServer.SpawnEntity(ce2);
            player.TheServer.AddJoint(jbs);
            player.TheServer.AddJoint(js);
            player.TheServer.AddJoint(jd);
            player.TheServer.AddJoint(jbs2);
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
                foreach (BaseJoint joint in new List<BaseJoint>(player.Hooks[0].One.Joints))
                {
                    player.TheServer.DestroyJoint(joint);
                }
                foreach (BaseJoint joint in new List<BaseJoint>(player.Hooks[0].Two.Joints))
                {
                    player.TheServer.DestroyJoint(joint);
                }
                player.TheServer.DespawnEntity(player.Hooks[0].One);
                player.TheServer.DespawnEntity(player.Hooks[0].Two);
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
        public CubeEntity One;
        public CubeEntity Two;
        public JointDistance JD;
    }
}
