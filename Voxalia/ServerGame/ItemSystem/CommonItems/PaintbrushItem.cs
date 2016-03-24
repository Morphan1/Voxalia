using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using Voxalia.ServerGame.OtherSystems;
using Voxalia.ServerGame.WorldSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class PaintbrushItem: GenericItem
    {
        public PaintbrushItem()
        {
            Name = "paintbrush";
        }

        public override void Click(Entity entity, ItemStack item)
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
                else if (player.Mode.GetDetails().CanPlace)
                {
                    Location block = (new Location(rcr.HitData.Location) - new Location(rcr.HitData.Normal).Normalize() * 0.01).GetBlockLocation();
                    block = block.GetBlockLocation();
                    BlockInternal blockdat = player.TheRegion.GetBlockInternal(block);
                    Material mat = (Material)blockdat.BlockMaterial;
                    if (mat != Material.AIR)
                    {
                        int paint = 0;
                        TemplateObject pinteger;
                        if (!item.SharedAttributes.TryGetValue("color", out pinteger))
                        {
                            IntegerTag nt = IntegerTag.TryFor(pinteger);
                            if (nt != null)
                            {
                                paint = (int)nt.Internal;
                            }
                        }
                        player.TheRegion.SetBlockMaterial(block, mat, blockdat.BlockData, (byte)paint, (byte)(blockdat.BlockLocalData | (byte)BlockFlags.EDITED));
                    }
                }
            }
        }
    }
}
