using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using Voxalia.ServerGame.JointSystem;
using BEPUutilities;
using BEPUphysics.CollisionRuleManagement;
using Voxalia.Shared.Collision;

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
            if (player.LastClick + 0.2 > player.TheRegion.GlobalTickTime)
            {
                return;
            }
            Location eye = player.GetEyePosition();
            Location adj = player.ForwardVector() * 20f;
            CollisionResult cr = player.TheRegion.Collision.CuboidLineTrace(new Location(0.1f), eye, eye + adj, player.IgnoreThis);
            if (!cr.Hit)
            {
                return;
            }
            RemoveHook(player);
            PhysicsEntity pe;
            float len = (float)(cr.Position - player.GetCenter()).Length();
            Location step = (player.GetCenter() - cr.Position) / len;
            Location forw = Utilities.VectorToAngles(step);
            BEPUutilities.Quaternion quat = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)(forw.Pitch * Utilities.PI180)) *
                Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)(forw.Yaw * Utilities.PI180));
            if (cr.HitEnt == null)
            {
                ModelEntity mod = new ModelEntity("cube", player.TheRegion);
                mod.Mass = 0;
                mod.CanSave = false;
                mod.scale = new Location(0.023, 0.05, 0.05);
                mod.mode = ModelCollisionMode.AABB;
                mod.SetPosition(cr.Position);
                mod.SetOrientation(quat);
                player.TheRegion.SpawnEntity(mod);
                pe = mod;
            }
            else
            {
                pe = (PhysicsEntity)cr.HitEnt.Tag;
            }
            BaseJoint jd;
            jd = new JointDistance(player, pe, 0.01f, len + 0.1f, player.GetCenter(), cr.Position);
            player.TheRegion.AddJoint(jd);
            player.Hooks.Add(new HookInfo() { Joint = jd, Hit = pe, IsBar = false });
            PhysicsEntity cent = pe;
            for (float f = 0; f < len - 1f; f += 0.5f)
            {
                Location cpos = cr.Position + step * f;
                ModelEntity ce = new ModelEntity("cube", player.TheRegion);
                ce.Mass = 1;
                ce.CanSave = false;
                ce.scale = new Location(0.023, 0.05, 0.05);
                ce.mode = ModelCollisionMode.AABB;
                ce.SetPosition(cpos + step * 0.5);
                ce.SetOrientation(quat);
                player.TheRegion.SpawnEntity(ce);
                jd = new JointBallSocket(ce, cent, cpos + step * 0.5f);
                player.TheRegion.AddJoint(jd);
                player.Hooks.Add(new HookInfo() { Joint = jd, Hit = ce, IsBar = true });
                cent = ce;
            }
            jd = new JointBallSocket(cent, player, player.GetCenter());
            player.TheRegion.AddJoint(jd);
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
            if (player.LastAltClick + 0.2 > player.TheRegion.GlobalTickTime)
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
                    player.TheRegion.DestroyJoint(player.Hooks[i].Joint);
                }
                for (int i = 0; i < player.Hooks.Count; i++)
                {
                    if (player.Hooks[i].IsBar)
                    {
                        player.Hooks[i].Hit.RemoveMe();
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
