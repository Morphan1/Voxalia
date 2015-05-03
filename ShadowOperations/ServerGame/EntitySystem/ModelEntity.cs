using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ServerGame.EntitySystem
{
    class ModelEntity: PhysicsEntity
    {
        public string model;

        public Location scale = Location.One;

        public ModelEntity(string mod, Server tserver)
            : base(tserver, true)
        {
            model = mod;
        }

        bool pActive = false;

        public override void Tick()
        {
            if (Body.ActivityInformation.IsActive || (pActive && !Body.ActivityInformation.IsActive))
            {
                pActive = Body.ActivityInformation.IsActive;
                PhysicsEntityUpdatePacketOut peupo = new PhysicsEntityUpdatePacketOut(this);
                for (int i = 0; i < TheServer.Players.Count; i++)
                {
                    TheServer.Players[i].Network.SendPacket(peupo);
                }
            }
        }

        public override bool ApplyVar(string var, string value)
        {
            switch (var)
            {
                case "model":
                    model = value;
                    return true;
                case "scale":
                    scale = Location.FromString(value);
                    return true;
                default:
                    return base.ApplyVar(var, value);
            }
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("model", model));
            vars.Add(new KeyValuePair<string, string>("scale", scale.ToString()));
            return vars;
        }

        public override void SpawnBody()
        {
            Shape = TheServer.Models.MeshToBepu(TheServer.Models.LoadModel(Program.Files.ReadBytes("models/" + model), model.Substring(model.IndexOf('.') + 1)));
            base.SpawnBody();
        }
    }
}
