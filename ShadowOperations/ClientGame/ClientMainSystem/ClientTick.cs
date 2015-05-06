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

        public List<BaseJoint> Joints = new List<BaseJoint>();

        public void AddJoint(BaseJoint joint)
        {
            Joints.Add(joint);
            joint.Ent1.Joints.Add(joint);
            joint.Ent2.Joints.Add(joint);
            joint.CurrentJoint = joint.GetBaseJoint();
            PhysicsWorld.Add(joint.CurrentJoint);
        }

        public void DestroyJoint(BaseJoint joint)
        {
            Joints.Remove(joint);
            joint.Ent1.Joints.Remove(joint);
            joint.Ent2.Joints.Remove(joint);
            PhysicsWorld.Remove(joint.CurrentJoint);
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

        void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            Delta = e.Time;
            try
            {
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
