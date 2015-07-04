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

        public override void PrepItem(PlayerEntity player, ItemStack item)
        {
        }

        public override void AltClick(PlayerEntity player, ItemStack item)
        {
        }

        public override void Click(PlayerEntity player, ItemStack item)
        {
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
                        player.TheWorld.SetBlockMaterial(block, Material.AIR);
                    }
                }
            }
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
