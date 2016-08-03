using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime;
using System.Reflection;
using System.IO; // Permitted IO rules exception
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;

namespace Voxalia.ServerGame.PluginSystem
{
    public class PluginLoader
    {
        public string GetBaseDir()
        {
            return TheServer.Files.BaseDirectory + "/plugins/server/";
        }

        public Server TheServer;

        public PluginLoader(Server tserver)
        {
            TheServer = tserver;
        }

        public string[] GetPossiblePlugins()
        {
            string bdir = GetBaseDir();
            if (!Directory.Exists(bdir))
            {
                return new string[] { };
            }
            string[] tf = Directory.GetFiles(bdir, "*.dll", SearchOption.TopDirectoryOnly); // TODO: Replace with Program.Files method
            List<string> res = new List<string>(tf.Length);
            foreach (string file in tf)
            {
                string f = file.Replace('\\', '/');
                f = f.Substring(f.LastIndexOf('/') + 1);
                res.Add(f.Substring(0, f.Length - ".dll".Length));
            }
            return res.ToArray();
        }

        public Type PluginType = typeof(ServerPlugin);

        public ServerPlugin LoadPlugin(string name)
        {
            Assembly plugcode = AppDomain.CurrentDomain.Load(File.ReadAllBytes(GetBaseDir() + name + ".dll"), File.ReadAllBytes(GetBaseDir() + name + ".pdb"));
            Type[] types = plugcode.GetTypes();
            Type pluginbase = null;
            foreach (Type type in types)
            {
                Type[] interfaces = type.GetInterfaces();
                if (interfaces.Contains(PluginType))
                {
                    pluginbase = type;
                    break;
                }
            }
            if (pluginbase == null)
            {
                SysConsole.Output(OutputType.ERROR, "Invalid plugin '" + name + "': no ServerPlugin implementation class!");
                return null;
            }
            ServerPlugin pl = (ServerPlugin)Activator.CreateInstance(pluginbase);
            return pl;
        }
    }
}
