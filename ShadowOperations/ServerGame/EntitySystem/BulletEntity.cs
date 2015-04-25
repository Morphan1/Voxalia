using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public class BulletEntity: PrimitiveEntity
    {
        public BulletEntity(Server tserver)
            : base(tserver)
        {
        }

        public float Size = 1;
        public float Damage = 1;
        public float SplashSize = 0;
        public float SplashDamage = 0;
    }
}
