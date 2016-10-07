using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.ClientMainSystem;
using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;
using FreneticScript.TagHandlers;
using Voxalia.Shared.Collision;
using Voxalia.ClientGame.AudioSystem;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ClientGame.EntitySystem;

namespace Voxalia.ClientGame.CommandSystem.CommonCommands
{
    public class CdevelCommand: AbstractCommand
    {
        public Client TheClient;

        public CdevelCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "cdevel";
            Description = "Clientside developmental commands.";
            Arguments = "";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(queue, entry);
                return;
            }
            switch (entry.GetArgument(queue, 0))
            {
                case "lightDebug":
                    {
                        Location pos = TheClient.Player.GetPosition();
                        pos.Z = pos.Z + 1;
                        int XP = (int)Math.Floor(pos.X / Chunk.CHUNK_SIZE);
                        int YP = (int)Math.Floor(pos.Y / Chunk.CHUNK_SIZE);
                        int ZP = (int)Math.Floor(pos.Z / Chunk.CHUNK_SIZE);
                        int x = (int)(Math.Floor(pos.X) - (XP * Chunk.CHUNK_SIZE));
                        int y = (int)(Math.Floor(pos.Y) - (YP * Chunk.CHUNK_SIZE));
                        int z = (int)(Math.Floor(pos.Z) - (ZP * Chunk.CHUNK_SIZE));
                        while (true)
                        {
                            Chunk ch = TheClient.TheRegion.GetChunk(new Vector3i(XP, YP, ZP));
                            if (ch == null)
                            {
                                entry.Good(queue, "Passed with flying light sources!");
                                goto end;
                            }
                            while (z < Chunk.CHUNK_SIZE)
                            {
                                if (ch.GetBlockAt((int)x, (int)y, (int)z).IsOpaque())
                                {
                                    entry.Info(queue, "Died: " + x + ", " + y + ", " + z + " -- " + XP + ", " + YP + ", " + ZP);
                                    goto end;
                                }
                                z++;
                            }
                            ZP++;
                            z = 0;
                        }
                        end:
                        break;
                    }
                case "vramUsage":
                    {
                        long c = 0;
                        foreach (Tuple<string, long> val in TheClient.CalculateVRAMUsage())
                        {
                            entry.Info(queue, "-> " + val.Item1 + ": " + val.Item2 + " (" + (val.Item2 / 1024 / 1024) + "MB)");
                            c += val.Item2;
                        }
                        entry.Info(queue, "-> Total: " + c + " (" + (c / 1024 / 1024) + "MB)");
                        break;
                    }
                case "speakText":
                    {
                        if (entry.Arguments.Count < 3)
                        {
                            ShowUsage(queue, entry);
                            break;
                        }
                        bool male = !entry.GetArgument(queue, 1).ToString().ToLowerFast().StartsWith("f");
                        TextToSpeech.Speak(entry.GetArgument(queue, 2), male);
                        break;
                    }
                case "chunkInfo":
                    {
                        Chunk ch = TheClient.TheRegion.GetChunk(TheClient.TheRegion.ChunkLocFor(TheClient.Player.GetPosition()));
                        if (ch == null)
                        {
                            entry.Info(queue, "Chunk is null!");
                            break;
                        }
                        entry.Info(queue, "Plants: " + ch.Plant_C + ", generated as ID: " + ch.Plant_VAO);
                        int c = 0;
                        foreach (Chunk chunk in TheClient.TheRegion.LoadedChunks.Values)
                        {
                            if (chunk._VBO != null && ch._VBO != null && chunk._VBO._VAO == ch._VBO._VAO)
                            {
                                c++;
                            }
                        }
                        entry.Info(queue, "Chunk rendering as " + (ch._VBO == null ? "{NULL}" : ch._VBO._VAO.ToString()) + ", which is seen in " + c + " chunks!");
                        break;
                    }
                case "blockInfo":
                    {
                        BlockInternal bi = TheClient.TheRegion.GetBlockInternal(TheClient.Player.GetPosition());
                        entry.Info(queue, "BLOCK: Material=" + bi.Material + ", Shape=" + bi.BlockData + ", Damage=" + bi.Damage + ", Paint=" + bi.BlockPaint + ",Light=" + bi.BlockLocalData);
                        break;
                    }
                case "igniteBlock":
                    {
                        Location pos = TheClient.Player.GetPosition().GetUpperBlockBorder();
                        FireEntity fe = new FireEntity(pos, null, TheClient.TheRegion);
                        TheClient.TheRegion.SpawnEntity(fe);
                        break;
                    }
                case "torchBlocks":
                    {
                        Location pos = TheClient.Player.GetPosition().GetUpperBlockBorder();
                        for (int x = -3; x <= 3; x++)
                        {
                            for (int y = -3; y <= 3; y++)
                            {
                                FireEntity fe = new FireEntity(pos + new Location(x, y, 0), null, TheClient.TheRegion);
                                TheClient.TheRegion.SpawnEntity(fe);
                            }
                        }
                        break;
                    }
                default:
                    ShowUsage(queue, entry);
                    break;
            }
        }
    }
}
