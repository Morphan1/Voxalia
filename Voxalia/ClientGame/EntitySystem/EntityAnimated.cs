using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ClientGame.EntitySystem
{
    public interface EntityAnimated
    {
        void SetAnimation(string anim, byte mode);
    }
}
