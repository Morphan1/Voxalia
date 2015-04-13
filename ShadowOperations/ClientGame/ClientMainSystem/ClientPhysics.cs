using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.EntitySystem;

namespace ShadowOperations.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        /// <summary>
        /// The physics world in which all physics-related activity takes place.
        /// </summary>
        public DiscreteDynamicsWorld PhysicsWorld;

        /// <summary>
        /// Builds the physics world.
        /// </summary>
        public void BuildWorld()
        {
            // Choose which broadphase to use - Dbvt = ?
            BroadphaseInterface broadphase = new DbvtBroadphase();
            // Choose collision configuration - default = ?
            DefaultCollisionConfiguration collision_configuration = new DefaultCollisionConfiguration();
            // Set the dispatcher
            CollisionDispatcher dispatcher = new CollisionDispatcher(collision_configuration);
            // Register the dispatcher
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            // Choose solver - SquentialImpulseConstract = ?
            SequentialImpulseConstraintSolver solver = new SequentialImpulseConstraintSolver();
            // Create the world for physics to happen in
            PhysicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collision_configuration);
            // Set the world's general default gravity
            PhysicsWorld.Gravity = new Vector3(0, 0, -9.8f);
        }

        /// <summary>
        /// Ticks the physics world.
        /// </summary>
        public void TickWorld(double delta)
        {
            PhysicsWorld.StepSimulation((float)delta); // TODO: More specific settings?
        }
    }
}
