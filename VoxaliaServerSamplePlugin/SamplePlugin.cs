using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.PluginSystem;
using Voxalia.Shared;

namespace VoxaliaServerSamplePlugin
{
    public class SamplePlugin: ServerPlugin
    {
        public static string OutputType = "SamplePlugin";

        public bool Load(PluginManager manager)
        {
            SysConsole.OutputCustom(OutputType, "Hello world!");
            return true;
        }

        public void Unload()
        {
            SysConsole.OutputCustom(OutputType, "Goodbye!");
        }

        public string Name { get { return "SamplePlugin"; } }

        public string ShortDescription { get { return "A simple plugin that says hello and goodbye, to show plugins are made!"; } }

        public string HelpLink { get { return "http://github.com/Voxalia/Voxalia"; } }

        public string Version { get { return "1.0.0"; } }

        public string[] Authors { get { return new string[] { "mcmonkey" }; } }

        public string[] Dependencies { get { return new string[] { }; } }
    }
}
