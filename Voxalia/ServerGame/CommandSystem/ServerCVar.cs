using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic;
using Frenetic.CommandSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.CommandSystem
{
    /// <summary>
    /// Handles the serverside CVar system.
    /// </summary>
    public class ServerCVar
    {
        /// <summary>
        /// The CVar System the client will use.
        /// </summary>
        public CVarSystem system;

        // System CVars
        public CVar s_filepath;

        // Game CVars
        public CVar g_timescale;

        /// <summary>
        /// Prepares the CVar system, generating default CVars.
        /// </summary>
        public void Init(Outputter output)
        {
            system = new CVarSystem(output);

            // System CVars
            s_filepath = Register("s_filepath", FileHandler.BaseDirectory, CVarFlag.Textual | CVarFlag.ReadOnly); // The current system environment filepath (The directory of /data).
            // Game CVars
            g_timescale = Register("g_timescale", "1", CVarFlag.Numeric); // The current game time scaling value.
        }

        CVar Register(string name, string value, CVarFlag flags)
        {
            return system.Register(name, value, flags);
        }
    }
}
