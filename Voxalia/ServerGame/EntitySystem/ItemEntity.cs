using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    class ItemEntity: ModelEntity, EntityUseable
    {
        public ItemStack Stack;

        public ItemEntity(ItemStack stack, Region tworld)
            : base(stack.Model, tworld)
        {
            Stack = stack;
            SetMass(5 * stack.Count); // TODO: Weight property for items!
            CGroup = CollisionUtil.Item;
        }

        public bool Use(Entity user)
        {
            if (user is PlayerEntity)
            {
                ((PlayerEntity)user).Items.GiveItem(Stack);
                TheWorld.DespawnEntity(this);
                return true;
            }
            return false;
        }
    }
}
