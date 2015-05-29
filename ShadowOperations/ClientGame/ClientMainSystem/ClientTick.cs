﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.UISystem;
using ShadowOperations.ClientGame.EntitySystem;
using ShadowOperations.ClientGame.OtherSystems;
using System.Drawing;
using ShadowOperations.ClientGame.JointSystem;
using System.Threading;
using Frenetic;
using Frenetic.TagHandlers.Common;

namespace ShadowOperations.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public double Delta;

        public List<Entity> Entities = new List<Entity>();

        public List<Entity> Tickers = new List<Entity>();

        public List<Entity> ShadowCasters = new List<Entity>();

        public List<ItemStack> Items = new List<ItemStack>();

        public int QuickBarPos = 0;

        public List<InternalBaseJoint> Joints = new List<InternalBaseJoint>();

        public void AddJoint(InternalBaseJoint joint)
        {
            Joints.Add(joint);
            joint.One.Joints.Add(joint);
            joint.Two.Joints.Add(joint);
            if (joint is BaseJoint)
            {
                BaseJoint pjoint = (BaseJoint)joint;
                pjoint.CurrentJoint = pjoint.GetBaseJoint();
                PhysicsWorld.Add(pjoint.CurrentJoint);
            }
        }

        public void DestroyJoint(InternalBaseJoint joint)
        {
            Joints.Remove(joint);
            joint.One.Joints.Remove(joint);
            joint.Two.Joints.Remove(joint);
            if (joint is BaseJoint)
            {
                BaseJoint pjoint = (BaseJoint)joint;
                PhysicsWorld.Remove(pjoint.CurrentJoint);
            }
        }

        /// <summary>
        /// Returns an item in the quick bar.
        /// Can return air.
        /// </summary>
        /// <param name="slot">The slot, any number is permitted</param>
        /// <returns>A valid item</returns>
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
                    DrawColor = Color.White.ToArgb(),
                    Tex = Textures.Clear,
                    Description = "An empty slot.",
                    Count = 0,
                    Datum = 0,
                    DisplayName = "Air"
                };
            }
            else
            {
                return Items[slot - 1];
            }
        }

        public double opsat = 0;

        string SaveStr = null;

        public void OncePerSecondActions()
        {
            gFPS = gTicks;
            gTicks = 0;
            if (CVars.system.Modified)
            {
                CVars.system.Modified = false;
                StringBuilder cvarsave = new StringBuilder(CVars.system.CVarList.Count * 100);
                cvarsave.Append("// THIS FILE IS AUTOMATICALLY GENERATED.\n");
                cvarsave.Append("// This file is run very early in startup, be careful with it!\n");
                cvarsave.Append("debug minimal;\n");
                for (int i = 0; i < CVars.system.CVarList.Count; i++)
                {
                    if (!CVars.system.CVarList[i].Flags.HasFlag(CVarFlag.ServerControl)
                        && !CVars.system.CVarList[i].Flags.HasFlag(CVarFlag.ReadOnly))
                    {
                        string val = CVars.system.CVarList[i].Value;
                        string val_esc = EscapeTags.Escape(val);
                        if (val_esc != val)
                        {
                            val = "<{unescape[" + val_esc + "]}>";
                        }
                        cvarsave.Append("set \"" + CVars.system.CVarList[i].Name + "\" \"" + val + "\";\n");
                    }
                }
                SaveStr = cvarsave.ToString();
                Thread thread = new Thread(new ThreadStart(SaveCFG));
                thread.Start();
            }
        }

        public void SaveCFG()
        {
            try
            {
                Program.Files.WriteText("clientdefaultsettings.cfg", SaveStr);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Saving settings: " + ex.ToString());
            }
        }

        public double GlobalTickTimeLocal = 0;

        void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            Delta = e.Time;
            GlobalTickTimeLocal += Delta;
            try
            {
                opsat += Delta;
                if (opsat >= 1)
                {
                    opsat -= 1;
                    OncePerSecondActions();
                }
                Textures.Update(GlobalTickTimeLocal);
                Shaders.Update(GlobalTickTimeLocal);
                Models.Update(GlobalTickTimeLocal);
                KeyHandler.Tick();
                MouseHandler.Tick();
                UIConsole.Tick();
                Commands.Tick();
                Network.Tick();
                TickWorld(Delta);
                for (int i = 0; i < Tickers.Count; i++)
                {
                    Tickers[i].Tick();
                }
                for (int i = 0; i < Joints.Count; i++)
                {
                    if (Joints[i].Enabled && Joints[i] is BaseFJoint)
                    {
                        ((BaseFJoint)Joints[i]).Solve();
                    }
                }
                Sounds.Update(CameraPos, CameraTarget - CameraPos, CameraUp, Player.GetVelocity(), Window.Focused);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Ticking: " + ex.ToString());
            }
        }

        /// <summary>
        /// Spawns an entity in the world.
        /// </summary>
        /// <param name="e">The entity to spawn</param>
        public void SpawnEntity(Entity e)
        {
            Entities.Add(e);
            if (e.Ticks)
            {
                Tickers.Add(e);
            }
            if (e.CastShadows)
            {
                ShadowCasters.Add(e);
            }
            if (e is PhysicsEntity)
            {
                ((PhysicsEntity)e).SpawnBody();
            }
            else if (e is PrimitiveEntity)
            {
                ((PrimitiveEntity)e).Spawn();
            }
        }

        public void Despawn(Entity e)
        {
            Entities.Remove(e);
            if (e.Ticks)
            {
                Tickers.Remove(e);
            }
            if (e.CastShadows)
            {
                ShadowCasters.Remove(e);
            }
            if (e is PhysicsEntity)
            {
                ((PhysicsEntity)e).DestroyBody();
            }
            else if (e is PrimitiveEntity)
            {
                ((PrimitiveEntity)e).Destroy();
            }
        }

        public Entity GetEntity(long EID)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].EID == EID)
                {
                    return Entities[i];
                }
            }
            return null;
        }

        public void ResetWorld()
        {
            Items.Clear();
            QuickBarPos = 0;
            for (int i = 0; i < Entities.Count; i++)
            {
                if (!(Entities[i] is PlayerEntity))
                {
                    Despawn(Entities[i]);
                    i--;
                }
            }
        }
    }
}
