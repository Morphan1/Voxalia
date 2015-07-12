using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class FistItem: BaseItemInfo
    {
        public FistItem()
            : base()
        {
            Name = "fist";
        }

        public override void PrepItem(Entity player, ItemStack item)
        {
        }

        public override void AltClick(Entity player, ItemStack item)
        {
        }

        public override void Click(Entity ent, ItemStack item)
        {
            if (!(ent is PlayerEntity))
            {
                // TODO: update to generic entity
                return;
            }
            PlayerEntity player = (PlayerEntity)ent;
            Location eye = player.GetEyePosition();
            CollisionResult cr = player.TheWorld.Collision.RayTrace(eye, eye + player.ForwardVector() * 5, player.IgnoreThis);
            if (cr.Hit)
            {
                if (cr.HitEnt != null)
                {
                    // TODO: Damage
                }
                else
                {
                    Location block = cr.Position - cr.Normal * 0.01;
                    Material mat = player.TheWorld.GetBlockMaterial(block);
                    if (mat != Material.AIR) // TODO: IsBreakable
                    {
                        player.TheWorld.BreakNaturally(block);
                    }
                }
            }
        }

        public override void ReleaseClick(Entity player, ItemStack item)
        {
        }

        public override void Use(Entity player, ItemStack item)
        {
        }

        public override void SwitchFrom(Entity player, ItemStack item)
        {
        }

        public override void SwitchTo(Entity player, ItemStack item)
        {
        }

        public override void Tick(Entity player, ItemStack item)
        {
        }
    }
}
