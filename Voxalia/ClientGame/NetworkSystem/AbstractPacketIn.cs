using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.NetworkSystem
{
    public abstract class AbstractPacketIn
    {
        public Client TheClient;

        /// <summary>
        /// Parse the given byte array and execute the results.
        /// </summary>
        /// <param name="data">The byte array received from a client</param>
        /// <returns>False if the array is invalid, true if it parses successfully</returns>
        public abstract bool ParseBytesAndExecute(byte[] data);
    }
}
