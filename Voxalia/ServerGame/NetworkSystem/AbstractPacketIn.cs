//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.NetworkSystem
{
    public abstract class AbstractPacketIn
    {
        /// <summary>
        /// The player that sent this packet.
        /// </summary>
        public PlayerEntity Player;

        /// <summary>
        /// Parse the given byte array and execute the results.
        /// </summary>
        /// <param name="data">The byte array received from a client.</param>
        /// <returns>False if the array is invalid, true if it parses successfully.</returns>
        public abstract bool ParseBytesAndExecute(byte[] data);

        public bool Chunk = false;
    }
}
