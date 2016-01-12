using System;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using BEPUphysics;
using BEPUutilities;
using Voxalia.ServerGame.OtherSystems;

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
            if (arg0 == "spawnVehicle" && entry.InputArguments.Count > 1)
            {
                VehicleEntity ve = new VehicleEntity(entry.InputArguments[1], entry.Player.TheRegion);
                ve.SetPosition(entry.Player.GetEyePosition() + entry.Player.ForwardVector() * 5);
                entry.Player.TheRegion.SpawnEntity(ve);
            }
            else if (arg0 == "fly")
            {
                if (entry.Player.IsFlying)
                {
                    entry.Player.Unfly();
                    entry.Player.Network.SendMessage("Unflying!");
                }
                else
                {
                    entry.Player.Fly();
                    entry.Player.Network.SendMessage("Flying!");
                }
            }
            else if (arg0 == "playerDebug")
            {
                entry.Player.Network.SendMessage("YOU: " + entry.Player.Name + ", tractionForce: " + entry.Player.CBody.TractionForce
                     + ", mass: " + entry.Player.CBody.Body.Mass + ", radius: " + entry.Player.CBody.BodyRadius + ", hasSupport: " + entry.Player.CBody.SupportFinder.HasSupport
                     + ", hasTraction: " + entry.Player.CBody.SupportFinder.HasTraction);
            }
            else if (arg0 == "chunkDebug")
            {
                Biome biome;
                Location posBlock = entry.Player.GetPosition().GetBlockLocation();
                float h = entry.Player.TheRegion.Generator.GetHeight(entry.Player.TheRegion.Seed, entry.Player.TheRegion.Seed2, entry.Player.TheRegion.Seed3,
                    entry.Player.TheRegion.Seed4, entry.Player.TheRegion.Seed5, (float)posBlock.X, (float)posBlock.Y, (float)posBlock.Z, out biome);
                BlockInternal bi = entry.Player.TheRegion.GetBlockInternal_NoLoad((entry.Player.GetPosition() + new Location(0, 0, -0.05f)).GetBlockLocation());
                entry.Player.Network.SendMessage("Mat: " + ((Material)bi.BlockMaterial) + ", data: " + ((int)bi.BlockData) + ", locDat: " + ((int)bi.BlockLocalData)
                    + ", xp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesXP() + ", xm: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesXM()
                    + ", yp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesYP() + ", ym: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesYM()
                    + ", zp: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesTOP() + ", zm: " + BlockShapeRegistry.BSD[bi.BlockData].OccupiesBOTTOM());
                float temp = entry.Player.TheRegion.BiomeGen.GetTemperature(entry.Player.TheRegion.Seed2, entry.Player.TheRegion.Seed3, (float)posBlock.X, (float)posBlock.Y);
                float down = entry.Player.TheRegion.BiomeGen.GetDownfallRate(entry.Player.TheRegion.Seed3, entry.Player.TheRegion.Seed4, (float)posBlock.X, (float)posBlock.Y);
                entry.Player.Network.SendMessage("Height: " + h + ", temperature: " + temp + ", downfallrate: " + down + ", biome yield: " + biome.GetName());
            }
            else if (arg0 == "structureCreate" && entry.InputArguments.Count > 1)
            {
                string arg1 = entry.InputArguments[1];
                entry.Player.Items.GiveItem(new ItemStack("structurecreate", arg1, entry.Player.TheServer, 1, "items/admin/strucutre_create",
                    "Structure Creator", "Creates a " + arg1 + " structure!", System.Drawing.Color.White, "items/admin/structure_create", false));
            }
            else if (arg0 == "structurePaste" && entry.InputArguments.Count > 1)
            {
                string arg1 = entry.InputArguments[1];
                entry.Player.Items.GiveItem(new ItemStack("structurepaste", arg1, entry.Player.TheServer, 1, "items/admin/strucutre_paste",
                    "Structor Paster", "Pastes a " + arg1 + " structure!", System.Drawing.Color.White, "items/admin/structure_paste", false));
            }
            else if (arg0 == "spawnSmallPlant" && entry.InputArguments.Count > 1)
            {
                entry.Player.TheRegion.SpawnSmallPlant(entry.InputArguments[1].ToLowerInvariant(), entry.Player.GetPosition());
            }
            else if (arg0 == "spawnTree" && entry.InputArguments.Count > 1)
            {
                entry.Player.TheRegion.SpawnTree(entry.InputArguments[1].ToLowerInvariant(), entry.Player.GetPosition());
            }
            else if (arg0 == "gameMode" && entry.InputArguments.Count > 1)
            {
                GameMode mode;
                if (Enum.TryParse(entry.InputArguments[1].ToUpperInvariant(), out mode))
                {
                    entry.Player.Mode = mode;
                }
            }
            else
            {
                entry.Player.Network.SendMessage("/devel <subcommand> [ values ... ]");
                return;
            }
        }
    }
}
