using System;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.EntitySystem
{
    class ItemEntity: ModelEntity, EntityUseable
    {
        public ItemStack Stack;

        public ItemEntity(ItemStack stack, Region tregion)
            : base(stack.Model, tregion)
        {
            Stack = stack;
            SetMass(Math.Max(1f, stack.Weight) * stack.Count);
            CGroup = CollisionUtil.Item;
        }

        public override EntityType GetEntityType()
        {
            return EntityType.ITEM;
        }

        public override byte[] GetSaveBytes()
        {
            byte[] bbytes = GetPhysicsBytes();
            byte[] item = Stack.ServerBytes();
            byte[] res = new byte[bbytes.Length + 4 + item.Length];
            bbytes.CopyTo(res, 0);
            Utilities.IntToBytes(item.Length).CopyTo(res, bbytes.Length);
            item.CopyTo(res, bbytes.Length + 4);
            return res;
        }

        public void StartUse(Entity user)
        {
            if (!Removed)
            {
                if (user is PlayerEntity)
                {
                    ((PlayerEntity)user).Items.GiveItem(Stack);
                    RemoveMe();
                    return;
                }
            }
        }

        public void StopUse(Entity user)
        {
            // Do nothing.
        }
    }

    public class ItemEntityConstructor : EntityConstructor
    {
        public override Entity Create(Region tregion, byte[] input)
        {
            int plen = PhysicsEntity.PhysByteLen;
            int stacklen = Utilities.BytesToInt(Utilities.BytesPartial(input, plen, 4));
            ItemStack stack = new ItemStack(Utilities.BytesPartial(input, plen + 4, stacklen), tregion.TheServer);
            ItemEntity ent = new ItemEntity(stack, tregion);
            ent.ApplyBytes(input);
            return ent;
        }
    }
}
