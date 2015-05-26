using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ItemSystem;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.Events;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities;
using BEPUphysics.CollisionRuleManagement;
using BEPUutilities;

namespace ShadowOperations.ServerGame.EntitySystem
{
    class ItemEntity: ModelEntity, EntityUseable
    {
        public ItemStack Stack;

        public ItemEntity(ItemStack stack, Server tserver)
            : base(stack.Model, tserver)
        {
            Stack = stack;
            SetMass(5 * stack.Count); // TODO: Weight property for items!
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            Body.CollisionInformation.CollisionRules.Group = TheServer.Collision.Item; // TODO: Collision group getter/setter
        }

        public bool Use(Entity user)
        {
            if (user is PlayerEntity)
            {
                ((PlayerEntity)user).GiveItem(Stack);
                TheServer.DespawnEntity(this);
                return true;
            }
            return false;
        }
    }
}
