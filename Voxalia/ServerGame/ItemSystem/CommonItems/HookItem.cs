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
            ApplyHook(player, item, cr.Position, cr.HitEnt);
        }

        public void ApplyHook(PlayerEntity player, ItemStack item, Location Position, BEPUphysics.Entities.Entity HitEnt)
        {
            RemoveHook(player);
            PhysicsEntity pe;
            float len = (float)(Position - player.GetCenter()).Length();
            Location step = (player.GetCenter() - Position) / len;
            Location forw = Utilities.VectorToAngles(step);
            BEPUutilities.Quaternion quat = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)(forw.Pitch * Utilities.PI180)) *
                Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)(forw.Yaw * Utilities.PI180));
            if (HitEnt == null)
            {
                ModelEntity mod = new ModelEntity("cube", player.TheRegion);
                mod.Mass = 0;
                mod.CanSave = false;
                mod.scale = new Location(0.023, 0.05, 0.05);
                mod.mode = ModelCollisionMode.AABB;
                mod.SetPosition(Position);
                mod.SetOrientation(quat);
                player.TheRegion.SpawnEntity(mod);
                pe = mod;
                player.Hooks.Add(new HookInfo() { Joint = null, Hit = pe, IsBar = true });
            }
            else
            {
                pe = (PhysicsEntity)HitEnt.Tag;
            }
            JointDistance jd;
            jd = new JointDistance(player, pe, 0.01f, len + 0.01f, player.GetCenter(), Position);
            player.TheRegion.AddJoint(jd);
            player.Hooks.Add(new HookInfo() { Joint = jd, Hit = pe, IsBar = false });



            PhysicsEntity cent = pe;
            for (float f = 0; f < len - 1f; f += 0.5f)
            {
                Location cpos = Position + step * f;
                ModelEntity ce = new ModelEntity("cube", player.TheRegion);
                ce.Mass = 1;
                ce.CanSave = false;
                ce.scale = new Location(0.023, 0.05, 0.05);
                ce.mode = ModelCollisionMode.AABB;
                ce.SetPosition(cpos + step * 0.5);
                ce.SetOrientation(quat);
                player.TheRegion.SpawnEntity(ce);
                jd = new JointDistance(ce, cent, 0.01f, 0.5f, ce.GetPosition(), (ReferenceEquals(cent, pe) ? Position: cent.GetPosition()));
                CollisionRules.AddRule(player.Body, ce.Body, CollisionRule.NoBroadPhase);
                player.TheRegion.AddJoint(jd);
                player.Hooks.Add(new HookInfo() { Joint = jd, Hit = ce, IsBar = true });
                cent = ce;
            }
            jd = new JointDistance(cent, player, 0.01f, 0.5f, cent.GetPosition(), player.GetCenter());
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
                    if (player.Hooks[i].Joint != null)
                    {
                        player.TheRegion.DestroyJoint(player.Hooks[i].Joint);
                    }
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

        public void Shrink(PlayerEntity player)
        {
            // TODO: Even out the magic player joint.
            for (int i = 0; i < player.Hooks.Count; i++)
            {
                if (player.Hooks[i].Joint != null)
                {
                    if (player.Hooks[i].Joint.One == player || player.Hooks[i].Joint.Max > 0.3f)
                    {
                        player.Hooks[i].Joint.Max *= 0.9f;
                        // TODO: less lazy networking of this
                        player.TheRegion.DestroyJoint(player.Hooks[i].Joint);
                        player.TheRegion.AddJoint(player.Hooks[i].Joint);
                    }
                }
            }
        }

        public void Extend(PlayerEntity player)
        {
            // TODO: Even out the magic player joint.
            for (int i = 0; i < player.Hooks.Count; i++)
            {
                if (player.Hooks[i].Joint != null)
                {
                    if (player.Hooks[i].Joint.One == player || player.Hooks[i].Joint.Max < 3f)
                    {
                        player.Hooks[i].Joint.Max *= 1.1f;
                        // TODO: less lazy networking of this
                        player.TheRegion.DestroyJoint(player.Hooks[i].Joint);
                        player.TheRegion.AddJoint(player.Hooks[i].Joint);
                    }
                }
            }
        }

        public override void Tick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            if (player.ItemLeft && !player.WasItemLefting && player.Hooks.Count > 0)
            {
                Extend(player);
            }
            if (player.ItemRight && !player.WasItemRighting && player.Hooks.Count > 0)
            {
                Shrink(player);
            }
        }
    }

    public class HookInfo
    {
        public PhysicsEntity Hit = null;
        public JointDistance Joint = null;
        public bool IsBar = false;
    }
}
