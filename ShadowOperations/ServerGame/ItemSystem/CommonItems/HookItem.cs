using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.JointSystem;
using BEPUphysics.CollisionRuleManagement;

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
            JointDistance jd;
            jd = new JointDistance(player, pe, 0.01f, len + 0.1f, player.GetCenter(), cr.Position);
            player.TheServer.AddJoint(jd);
            player.Hooks.Add(new HookInfo() { JD = jd, Hit = pe, IsBar = false });
            Location step = (player.GetCenter() - cr.Position) / len;
            PhysicsEntity cent = pe;
            for (float f = 0; f < len; f += 0.5f)
            {
                Location cpos = cr.Position + step * f;
                CubeEntity ce = new CubeEntity(new Location(0.05, 0.05, 0.23), player.TheServer, 1);
                ce.SetPosition(cpos + step * 0.5);
                player.TheServer.SpawnEntity(ce);
                jd = new JointDistance(ce, cent, 0.0001f, 0.00011f, ce.GetPosition() + new Location(0, 0, 0.15), cpos - new Location(0, 0, 0.15));
                player.TheServer.AddJoint(jd);
                player.Hooks.Add(new HookInfo() { JD = jd, Hit = ce, IsBar = true });
                cent = ce;
            }
            //cent.Body.CollisionInformation.CollisionRules.Specific.Add(player.Body.CollisionInformation.CollisionRules, CollisionRule.NoBroadPhase);
            //player.Body.CollisionInformation.CollisionRules.Specific.Add(cent.Body.CollisionInformation.CollisionRules, CollisionRule.NoBroadPhase);
            jd = new JointDistance(cent, player, 0.001f, 0.0011f, player.GetCenter(), player.GetCenter());
            player.TheServer.AddJoint(jd);
            player.Hooks.Add(new HookInfo() { JD = jd, Hit = player, IsBar = false });
        }

        public override void AltClick(PlayerEntity player, ItemStack item)
        {
            if (player.LastAltClick + 0.2 > player.TheServer.GlobalTickTime)
            {
                return;
            }
            RemoveHook(player);
        }

        public override void ReleaseClick(PlayerEntity player, ItemStack item)
        {
        }

        void RemoveHook(PlayerEntity player)
        {
            if (player.Hooks.Count > 0)
            {
                for (int i = 0; i < player.Hooks.Count; i++)
                {
                    player.TheServer.DestroyJoint(player.Hooks[i].JD);
                    if (player.Hooks[i].IsBar)
                    {
                        player.TheServer.DespawnEntity(player.Hooks[i].Hit);
                    }
                }
                player.Hooks.Clear();
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

        public override void Tick(PlayerEntity player, ItemStack item)
        {
        }
    }

    public class HookInfo
    {
        public PhysicsEntity Hit = null;
        public JointDistance JD = null;
        public bool IsBar = false;
    }
}
