using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using System.Diagnostics;
using System.Threading;

namespace ShadowOperations.ServerGame.ServerMainSystem
{
    /// <summary>
    /// The center of all server activity in Shadow Operations.
    /// </summary>
    public partial class Server
    {
        /// <summary>
        /// The primary running server.
        /// </summary>
        public static Server Central = null;

        /// <summary>
        /// Starts up a new server.
        /// </summary>
        public static void Init()
        {
            Central = new Server();
            Central.StartUp();
        }

        /// <summary>
        /// Start up and run the server.
        /// </summary>
        public void StartUp()
        {
            SysConsole.Output(OutputType.INIT, "Launching as new server, this is " + (this == Central ? "" : "NOT ") + "the Central server.");
            // Tick
            double TARGETFPS = 40d;
            Stopwatch Counter = new Stopwatch();
            Stopwatch DeltaCounter = new Stopwatch();
            DeltaCounter.Start();
            double TotalDelta = 0;
            double CurrentDelta = 0d;
            double TargetDelta = 0d;
            int targettime = 0;
            while (true)
            {
                // Update the tick time usage counter
                Counter.Reset();
                Counter.Start();
                // Update the tick delta counter
                DeltaCounter.Stop();
                // Delta time = Elapsed ticks * (ticks/second)
                CurrentDelta = ((double)DeltaCounter.ElapsedTicks) / ((double)Stopwatch.Frequency);
                // How much time should pass between each tick ideally
                TARGETFPS = 20;// ServerCVar.g_fps.ValueD; // TODO: Use the CVar!
                if (TARGETFPS < 1 || TARGETFPS > 100)
                {
                    //ServerCVar.g_fps.Set("20"); // TODO: Use the CVar!
                    TARGETFPS = 20;
                }
                TargetDelta = (1d / TARGETFPS);
                // How much delta has been built up
                TotalDelta += CurrentDelta;
                if (TotalDelta > TargetDelta * 10)
                {
                    // Lagging - cheat to catch up!
                    TargetDelta *= 3;
                }
                if (TotalDelta > TargetDelta * 10)
                {
                    // Lagging a /lot/ - cheat /extra/ to catch up!
                    TargetDelta *= 3;
                }
                if (TotalDelta > TargetDelta * 10)
                {
                    // At this point, the server's practically dead.
                    TargetDelta *= 3;
                }
                // Give up on acceleration at this point. 50 * 27 = 1.35 seconds / tick under a default tickrate.
                // As long as there's more delta built up than delta wanted, tick
                while (TotalDelta > TargetDelta)
                {
                    Tick(TargetDelta);
                    TotalDelta -= TargetDelta;
                }
                // Begin the delta counter to find out how much time is /really/ slept for
                DeltaCounter.Reset();
                DeltaCounter.Start();
                // The tick is done, stop measuring it
                Counter.Stop();
                // Only sleep for target milliseconds/tick minus how long the tick took... this is imprecise but that's okay
                targettime = (int)((1000d / TARGETFPS) - Counter.ElapsedMilliseconds);
                // Only sleep at all if we're not lagging
                if (targettime > 0)
                {
                    // Try to sleep for the target time - very imprecise, thus we deal with precision inside the tick code
                    Thread.Sleep(targettime);
                }
            }
        }
    }
}
