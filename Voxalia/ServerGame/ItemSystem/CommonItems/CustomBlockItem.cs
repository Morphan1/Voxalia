using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.EntitySystem;
using BEPUphysics;
using BEPUutilities;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using Voxalia.ServerGame.OtherSystems;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class CustomBlockItem : GenericItem
    {
        public CustomBlockItem()
        {
            Name = "customblock";
        }

        public override void AltClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            Location eye = player.GetEyePosition();
            Location forw = player.ForwardVector();
            RayCastResult rcr;
            bool h = player.TheRegion.SpecialCaseRayTrace(eye, forw, 5, MaterialSolidity.ANY, player.IgnoreThis, out rcr);
            if (h)
            {
                if (rcr.HitObject != null && rcr.HitObject is EntityCollidable && ((EntityCollidable)rcr.HitObject).Entity != null)
                {
                    // TODO: ???
                }
                else if (player.Mode.GetDetails().CanPlace && player.TheRegion.GlobalTickTime - player.LastBlockPlace >= 0.2)
                {
                    Location block = new Location(rcr.HitData.Location) + new Location(rcr.HitData.Normal).Normalize() * 0.9f;
                    block = block.GetBlockLocation();
                    Material mat = player.TheRegion.GetBlockMaterial(block);
                    if (player.TheRegion.IsAllowedToPlaceIn(player, block, mat))
                    {
                        CollisionResult hit = player.TheRegion.Collision.CuboidLineTrace(new Location(0.45, 0.45, 0.45), block + new Location(0.5),
                            block + new Location(0.5, 0.5, 0.501), player.TheRegion.Collision.ShouldCollide);
                        if (!hit.Hit)
                        {
                            BlockInternal bi = BlockInternal.FromItemDatum(item.Datum);
                            MusicBlockEntity mbe = new MusicBlockEntity(player.TheRegion, item, block); // TODO: Vary based on material!
                            player.TheRegion.SpawnEntity(mbe);
                            player.Network.SendPacket(new DefaultSoundPacketOut(block, DefaultSound.PLACE, (byte)((Material)bi.BlockMaterial).Sound()));
                            item.Count = item.Count - 1;
                            if (item.Count <= 0)
                            {
                                player.Items.RemoveItem(player.Items.cItem);
                            }
                            else
                            {
                                player.Items.SetSlot(player.Items.cItem - 1, item);
                            }
                            player.LastBlockPlace = player.TheRegion.GlobalTickTime;
                        }
                    }
                }
            }
        }

        public override void ReleaseAltClick(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: non-player support
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.LastBlockPlace = 0;
        }
    }
}
