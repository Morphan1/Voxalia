using System.Collections.Generic;
using Frenetic.CommandSystem;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class TargetScriptRunnerEntity: TargetEntity
    {
        public TargetScriptRunnerEntity(Region tworld)
            : base(tworld)
        {
        }

        public override void Trigger(Entity ent, Entity user)
        {
            CommandQueue queue;
            Dictionary<string, TemplateObject> vars = new Dictionary<string, TemplateObject>();
            vars.Add("entity", new TextTag(ent.EID));
            vars.Add("user", new TextTag(user.EID)); // TODO: Entity objects!
            TheServer.Commands.CommandSystem.ExecuteScript(scriptcommands, vars, out queue);
        }
        public CommandScript scriptcommands = null;

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "scriptcommands":
                    scriptcommands = CommandScript.SeparateCommands("SCRIPTRUNNERFORTARGET" + Targetname, data, TheServer.Commands.CommandSystem);
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("scriptcommands", scriptcommands.FullString()));
            return vars;
        }
    }
}
