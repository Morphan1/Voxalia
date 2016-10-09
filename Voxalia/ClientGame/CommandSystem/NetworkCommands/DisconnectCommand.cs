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
    class DisconnectCommand: AbstractCommand
    {
        public Client TheClient;

        public DisconnectCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "disconnect";
            Description = "Disconnects from the server.";
            Arguments = "";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            TheClient.Network.Disconnect();
        }
    }
}
