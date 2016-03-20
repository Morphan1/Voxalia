using Voxalia.ServerGame.ServerMainSystem;
using FreneticScript.CommandSystem;
using Voxalia.ServerGame.CommandSystem.CommonCommands;
using Voxalia.ServerGame.CommandSystem.PlayerCommands;
using Voxalia.ServerGame.CommandSystem.FileCommands;
using Voxalia.ServerGame.CommandSystem.ItemCommands;
using Voxalia.ServerGame.TagSystem.TagBases;

namespace Voxalia.ServerGame.CommandSystem
{
    /// <summary>
    /// Handles all console commands and key binds.
    /// </summary>
    public class ServerCommands
    {
        /// <summary>
        /// The client that manages this command system.
        /// </summary>
        public Server TheServer;

        /// <summary>
        /// The Commands object that all commands actually go to.
        /// </summary>
        public Commands CommandSystem;

        /// <summary>
        /// The output system.
        /// </summary>
        public Outputter Output;

        /// <summary>
        /// Prepares the command system, registering all base commands.
        /// </summary>
        public void Init(Outputter _output, Server tserver)
        {
            // General Init
            TheServer = tserver;
            CommandSystem = new Commands();
            Output = _output;
            CommandSystem.Output = Output;
            CommandSystem.Init();

            // Common Commands
            CommandSystem.RegisterCommand(new MeminfoCommand(TheServer));
            CommandSystem.RegisterCommand(new QuitCommand(TheServer));
            CommandSystem.RegisterCommand(new SayCommand(TheServer));

            // File Commands
            CommandSystem.RegisterCommand(new AddpathCommand(TheServer));

            // World Commands
            // ...

            // Item Commands
            CommandSystem.RegisterCommand(new AddrecipeCommand(TheServer));
            
            // Player Management Commands
            CommandSystem.RegisterCommand(new KickCommand(TheServer));

            // Tag Bases
            CommandSystem.TagSystem.Register(new ArrowEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new BlockGroupEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new BlockItemEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new BulletEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new ColorTagBase());
            CommandSystem.TagSystem.Register(new EntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new GlowstickEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new GrenadeEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new ItemEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new ItemTagBase(TheServer));
            CommandSystem.TagSystem.Register(new LivingEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new LocationTagBase());
            CommandSystem.TagSystem.Register(new MaterialTagBase());
            CommandSystem.TagSystem.Register(new ModelEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new PhysicsEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new PlayerTagBase(TheServer));
            CommandSystem.TagSystem.Register(new PrimitiveEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new RecipeResultTagBase(TheServer));
            CommandSystem.TagSystem.Register(new RecipeTagBase(TheServer));
            CommandSystem.TagSystem.Register(new RegionTagBase(TheServer));
            CommandSystem.TagSystem.Register(new ServerTagBase(TheServer));
            CommandSystem.TagSystem.Register(new SmokeGrenadeEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new VehicleEntityTagBase(TheServer));
            CommandSystem.TagSystem.Register(new VehiclePartEntityTagBase(TheServer));
        }

        /// <summary>
        /// Advances any running command queues.
        /// </summary>
        public void Tick(double delta)
        {
            CommandSystem.Tick((float)delta);
        }

        /// <summary>
        /// Executes an arbitrary list of command inputs (separated by newlines, semicolons, ...)
        /// </summary>
        /// <param name="commands">The command string to parse.</param>
        public void ExecuteCommands(string commands)
        {
            CommandSystem.ExecuteCommands(commands, null);
        }
    }
}
