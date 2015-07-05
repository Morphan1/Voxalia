using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class BlockItem: BaseItemInfo
    {
        public BlockItem()
            : base()
        {
            Name = "block";
        }

        public override void PrepItem(PlayerEntity player, ItemStack item)
        {
        }

        public override void AltClick(PlayerEntity player, ItemStack item)
        {
            Location eye = player.GetEyePosition();
            CollisionResult cr = player.TheWorld.Collision.RayTrace(eye, eye + player.ForwardVector() * 5, player.IgnoreThis);
            if (cr.Hit)
            {
                Location block = player.TheWorld.GetBlockLocation(cr.Position + cr.Normal * 0.5);
                Material mat = player.TheWorld.GetBlockMaterial(block);
                if (mat == Material.AIR) // TODO: IsPlaceableIn
                {
                    CollisionResult hit = player.TheWorld.Collision.CuboidLineTrace(new Location(0.41, 0.41, 0.41), block + new Location(0.5),
                        block + new Location(0.5, 0.5, 0.501), player.TheWorld.Collision.ShouldCollide);
                    if (!hit.Hit)
                    {
                        player.TheWorld.SetBlockMaterial(block, (Material)item.Datum);
                        item.Count = item.Count - 1;
                        if (item.Count <= 0)
                        {
                            player.RemoveItem(player.cItem);
                        }
                        else
                        {
                            player.Network.SendPacket(new SetItemPacketOut(player.Items.IndexOf(item), item));
                        }
                    }
                    else
                    {
                        SysConsole.Output(OutputType.INFO, "HIT: " + (hit.HitEnt == null ? "Null" : hit.HitEnt.Tag));
                    }
                }
            }
        }

        public override void Click(PlayerEntity player, ItemStack item)
        {
            // TODO: Break?
        }

        public override void ReleaseClick(PlayerEntity player, ItemStack item)
        {
        }

        public override void Use(EntitySystem.PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchFrom(PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchTo(PlayerEntity player, ItemStack item)
        {
        }

        public override void Tick(PlayerEntity player, ItemStack item)
        {
        }
    }
}
