using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.ServerMainSystem
{
    public partial class Server
    {
        /// <summary>
        /// How much time has elapsed since the last tick started.
        /// </summary>
        public double Delta;

        /// <summary>
        /// The server's primary tick function.
        /// </summary>
        public void Tick(double delta)
        {
            Delta = delta;
            SysConsole.Output(OutputType.INFO, "Tick: " + Delta); // TODO: Remove me!
        }
    }
}
