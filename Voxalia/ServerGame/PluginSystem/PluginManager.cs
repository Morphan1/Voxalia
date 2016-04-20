using System;
using System.Text;
using System.Collections.Generic;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;
using FreneticScript;

namespace Voxalia.ServerGame.PluginSystem
{
    public class PluginManager
    {
        public PluginLoader Loader;

        public Server TheServer;

        public List<ServerPlugin> Plugins;

        public PluginManager(Server tserver)
        {
            TheServer = tserver;
            Loader = new PluginLoader();
            Plugins = new List<ServerPlugin>();
        }

        public void Init()
        {
            LoadPlugins();
        }

        public void LoadPlugins()
        {
            if (Plugins.Count > 0)
            {
                UnloadPlugins();
            }
            string[] poss = Loader.GetPossiblePlugins();
            for (int i = 0; i < poss.Length; i++)
            {
                ServerPlugin pl = Loader.LoadPlugin(poss[i]);
                if (pl != null)
                {
                    bool valid = true;
                    string[] depends = pl.Dependencies;
                    foreach (string depend in depends)
                    {
                        string dep = depend.ToLowerFast();
                        bool has = false;
                        foreach (string possib in poss)
                        {
                            if (possib == dep)
                            {
                                has = true;
                                break;
                            }
                        }
                        if (!has)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid)
                    {
                        StringBuilder sb = new StringBuilder(depends.Length * 50);
                        for (int d = 0; d < depends.Length; d++)
                        {
                            sb.Append(depends[d]);
                            if (d + 1 < depends.Length)
                            {
                                sb.Append(", ");
                            }
                        }
                        SysConsole.Output(OutputType.ERROR, "Plugin has unmet dependencies: '" + Plugins[i].Name + "'... requires: " + sb.ToString());
                    }
                    else
                    {
                        // TODO: What if a plugin has a dependency, that has a dependency which isn't loaded -> must chain-kill plugins!
                        Plugins.Add(pl);
                    }
                }
            }
            for (int i = 0; i < Plugins.Count; i++)
            {
                try
                {
                    SysConsole.Output(OutputType.INIT, "Loading plugin: " + Plugins[i].Name + " version " + Plugins[i].Version);
                    Plugins[i].Load(this);
                }
                catch (Exception ex)
                {
                    SysConsole.Output("Loading plugin '" + Plugins[i].Name + "'", ex);
                }
            }
            SysConsole.Output(OutputType.INIT, "Plugins loaded!");
        }

        public void UnloadPlugins()
        {
            for (int i = 0; i < Plugins.Count; i++)
            {
                try
                {
                    Plugins[i].Unload();
                }
                catch (Exception ex)
                {
                    Utilities.CheckException(ex);
                    SysConsole.Output("Unloading plugin '" + Plugins[i].Name + "'", ex);
                }
            }
            Plugins.Clear();
            SysConsole.Output(OutputType.INFO, "Plugins unloaded!");
        }
    }
}
