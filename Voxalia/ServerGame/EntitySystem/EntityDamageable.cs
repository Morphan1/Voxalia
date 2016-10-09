//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

namespace Voxalia.ServerGame.EntitySystem
{
    public interface EntityDamageable
    {
        double GetHealth();

        double GetMaxHealth();

        void SetHealth(double health);

        void SetMaxHealth(double health);

        void Damage(double amount);
    }
}
