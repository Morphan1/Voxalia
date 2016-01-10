using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.Shared.Files;
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
        public CVar g_timescale, g_fps;

        /// <summary>
        /// Prepares the CVar system, generating default CVars.
        /// </summary>
        public void Init(Outputter output)
        {
            system = new CVarSystem(output);

            // System CVars
            s_filepath = Register("s_filepath", Program.Files.BaseDirectory, CVarFlag.Textual | CVarFlag.ReadOnly, "The current system environment filepath (The directory of /data).");
            // Game CVars
            g_timescale = Register("g_timescale", "1", CVarFlag.Numeric, "The current game time scaling value.");
            g_fps = Register("g_fps", "30", CVarFlag.Numeric, "What framerate to use.");
        }

        CVar Register(string name, string value, CVarFlag flags, string desc)
        {
            return system.Register(name, value, flags, desc);
        }
    }
}
