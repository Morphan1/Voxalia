//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;
using LiteDB;

namespace Voxalia.ServerGame.EntitySystem
{
    public class GlowstickEntity: GrenadeEntity
    {
        public System.Drawing.Color Color;

        public GlowstickEntity(System.Drawing.Color col, Region tregion) :
            base(tregion)
        {
            Color = col;
        }

        public override EntityType GetEntityType()
        {
            return EntityType.GLOWSTICK;
        }

        public override BsonDocument GetSaveData()
        {
            BsonDocument doc = new BsonDocument();
            AddPhysicsData(doc);
            doc["gs_color"] = Color.ToArgb();
            return doc;
        }

        public override NetworkEntityType GetNetType()
        {
            return NetworkEntityType.GLOWSTICK;
        }

        public override byte[] GetNetData()
        {
            byte[] phys = GetPhysicsNetData();
            byte[] dat = new byte[phys.Length + 4];
            phys.CopyTo(dat, 0);
            Utilities.IntToBytes(Color.ToArgb()).CopyTo(dat, phys.Length);
            return dat;
        }
    }

    public class GlowstickEntityConstructor : EntityConstructor
    {
        public override Entity Create(Region tregion, BsonDocument doc)
        {
            GlowstickEntity glowstick = new GlowstickEntity(System.Drawing.Color.FromArgb(doc["gs_color"].AsInt32), tregion);
            glowstick.ApplyPhysicsData(doc);
            return glowstick;
        }
    }
}
