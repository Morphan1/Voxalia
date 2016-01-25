using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.Shared.Files;
using Voxalia.Shared;

namespace Voxalia.ClientGame.CommandSystem
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
        public CVar s_filepath, s_glversion, s_glrenderer, s_glvendor, s_littleendian;

        // Game CVars
        public CVar g_timescale, g_firstperson;

        // Network CVars
        public CVar n_first, n_debugmovement, n_movement_maxdistance, n_movement_adjustment, n_movemode;

        // Renderer CVars
        public CVar r_fullscreen, r_width, r_height, r_antialiasing, r_vsync, r_lighting, r_renderwireframe,
            r_fov, r_znear, r_zfar,
            r_dof_strength,
            r_maxfps,
            r_lightmaxdistance, r_fallbacklighting,
            r_shadowquality_flashlight, r_shadowquality_max, r_shadowblur, r_shadowquality_sun, r_shadowpace, r_shadows,
            r_good_graphics, r_skybox, r_lensflare, r_blocktexturelinear, r_blocktexturewidth, r_toonify, r_transplighting, r_transpshadows,
            r_godrays, r_godray_samples, r_godray_wexposure, r_godray_decay, r_godray_density, r_godray_color;

        // Audio CVars
        public CVar a_musicvolume, a_musicpitch, a_globalvolume, a_globalpitch, a_music, a_quietondeselect;

        // UI CVars
        public CVar u_mouse_sensitivity, u_reticle, u_reticlescale, u_showhud,
            u_highlight_targetblock, u_highlight_placeblock, u_showping,
            u_debug, u_showmap, u_showrangefinder;

        /// <summary>
        /// Prepares the CVar system, generating default CVars.
        /// </summary>
        public void Init(Outputter output)
        {
            system = new CVarSystem(output);

            // System CVars
            s_filepath = Register("s_filepath", Program.Files.BaseDirectory, CVarFlag.Textual | CVarFlag.ReadOnly, "The current system environment filepath (The directory of /data).");
            s_glversion = Register("s_glversion", "UNKNOWN", CVarFlag.Textual | CVarFlag.ReadOnly, "What version of OpenGL is in use.");
            s_glrenderer = Register("s_glrenderer", "UNKNOWN", CVarFlag.Textual | CVarFlag.ReadOnly, "What renderer for OpenGL is in use.");
            s_glvendor = Register("s_glvendor", "UNKNOWN", CVarFlag.Textual | CVarFlag.ReadOnly, "What graphics card vendor made the device being used for rendering.");
            // Game CVars
            g_timescale = Register("g_timescale", "1", CVarFlag.Numeric | CVarFlag.ServerControl, "The current game time scale value.");
            g_firstperson = Register("g_firstperson", "true", CVarFlag.Boolean, "Whether to be in FirstPerson view mode.");
            // Network CVars
            n_first = Register("n_first", "ipv4", CVarFlag.Textual, "Whether to prefer IPv4 or IPv6.");
            n_debugmovement = Register("n_debugmovement", "false", CVarFlag.Boolean, "Whether to debug movement networking.");
            n_movement_maxdistance = Register("n_movement_maxdistance", "20", CVarFlag.Numeric, "How far apart the client can move from the serverside player before snapping to the correct location.");
            n_movement_adjustment = Register("n_movement_adjustment", "0.1", CVarFlag.Numeric, "How rapidly to adjust the player's position to better match the server. Smaller numbers yield quicker results.");
            n_movemode = Register("n_movemode", "2", CVarFlag.Numeric, "Which movement mode to use. 1 = run-and-adjust, 2 = back-trace.");
            // Renderer CVars
            r_fullscreen = Register("r_fullscreen", "false", CVarFlag.Boolean | CVarFlag.Delayed, "Whether to use fullscreen mode.");
            r_width = Register("r_width", "1280", CVarFlag.Numeric | CVarFlag.Delayed, "What width the window should be.");
            r_height = Register("r_height", "720", CVarFlag.Numeric | CVarFlag.Delayed, "What height the window should be.");
            r_antialiasing = Register("r_antialiasing", "2", CVarFlag.Numeric | CVarFlag.Delayed, "What AA mode to use (0 = none)."); // TODO: IMPLEMENT
            r_vsync = Register("r_vsync", "true", CVarFlag.Boolean, "Whether to use vertical synchronization mode.");
            r_lighting = Register("r_lighting", "true", CVarFlag.Boolean, "Whether to enable 3D lighting (Otherwise, use FullBright).");
            r_renderwireframe = Register("r_renderwireframe", "false", CVarFlag.Boolean, "Whether to render a wireframe.");
            r_fov = Register("r_fov", "70", CVarFlag.Numeric, "What Field of Vision range value to use.");
            r_znear = Register("r_znear", "0.1", CVarFlag.Numeric, "How close the near plane should be to the camera.");
            r_zfar = Register("r_zfar", "1000", CVarFlag.Numeric, "How far the far plane should be from the camera.");
            r_dof_strength = Register("r_dof_strength", "4", CVarFlag.Numeric, "How strong the Depth Of Field effect should be.");
            r_maxfps = Register("r_maxfps", "60", CVarFlag.Numeric | CVarFlag.Delayed, "What the FPS cap should be.");
            r_lightmaxdistance = Register("r_lightmaxdistance", "35", CVarFlag.Numeric, "How far away a light can be from the camera before it is disabled.");
            r_fallbacklighting = Register("r_fallbacklighting", "true", CVarFlag.Boolean, "Whether to calculate fallback block lighting (Requires chunk reload).");
            r_shadowquality_flashlight = Register("r_shadowquality_flashlight", "512", CVarFlag.Numeric, "What texture size to use for flashlight shadows.");
            r_shadowquality_max = Register("r_shadowquality_max", "2048", CVarFlag.Numeric, "What maximum light texture size to accept from the server.");
            r_shadowblur = Register("r_shadowblur", "0.25", CVarFlag.Numeric, "What factor to use for shadow blurring. Smaller = blurrier.");
            r_shadowquality_sun = Register("r_shadowquality_sun", "2048", CVarFlag.Numeric | CVarFlag.Delayed, "What texture size to use for the sun."); // TODO: Callback to auto-set
            r_shadowpace = Register("r_shadowpace", "1", CVarFlag.Numeric, "How rapidly to rerender shadows, in frames.");
            r_shadows = Register("r_shadows", "false", CVarFlag.Boolean, "Whether to render shadows at all.");
            r_good_graphics = Register("r_good_graphics", "true", CVarFlag.Boolean | CVarFlag.Delayed, "Whether to use 'good' graphics."); // TODO: Callback to auto-set
            r_skybox = Register("r_skybox", "default", CVarFlag.ServerControl | CVarFlag.Textual, "What skybox to use.");
            r_lensflare = Register("r_lensflare", "true", CVarFlag.Boolean, "Whether to render a lens flare from the sun.");
            r_blocktexturelinear = Register("r_blocktexturelinear", "true", CVarFlag.Boolean | CVarFlag.Delayed, "Whether block textures are to use a linear blur or nearest-pixel mode.");
            r_blocktexturewidth = Register("r_blocktexturewidth", "128", CVarFlag.Numeric | CVarFlag.Delayed, "What texture width (pixels) block textures should use.");
            r_godrays = Register("r_godrays", "true", CVarFlag.Boolean, "Whether to render GodRays (rays of light from the sun."); // TODO: Validate?
            r_godray_samples = Register("r_godray_samples", "75", CVarFlag.Numeric, "How many samples to use when generating GodRays."); // TODO: Validate?
            r_godray_wexposure = Register("r_godray_wexposure", (0.003f * 5.65f).ToString(), CVarFlag.Numeric, "What weighted exposure value to use when generating GodRays."); // TODO: Validate?
            r_godray_decay = Register("r_godray_decay", "1", CVarFlag.Numeric, "What decay value to use when generating GodRays."); // TODO: Validate?
            r_godray_density = Register("r_godray_density", "0.84", CVarFlag.Numeric, "What density value to use when generating GodRays."); // TODO: Validate?
            r_godray_color = Register("r_godray_color", "1,1,1", CVarFlag.Textual, "What color to use for GodRays.");
            r_toonify = Register("r_toonify", "false", CVarFlag.Boolean | CVarFlag.Delayed, "Whether to use a 'toonify' post-processing effect."); // TODO: callback to auto-set
            r_transplighting = Register("r_transplighting", "true", CVarFlag.Boolean, "Whether transparent objects should be lit properly (otherwise, fullbright).");
            r_transpshadows = Register("r_transpshadows", "false", CVarFlag.Boolean, "Whether transparent objects should be lit using HD shadows (Requires r_shadows true).");
            // Audio CVars
            a_musicvolume = Register("a_musicvolume", "1", CVarFlag.Numeric, "What volume the music should be.");
            a_musicpitch = Register("a_musicpitch", "1", CVarFlag.Numeric, "What pitch the music should be.");
            a_globalvolume = Register("a_globalvolume", "1", CVarFlag.Numeric, "What volume all sounds should be.");
            a_globalpitch = Register("a_globalpitch", "1", CVarFlag.Numeric, "What pitch all sounds should be.");
            a_music = Register("a_music", "music/epic/bcvoxalia", CVarFlag.Textual | CVarFlag.ServerControl, "What music should be played.");
            a_quietondeselect = Register("a_quietondeselect", "true", CVarFlag.Boolean, "Whether to quiet music when the window is deselected.");
            // UI CVars
            u_mouse_sensitivity = Register("u_mouse_sensitivity", "1", CVarFlag.Numeric, "How sensitive the mouse is.");
            u_reticle = Register("u_reticle", "1", CVarFlag.Textual, "What reticle to use.");
            u_reticlescale = Register("u_reticlescale", "32", CVarFlag.Numeric, "How big the reticle should be.");
            u_showhud = Register("u_showhud", "true", CVarFlag.Boolean, "Whether to render the HUD.");
            u_highlight_targetblock = Register("u_highlight_targetblock", "true", CVarFlag.Boolean, "Whether to highlight the targeted block.");
            u_highlight_placeblock = Register("u_highlight_placeblock", "true", CVarFlag.Boolean, "Whether to highlight the targeted placement block.");
            u_debug = Register("u_debug", "false", CVarFlag.Boolean, "Whether to display debug information on the HUD.");
            u_showmap = Register("u_showmap", "false", CVarFlag.Boolean | CVarFlag.ServerControl, "Whether to display a map on the HUD.");
            u_showrangefinder = Register("u_showrangefinder", "false", CVarFlag.Boolean | CVarFlag.ServerControl, "Whether to display a range finder on the HUD.");
            u_showping = Register("u_showping", "true", CVarFlag.Boolean, "Whether to display the current ping on the UI.");
        }

        CVar Register(string name, string value, CVarFlag flags, string desc = null)
        {
            return system.Register(name, value, flags, desc);
        }
    }
}
