using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic;
using Frenetic.CommandSystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.CommandSystem
{
    /// <summary>
    /// Handles the clientside CVar system.
    /// </summary>
    public class ClientCVar
    {
        /// <summary>
        /// The CVar System the client will use.
        /// </summary>
        public CVarSystem system;

        // System CVars
        public CVar s_filepath;

        // Network CVars
        public CVar n_first;

        // Renderer CVars
        public CVar r_fullscreen, r_width, r_height, r_antialiasing, r_lighting;

        // UI CVars
        public CVar u_mouse_sensitivity;

        /// <summary>
        /// Prepares the CVar system, generating default CVars.
        /// </summary>
        public void Init(Outputter output)
        {
            system = new CVarSystem(output);

            // System CVars
            s_filepath = Register("s_filepath", FileHandler.BaseDirectory, CVarFlag.Textual | CVarFlag.ReadOnly); // The current system environment filepath (The directory of /data).
            // Network CVars
            n_first = Register("n_first", "ipv4", CVarFlag.Textual); // Whether to prefer IPv4 or IPv6.
            // Renderer CVars
            r_fullscreen = Register("r_fullscreen", "false", CVarFlag.Boolean | CVarFlag.Delayed); // Whether to use fullscreen mode.
            r_width = Register("r_width", "800", CVarFlag.Numeric | CVarFlag.Delayed); // What width the window should be.
            r_height = Register("r_height", "600", CVarFlag.Numeric | CVarFlag.Delayed); // What height the window should be.
            r_antialiasing = Register("r_antialiasing", "2", CVarFlag.Numeric | CVarFlag.Delayed); // What AA mode to use (0 = none).
            r_lighting = Register("r_lighting", "true", CVarFlag.Boolean); // Whether to enable 3D lighting (Otherwise, use FullBright).
            // UI CVars
            u_mouse_sensitivity = Register("u_mouse_sensitivity", "1", CVarFlag.Numeric); // How sensitive the mouse is.
        }

        CVar Register(string name, string value, CVarFlag flags)
        {
            return system.Register(name, value, flags);
        }
    }
}
