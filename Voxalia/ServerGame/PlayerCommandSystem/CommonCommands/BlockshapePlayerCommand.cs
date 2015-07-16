using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
{
    class BlockshapePlayerCommand: AbstractPlayerCommand
    {
        public BlockshapePlayerCommand()
        {
            Name = "blockshape";
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count < 1)
            {
                entry.Player.Network.SendMessage("/blockshape <data>");
                return;
            }
            byte dat = (byte)Utilities.StringToInt(entry.InputArguments[0]);
            Location eye = entry.Player.GetEyePosition();
            CollisionResult cr = entry.Player.TheWorld.Collision.RayTrace(eye, eye + entry.Player.ForwardVector() * 5, entry.Player.IgnoreThis);
            if (cr.Hit && cr.HitEnt == null)
            {
                Location block = cr.Position - cr.Normal * 0.01;
                Material mat = entry.Player.TheWorld.GetBlockMaterial(block);
                if (mat != Material.AIR)
                {
                    entry.Player.TheWorld.SetBlockMaterial(block, mat, dat);
                    entry.Player.Network.SendMessage("Set.");
                    return;
                }
            }
            entry.Player.Network.SendMessage("Failed to set: couldn't hit a block!");
        }
    }
}
