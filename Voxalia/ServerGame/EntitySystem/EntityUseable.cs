using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public interface EntityUseable
    {
        bool Use(Entity user);
    }
}
