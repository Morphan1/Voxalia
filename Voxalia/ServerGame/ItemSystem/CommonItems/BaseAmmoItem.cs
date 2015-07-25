using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public abstract class BaseAmmoItem : BaseItemInfo
    {
        public BaseAmmoItem(string name)
        {
            Name = name;
        }

        public override void PrepItem(Entity player, ItemStack item)
        {
        }

        public override void Click(Entity player, ItemStack item)
        {
        }

        public override void AltClick(Entity player, ItemStack item)
        {
        }

        public override void ReleaseClick(Entity player, ItemStack item)
        {
        }

        public override void ReleaseAltClick(Entity player, ItemStack item)
        {
        }

        public override void Use(Entity player, ItemStack item)
        {
        }

        public override void SwitchFrom(Entity player, ItemStack item)
        {
        }

        public override void SwitchTo(Entity player, ItemStack item)
        {
        }

        public override void Tick(Entity player, ItemStack item)
        {
        }
    }
}
