using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ServerGame.EntitySystem
{
    public interface EntityTargettable
    {
        string GetTargetName();

        void Trigger(Entity ent, Entity user);
    }
}
