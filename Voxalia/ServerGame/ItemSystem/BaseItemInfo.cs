//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.ItemSystem
{
    public abstract class BaseItemInfo
    {
        public string Name;

        // TODO: Entity -> LivingEntity? Or CharacterEntity?

        public abstract void PrepItem(Entity entity, ItemStack item);

        public abstract void Click(Entity entity, ItemStack item);

        public abstract void AltClick(Entity entity, ItemStack item);

        public abstract void ReleaseClick(Entity entity, ItemStack item);

        public abstract void ReleaseAltClick(Entity entity, ItemStack item);

        public abstract void Use(Entity entity, ItemStack item);

        public abstract void SwitchFrom(Entity entity, ItemStack item);

        public abstract void SwitchTo(Entity entity, ItemStack item);

        public abstract void Tick(Entity entity, ItemStack item);
    }
}
