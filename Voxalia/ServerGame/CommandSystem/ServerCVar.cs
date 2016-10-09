//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.Shared.Files;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;

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
        public CVar s_filepath, s_debug;

        // Game CVars
        public CVar g_fps, g_maxheight, g_minheight, g_maxdist, g_renderblocks;

        // Network CVars
        public CVar n_verifyip, n_rendersides, n_chunkspertick, n_online;

        // Text CVars
        public CVar t_translateurls, t_blockurls, t_blockcolors;

        /// <summary>
        /// Prepares the CVar system, generating default CVars.
        /// </summary>
        public void Init(Server tserver, Outputter output)
        {
            system = new CVarSystem(output);

            // System CVars
            s_filepath = Register("s_filepath", tserver.Files.BaseDirectory, CVarFlag.Textual | CVarFlag.ReadOnly, "The current system environment filepath (The directory of /data)."); // TODO: Scrap this! Tags!
            s_debug = Register("s_debug", "true", CVarFlag.Boolean, "Whether to output debug information.");
            // Game CVars
            //g_timescale = Register("g_timescale", "1", CVarFlag.Numeric, "The current game time scaling value.");
            g_fps = Register("g_fps", "30", CVarFlag.Numeric, "What framerate to use.");
            g_maxheight = Register("g_maxheight", "5000", CVarFlag.Numeric, "What the highest possible Z coordinate should be (for building)."); // TODO: Also per-world?
            g_minheight = Register("g_minheight", "-5000", CVarFlag.Numeric, "What the lowest possible Z coordinate should be (for building)."); // TODO: Also per-world?
            g_maxdist = Register("g_maxdist", "50000", CVarFlag.Numeric, "How far on the X or Y axis a player may travel from the origin."); // TODO: Also per-world?
            g_renderblocks = Register("g_renderblocks", "false", CVarFlag.Boolean, "Whether to render blocks for mapping purposes."); // TODO: Also per-world?
            // Network CVars
            n_verifyip = Register("n_verifyip", "true", CVarFlag.Boolean, "Whether to verify connecting users' IP addresses with the global server. Disabling this may help allow LAN connections.");
            n_rendersides = Register("n_rendersides", "false", CVarFlag.Boolean, "Whether to render the side-on map view for the linked webpage."); // TODO: Also per-world?
            n_chunkspertick = Register("n_chunkspertick", "15", CVarFlag.Numeric, "How many chunks can be sent in a single server tick, per player.");
            n_online = Register("n_online", "true", CVarFlag.Boolean, "Whether the server with authorize connections against the global server. Disable this if you want to play singleplayer without a live internet connection.");
            // Text CVars
            t_translateurls = Register("t_translateurls", "true", CVarFlag.Boolean, "Whether to automatically translate URLs posted in chat.");
            t_blockurls = Register("t_blockurls", "false", CVarFlag.Boolean, "Whether to block URLs as input to chat.");
            t_blockcolors = Register("t_blockcolors", "false", CVarFlag.Boolean, "Whether to block colors as input to chat.");
        }

        CVar Register(string name, string value, CVarFlag flags, string desc)
        {
            return system.Register(name, value, flags, desc);
        }
    }
}
