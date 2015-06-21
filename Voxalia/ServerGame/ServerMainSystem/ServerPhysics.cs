using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Settings;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.ServerMainSystem
{
    public partial class Server
    {
        /// <summary>
        /// The physics world in which all physics-related activity takes place.
        /// </summary>
        public Space PhysicsWorld;

        public CollisionUtil Collision;

        public Location GravityNormal = new Location(0, 0, -1);

        /// <summary>
        /// Builds the physics world.
        /// </summary>
        public void BuildWorld()
        {
            PhysicsWorld = new Space();
            // Set the world's general default gravity
            PhysicsWorld.ForceUpdater.Gravity = new Vector3(0, 0, -9.8f);
            // Minimize penetration
            CollisionDetectionSettings.AllowedPenetration = 0.001f;
            // Load a CollisionUtil instance
            Collision = new CollisionUtil(PhysicsWorld);
        }

        public List<World> LoadedWorlds = new List<World>();

        public void LoadWorld(string name)
        {
            // TODO: Actually load from file!
            World world = new World();
            world.Name = name.ToLower();
            world.TheServer = this;
            LoadedWorlds.Add(world);
        }

        /// <summary>
        /// Ticks the physics world.
        /// </summary>
        public void TickWorld(double delta)
        {
            PhysicsWorld.Update((float)delta); // TODO: More specific settings?
        }
    }
}
