﻿using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Input;
using Voxalia.Shared;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.OtherSystems;
using System.Drawing;
using FreneticScript;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers.Common;
using System.Linq;
using Voxalia.Shared.Collision;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using Voxalia.ClientGame.NetworkSystem;
using Voxalia.ClientGame.EntitySystem;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public List<ItemStack> Items = new List<ItemStack>();

        public int QuickBarPos = 0;

        public Object TickLock = new Object();
        
        /// <summary>
        /// Returns an item in the quick bar.
        /// Can return air.
        /// </summary>
        /// <param name="slot">The slot, any number is permitted.</param>
        /// <returns>A valid item.</returns>
        public ItemStack GetItemForSlot(int slot)
        {
            while (slot < 0)
            {
                slot += Items.Count + 1;
            }
            while (slot > Items.Count)
            {
                slot -= Items.Count + 1;
            }
            if (slot == 0)
            {
                return new ItemStack(this, "Air")
                {
                    DrawColor = Color.White,
                    Tex = Textures.Clear,
                    Description = "An empty slot.",
                    Count = 0,
                    Datum = 0,
                    DisplayName = "Air",
                    Name = "air",
                    TheClient = this
                };
            }
            else
            {
                return Items[slot - 1];
            }
        }

        public double opsat = 0;

        public bool first = true;

        string HSaveStr = "// THIS FILE IS AUTOMATICALLY GENERATED.\n" + "// This file is run very early in startup, be careful with it!\n" + "debug minimal;\n";

        string CSaveStr = null;

        string BSaveStr = null;

        public void OncePerSecondActions()
        {
            gFPS = gTicks;
            gTicks = 0;
            bool edited = false;
            if (CVars.system.Modified || first)
            {
                edited = true;
                CVars.system.Modified = false;
                StringBuilder cvarsave = new StringBuilder(CVars.system.CVarList.Count * 100);
                for (int i = 0; i < CVars.system.CVarList.Count; i++)
                {
                    if (!CVars.system.CVarList[i].Flags.HasFlag(CVarFlag.ServerControl)
                        && !CVars.system.CVarList[i].Flags.HasFlag(CVarFlag.ReadOnly)
                        && !CVars.system.CVarList[i].Flags.HasFlag(CVarFlag.DoNotSave))
                    {
                        string val = CVars.system.CVarList[i].Value;
                        if (val.Contains('\"'))
                        {
                            val = "<{unescape[" + EscapeTagBase.Escape(val) + "]}>";
                        }
                        cvarsave.Append("set \"" + CVars.system.CVarList[i].Name + "\" \"" + val + "\";\n");
                    }
                }
                lock (saveLock)
                {
                    CSaveStr = cvarsave.ToString();
                }
            }
            if (KeyHandler.Modified || first)
            {
                edited = true;
                KeyHandler.Modified = false;
                first = false;
                StringBuilder keybindsave = new StringBuilder(KeyHandler.keystonames.Count * 100);
                keybindsave.Append("wait 0.5;\n");
                foreach (KeyValuePair<string, Key> keydat in KeyHandler.namestokeys)
                {
                    CommandScript cs = KeyHandler.GetBind(keydat.Value);
                    if (cs == null)
                    {
                        keybindsave.Append("unbind \"" + keydat.Key + "\";\n");
                    }
                    else
                    {
                        keybindsave.Append("bindblock \"" + keydat.Key + "\"\n{\n" + cs.FullString("\t") + "}\n");
                    }
                }
                lock (saveLock)
                {
                    BSaveStr = keybindsave.ToString();
                }
            }
            if (edited)
            {
                Schedule.StartASyncTask(SaveCFG);
            }
            MainWorldView.ShadowSpikeTime = 0;
            TickSpikeTime = 0;
            MainWorldView.FBOSpikeTime = 0;
            MainWorldView.LightsSpikeTime = 0;
            FinishSpikeTime = 0;
            TWODSpikeTime = 0;
            for (int i = 0; i < (int)NetUsageType.COUNT; i++)
            {
                Network.UsagesLastSecond[i] = Network.UsagesThisSecond[i];
                Network.UsagesThisSecond[i] = 0;
            }
    }

        Object saveLock = new Object();

        public void SaveCFG()
        {
            try
            {
                lock (saveLock)
                {
                    Files.WriteText("clientdefaultsettings.cfg", HSaveStr + CSaveStr + BSaveStr);
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Saving settings: " + ex.ToString());
            }
        }

        public double GlobalTickTimeLocal = 0;

        public double Delta;

        public float GamePadVibration = 0f;
        
        void tick (double delt)
        {
            lock (TickLock)
            {
                Delta = delt * CVars.g_timescale.ValueD;
                GlobalTickTimeLocal += Delta;
                try
                {
                    opsat += Delta;
                    if (opsat >= 1)
                    {
                        opsat -= 1;
                        OncePerSecondActions();
                    }
                    Schedule.RunAllSyncTasks(Delta);
                    Textures.Update(GlobalTickTimeLocal);
                    TBlock.Tick(Delta);
                    Shaders.Update(GlobalTickTimeLocal);
                    Models.Update(GlobalTickTimeLocal);
                    KeyHandler.Tick();
                    if (RawGamePad != null)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (GamePad.GetCapabilities(i).IsConnected)
                            {
                                RawGamePad.SetVibration(i, GamePadVibration, GamePadVibration);
                            }
                        }
                    }
                    GamePadHandler.Tick(Delta);
                    MouseHandler.Tick();
                    UIConsole.Tick();
                    Commands.Tick();
                    TickWorld(Delta);
                    TickInvMenu();
                    Sounds.Update(MainWorldView.CameraPos, MainWorldView.CameraTarget - MainWorldView.CameraPos, MainWorldView.CameraUp, Player.GetVelocity(), Window.Focused);
                    CScreen.Tick();
                    Schedule.RunAllSyncTasks(0);
                    Player.PostTick();
                    TheRegion.SolveJoints();
                    //ProcessChunks();
                }
                catch (Exception ex)
                {
                    SysConsole.Output(OutputType.ERROR, "Ticking: " + ex.ToString());
                }
                PlayerEyePosition = Player.GetCameraPosition();
                RayCastResult rcr;
                Location forw = Player.ForwardVector();
                bool h = TheRegion.SpecialCaseRayTrace(PlayerEyePosition, forw, 100, MaterialSolidity.ANY, IgnorePlayer, out rcr);
                CameraFinalTarget = h ? new Location(rcr.HitData.Location) - new Location(rcr.HitData.Normal).Normalize() * 0.01: PlayerEyePosition + forw * 100;
                CameraImpactNormal = h ? new Location(rcr.HitData.Normal).Normalize() : Location.Zero;
                CameraDistance = h ? rcr.HitData.T: 100;
            }
            double cping = Math.Max(LastPingValue, GlobalTickTimeLocal - LastPingTime);
            AveragePings.Push(new KeyValuePair<double, double>(GlobalTickTimeLocal, cping));
            while ((GlobalTickTimeLocal - AveragePings.Peek().Key) > 1)
            {
                AveragePings.Pop();
            }
            APing = 0;
            for (int i = 0; i < AveragePings.Length; i++)
            {
                APing += AveragePings[i].Value;
            }
            APing /= (double)AveragePings.Length;
        }

        public Location PlayerEyePosition;

        public Location CameraImpactNormal;

        bool IgnorePlayer(BEPUphysics.BroadPhaseEntries.BroadPhaseEntry entry)
        {
            if (entry is EntityCollidable)
            {
                Entity e = (Entity)((EntityCollidable)entry).Entity.Tag;
                if (e == Player || e == Player.Vehicle)
                {
                    return false;
                }
            }
            return TheRegion.Collision.ShouldCollide(entry);
        }

        public Location CameraFinalTarget;

        public double CameraDistance;

        public void Resetregion()
        {
            Items.Clear();
            UpdateInventoryMenu();
            QuickBarPos = 0;
            BuildWorld();
        }

        public double LastPingTime = 0;

        public double LastPingValue = 0;

        public ListQueue<KeyValuePair<double, double>> AveragePings = new ListQueue<KeyValuePair<double, double>>();

        public double APing = 0;
    }
}
