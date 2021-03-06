//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.JointSystem;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.EntitySystem
{
    /// <summary>
    /// Represents an object within the world.
    /// </summary>
    public abstract class Entity
    {
        public OpenTK.Graphics.Color4 WireColor;

        public Entity(Region tregion, bool tickme, bool cast_shadows)
        {
            TheRegion = tregion;
            TheClient = tregion.TheClient;
            Ticks = tickme;
            WireColor = new OpenTK.Graphics.Color4((float)Utilities.UtilRandom.NextDouble(), (float)Utilities.UtilRandom.NextDouble(), 0f, 1f);
            CastShadows = cast_shadows;
        }

        /// <summary>
        /// The unique ID for this entity.
        /// </summary>
        public long EID;

        /// <summary>
        /// Whether this entity should tick.
        /// </summary>
        public readonly bool Ticks;

        /// <summary>
        /// Wether this entity should cast shadows.
        /// </summary>
        public readonly bool CastShadows;

        /// <summary>
        /// The client that manages this entity.
        /// </summary>
        public Client TheClient = null;

        public Region TheRegion = null;

        /// <summary>
        /// Draw the entity in the 3D world.
        /// </summary>
        public abstract void Render();

        public bool Visible = false;

        public abstract BEPUutilities.Quaternion GetOrientation();

        public abstract void SetOrientation(BEPUutilities.Quaternion quat);

        public List<InternalBaseJoint> Joints = new List<InternalBaseJoint>();

        /// <summary>
        /// Tick the entity. Default implementation throws exception.
        /// </summary>
        public virtual void Tick()
        {
            throw new NotImplementedException();
        }

        public abstract Location GetPosition();

        public abstract void SetPosition(Location pos);
    }

    public abstract class EntityTypeConstructor
    {
        public abstract Entity Create(Region tregion, byte[] e);
    }
}
