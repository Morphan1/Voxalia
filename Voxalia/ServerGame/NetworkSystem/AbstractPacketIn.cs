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
        /// <param name="data">The byte array received from a client</param>
        /// <returns>False if the array is invalid, true if it parses successfully</returns>
        public abstract bool ParseBytesAndExecute(byte[] data);

        public bool Chunk = false;
    }
}
