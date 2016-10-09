//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.NetworkCommands
{
    class ConnectCommand: AbstractCommand
    {
        public Client TheClient;

        public ConnectCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "connect";
            Description = "Connects to a server.";
            Arguments = "<ip> <port>";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 2)
            {
                ShowUsage(queue, entry);
                return;
            }
            entry.Good(queue, "Connecting...");
            TheClient.Network.Connect(entry.GetArgument(queue, 0), entry.GetArgument(queue, 1));
        }
    }
}
