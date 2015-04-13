using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.EntitySystem
{
    public class CubeEntity: Entity
    {
        /// <summary>
        /// Half the size of the cuboid.
        /// </summary>
        public Location HalfSize = new Location(1, 1, 1);

        public override void Render()
        {
        }
    }
}
