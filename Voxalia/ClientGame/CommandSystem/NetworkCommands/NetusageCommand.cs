using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.NetworkSystem;

namespace Voxalia.ClientGame.CommandSystem.NetworkCommands
{
    public class NetusageCommand: AbstractCommand
    {
        public Client TheClient;

        public NetusageCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "netusage";
            Description = "Shows information on network usage.";
            Arguments = "";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            entry.Info(queue, "Network usage (last second): " + GetUsages(TheClient.Network.UsagesLastSecond));
            entry.Info(queue, "Network usage (total): " + GetUsages(TheClient.Network.UsagesTotal));
        }

        public string GetUsages(long[] usages)
        {
            return "Effects: " + usages[(int)NetUsageType.EFFECTS]
                + ", entities: " + usages[(int)NetUsageType.ENTITIES]
                + ", players: " + usages[(int)NetUsageType.PLAYERS]
                + ", clouds: " + usages[(int)NetUsageType.CLOUDS]
                + ", pings: " + usages[(int)NetUsageType.PINGS]
                + ", chunks: " + usages[(int)NetUsageType.CHUNKS]
                + ", other: " + usages[(int)NetUsageType.GENERAL];
        }
    }
}
