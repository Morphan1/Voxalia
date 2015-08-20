using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.OtherSystems
{
    public class ModelEngine
    {
        public Dictionary<string, Model> Models = new Dictionary<string, Model>();

        public ModelHandler handler = new ModelHandler();

        public Model GetModel(string name)
        {
            string nl = name.ToLower();
            Model temp;
            if (Models.TryGetValue(nl, out temp))
            {
                return temp;
            }
            temp = LoadModel(nl);
            if (temp != null)
            {
                return temp;
            }
            return LoadModel("cube");
        }

        Model LoadModel(string name)
        {
            string n = "models/" + name;
            try
            {
                if (Program.Files.Exists(n))
                {
                    byte[] dat = Program.Files.ReadBytes(n);
                    Model3D scene = handler.LoadModel(dat);
                    return new Model() { Name = name, Original = scene };
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, ex.ToString());
            }
            return null;
        }
    }

    public class Model
    {
        public string Name;
        public Model3D Original;
    }
}
