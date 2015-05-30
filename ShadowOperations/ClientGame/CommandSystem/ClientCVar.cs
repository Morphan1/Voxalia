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
        public CVar r_fullscreen, r_width, r_height, r_antialiasing, r_lighting, r_renderwireframe,
            r_fov, r_znear, r_zfar,
            r_ssao, r_dof_strength,
            r_shadowquality_flashlight, r_shadowquality_max;

        // Audio CVars
        public CVar a_musicvolume, a_musicpitch, a_globalvolume, a_globalpitch;

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
            r_renderwireframe = Register("r_renderwireframe", "false", CVarFlag.Boolean); // Whether to render a wireframe.
            r_fov = Register("r_fov", "70", CVarFlag.Numeric); // What Field of Vision range value to use.
            r_znear = Register("r_znear", "0.1", CVarFlag.Numeric); // How close the near plane should be to the camera.
            r_zfar = Register("r_zfar", "1000", CVarFlag.Numeric); // How far the far plane should be from the camera.
            r_ssao = Register("r_ssao", "false", CVarFlag.Boolean); // Whether to render with SSAO.
            r_dof_strength = Register("r_dof_strength", "0.5", CVarFlag.Numeric); // How strong the Depth Of Field effect should be.
            r_shadowquality_flashlight = Register("r_shadowquality_flashlight", "512", CVarFlag.Numeric); // What texture size to use for flashlight shadows.
            r_shadowquality_max = Register("r_shadowquality_max", "2048", CVarFlag.Numeric); // What maximum light texture size to accept from the server.
            // Audio CVars
            a_musicvolume = Register("a_musicvolume", "1", CVarFlag.Numeric | CVarFlag.Delayed); // What volume the music should be.
            a_musicpitch = Register("a_musicpitch", "1", CVarFlag.Numeric | CVarFlag.Delayed); // What pitch the music should be.
            a_globalvolume = Register("a_globalvolume", "1", CVarFlag.Numeric); // What volume all sounds should be.
            a_globalpitch = Register("a_globalpitch", "1", CVarFlag.Numeric); // What pitch all sounds should be.
            // UI CVars
            u_mouse_sensitivity = Register("u_mouse_sensitivity", "1", CVarFlag.Numeric); // How sensitive the mouse is.
        }

        CVar Register(string name, string value, CVarFlag flags)
        {
            return system.Register(name, value, flags);
        }
    }
}
