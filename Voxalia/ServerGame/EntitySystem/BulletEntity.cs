//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.Shared;
using BEPUutilities;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.NetworkSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using LiteDB;

namespace Voxalia.ServerGame.EntitySystem
{
    public class BulletEntity: PrimitiveEntity
    {
        public BulletEntity(Region tregion)
            : base(tregion)
        {
            Collide += new EventHandler<CollisionEventArgs>(OnCollide);
            Gravity = new Location(TheRegion.PhysicsWorld.ForceUpdater.Gravity);
        }

        public override string GetModel()
        {
            return "invisbox";
        }

        public override NetworkEntityType GetNetType()
        {
            return NetworkEntityType.BULLET;
        }

        public override byte[] GetNetData()
        {
            byte[] res = new byte[4 + 24 + 24];
            Utilities.FloatToBytes((float)Size).CopyTo(res, 0);
            GetPosition().ToDoubleBytes().CopyTo(res, 4);
            GetVelocity().ToDoubleBytes().CopyTo(res, 4 + 24);
            return res;
        }

        public override EntityType GetEntityType()
        {
            return EntityType.BULLET;
        }

        public override BsonDocument GetSaveData()
        {
            // Does not save.
            return null;
        }
        
        public void OnCollide(object sender, CollisionEventArgs args)
        {
            if (args.Info.HitEnt != null)
            {
                PhysicsEntity physent = ((PhysicsEntity)args.Info.HitEnt.Tag);
                Vector3 loc = (GetPosition() - physent.GetPosition()).ToBVector();
                Vector3 impulse = GetVelocity().ToBVector() * Damage / 1000f;
                physent.Body.ApplyImpulse(ref loc, ref impulse);
                physent.Body.ActivityInformation.Activate();
                if (physent is EntityDamageable)
                {
                    ((EntityDamageable)physent).Damage(Damage);
                }
            }
            if (SplashSize > 0 && SplashDamage > 0)
            {
                // TODO: Apply Splash Damage
                // TODO: Apply Splash Impulses
            }
            RemoveMe();
        }

        public double Size = 1;
        public double Damage = 1;
        public double SplashSize = 0;
        public double SplashDamage = 0;
    }
}
