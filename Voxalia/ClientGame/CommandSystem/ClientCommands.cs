using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.CommandSystem.UICommands;
using Voxalia.ClientGame.CommandSystem.CommonCommands;
using Voxalia.ClientGame.CommandSystem.NetworkCommands;
using Voxalia.ClientGame.CommandSystem.GameCommands;
using Voxalia.ClientGame.CommandSystem.TagBases;

namespace Voxalia.ClientGame.CommandSystem
{
    /// <summary>
    /// Handles all console commands and key binds.
    /// </summary>
    public class ClientCommands
    {
        /// <summary>
        /// The client that manages this command system.
        /// </summary>
        public Client TheClient;

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
        public void Init(Outputter _output, Client tclient)
        {
            // General Init
            TheClient = tclient;
            CommandSystem = new Commands();
            Output = _output;
            CommandSystem.Output = Output;
            CommandSystem.Init();

            // UI Commands
            CommandSystem.RegisterCommand(new AttackCommand(TheClient));
            CommandSystem.RegisterCommand(new BackwardCommand(TheClient));
            CommandSystem.RegisterCommand(new BindblockCommand(TheClient));
            CommandSystem.RegisterCommand(new BindCommand(TheClient));
            CommandSystem.RegisterCommand(new ForwardCommand(TheClient));
            CommandSystem.RegisterCommand(new ItemdownCommand(TheClient));
            CommandSystem.RegisterCommand(new ItemleftCommand(TheClient));
            CommandSystem.RegisterCommand(new ItemrightCommand(TheClient));
            CommandSystem.RegisterCommand(new ItemupCommand(TheClient));
            CommandSystem.RegisterCommand(new LeftwardCommand(TheClient));
            CommandSystem.RegisterCommand(new MovedownCommand(TheClient));
            CommandSystem.RegisterCommand(new RightwardCommand(TheClient));
            CommandSystem.RegisterCommand(new SecondaryCommand(TheClient));
            CommandSystem.RegisterCommand(new SprintCommand(TheClient));
            CommandSystem.RegisterCommand(new UnbindCommand(TheClient));
            CommandSystem.RegisterCommand(new UpwardCommand(TheClient));
            CommandSystem.RegisterCommand(new UseCommand(TheClient));
            CommandSystem.RegisterCommand(new WalkCommand(TheClient));

            // Common Commands
            CommandSystem.RegisterCommand(new CdevelCommand(TheClient));
            CommandSystem.RegisterCommand(new ItemnextCommand(TheClient));
            CommandSystem.RegisterCommand(new ItemprevCommand(TheClient));
            CommandSystem.RegisterCommand(new ItemselCommand(TheClient));
            CommandSystem.RegisterCommand(new PlayCommand(TheClient));
            CommandSystem.RegisterCommand(new QuitCommand(TheClient));
            CommandSystem.RegisterCommand(new ReloadCommand(TheClient));

            // Network Commands
            CommandSystem.RegisterCommand(new ConnectCommand(TheClient));
            CommandSystem.RegisterCommand(new DisconnectCommand(TheClient));
            CommandSystem.RegisterCommand(new NetusageCommand(TheClient));
            CommandSystem.RegisterCommand(new StartlocalserverCommand(TheClient));

            // Game Commands
            CommandSystem.RegisterCommand(new InventoryCommand(TheClient));
            CommandSystem.RegisterCommand(new TesteffectCommand(TheClient));

            // Entity Tags
            CommandSystem.TagSystem.Register(new PlayerTags(TheClient));

            CommandSystem.PostInit();
        }

        /// <summary>
        /// Advances any running command queues.
        /// </summary>
        public void Tick()
        {
            CommandSystem.Tick((float)TheClient.Delta);
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
