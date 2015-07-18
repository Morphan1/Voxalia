using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
{
    class DevelPlayerCommand : AbstractPlayerCommand
    {
        public DevelPlayerCommand()
        {
            Name = "devel";
            Silent = false;
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count <= 0)
            {
                entry.Player.Network.SendMessage("/devel <subcommand> [ values ... ]");
                return;
            }
            string arg0 = entry.InputArguments[0];
            if (arg0 == "spawnVehicle")
            {
                VehicleEntity ve = new VehicleEntity("failmobile", entry.Player.TheWorld);
                ve.SetPosition(entry.Player.GetEyePosition() + entry.Player.ForwardVector() * 5);
                entry.Player.TheWorld.SpawnEntity(ve);
            }
            else if (arg0 == "chunkDebug")
            {
                float h = QuickGenerator.GetHeight(entry.Player.TheWorld.Seed, entry.Player.TheWorld.Seed2, (float)Math.Floor(entry.Player.GetPosition().X), (float)Math.Floor(entry.Player.GetPosition().Y));
                entry.Player.Network.SendMessage("Generate height: " + h + " ==  " + Math.Round(h));
                BlockInternal bi = entry.Player.TheWorld.GetBlockInternal_NoLoad((entry.Player.GetPosition() + new Location(0, 0, -0.1)).GetBlockLocation());
                entry.Player.Network.SendMessage("Mat: " + ((Material)bi.BlockMaterial) + ", data: " + ((int)bi.BlockData)
                    + ", xp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesXP() + ", xm: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesXM()
                    + ", yp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesYP() + ", ym: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesYM()
                    + ", zp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesTOP() + ", zm: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesBOTTOM());
            }
            else
            {
                entry.Player.Network.SendMessage("/devel <subcommand> [ values ... ]");
                return;
            }
        }
    }
}
