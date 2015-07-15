using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using Voxalia.ServerGame.JointSystem;
using BEPUphysics.CollisionRuleManagement;
using BEPUutilities;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    class HookItem : BaseItemInfo
    {
        public HookItem()
        {
            Name = "hook";
        }

        public override void Click(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.LastClick + 0.2 > player.TheWorld.GlobalTickTime)
            {
                return;
            }
            Location eye = player.GetEyePosition();
            Location adj = player.ForwardVector() * 20f;
            CollisionResult cr = player.TheWorld.Collision.CuboidLineTrace(new Location(0.1f), eye, eye + adj, player.IgnoreThis);
            if (!cr.Hit)
            {
                return;
            }
            RemoveHook(player);
            if (cr.HitEnt == null)
            {
                return;
                // TODO: Hook static world somehow - perhaps spawn a temporary mass=0 entity?
            }
            PhysicsEntity pe = (PhysicsEntity)cr.HitEnt.Tag;
            float len = (float)(cr.Position - player.GetCenter()).Length();
            BaseJoint jd;
            jd = new JointDistance(player, pe, 0.01f, len + 0.1f, player.GetCenter(), cr.Position);
            player.TheWorld.AddJoint(jd);
            player.Hooks.Add(new HookInfo() { Joint = jd, Hit = pe, IsBar = false });
            Location step = (player.GetCenter() - cr.Position) / len;
            Location forw = Utilities.VectorToAngles(step);
            BEPUutilities.Quaternion quat = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)(forw.Pitch * Utilities.PI180)) *
                Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)(forw.Yaw * Utilities.PI180));
            PhysicsEntity cent = pe;
            for (float f = 0; f < len - 1f; f += 0.5f)
            {
                Location cpos = cr.Position + step * f;
                CubeEntity ce = new CubeEntity(new Location(0.23, 0.05, 0.05), player.TheWorld, 1);
                ce.SetPosition(cpos + step * 0.5);
                ce.SetOrientation(quat);
                player.TheWorld.SpawnEntity(ce);
                jd = new JointBallSocket(ce, cent, cpos);
                player.TheWorld.AddJoint(jd);
                player.Hooks.Add(new HookInfo() { Joint = jd, Hit = ce, IsBar = true });
                cent = ce;
            }
            //jd = new JointDistance(cent, player, 0.001f, 0.0011f, player.GetCenter(), player.GetCenter());
            jd = new JointBallSocket(cent, player, player.GetCenter());
            player.TheWorld.AddJoint(jd);
            player.Hooks.Add(new HookInfo() { Joint = jd, Hit = player, IsBar = false });
        }

        public override void AltClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.LastAltClick + 0.2 > player.TheWorld.GlobalTickTime)
            {
                return;
            }
            RemoveHook(player);
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
        }

        public override void ReleaseAltClick(Entity entity, ItemStack item)
        {
        }

        public static void RemoveHook(PlayerEntity player)
        {
            if (player.Hooks.Count > 0)
            {
                for (int i = 0; i < player.Hooks.Count; i++)
                {
                    player.TheWorld.DestroyJoint(player.Hooks[i].Joint);
                }
                for (int i = 0; i < player.Hooks.Count; i++)
                {
                    if (player.Hooks[i].IsBar)
                    {
                        player.TheWorld.DespawnEntity(player.Hooks[i].Hit);
                    }
                }
                player.Hooks.Clear();
            }
        }

        public override void PrepItem(Entity entity, ItemStack item)
        {
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
        }

        public override void SwitchTo(Entity entity, ItemStack item)
        {
        }

        public override void Use(Entity entity, ItemStack item)
        {
        }

        public override void Tick(Entity entity, ItemStack item)
        {
        }
    }

    public class HookInfo
    {
        public PhysicsEntity Hit = null;
        public BaseJoint Joint = null;
        public bool IsBar = false;
    }
}
