﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.ClientGame.UISystem;
using ShadowOperations.ClientGame.NetworkSystem.PacketsOut;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using BEPUphysics.EntityStateManagement;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using ShadowOperations.ClientGame.GraphicsSystems;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;

namespace ShadowOperations.ClientGame.EntitySystem
{
    class OtherPlayerEntity: PhysicsEntity
    {
        public Location HalfSize = new Location(0.3f, 0.3f, 1);

        public Location Direction = new Location(0, 0, 0);

        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;
        public bool Upward = false;
        public bool Downward = false;

        bool pup = false;

        public OtherPlayerEntity(Client tclient, Location half) :
            base (tclient, true)
        {
            HalfSize = half;
            SetMass(100);
            Shape = new Box(new BEPUutilities.Vector3(0, 0, 0), (float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            Shape.AngularDamping = 1;
            CanRotate = false;
            EID = -1;
        }

        public bool IgnoreThis(BroadPhaseEntry entry)
        {
            return ((EntityCollidable)entry).Entity.Tag != this;
        }

        public override void Tick()
        {
            while (Direction.X < 0)
            {
                Direction.X += 360;
            }
            while (Direction.X > 360)
            {
                Direction.X -= 360;
            }
            if (Direction.Y > 89.9f)
            {
                Direction.Y = 89.9f;
            }
            if (Direction.Y < -89.9f)
            {
                Direction.Y = -89.9f;
            }
            bool fly = false;
            bool on_ground = TheClient.Collision.CuboidLineIsSolid(new Location(0.2f, 0.2f, 0.1f), GetPosition(), GetPosition() - new Location(0, 0, 0.1f), IgnoreThis);
            if (Upward && !fly && !pup && on_ground)
            {
                Body.ApplyImpulse(new Vector3(0, 0, 0), (Location.UnitZ * 500f).ToBVector());
                Body.ActivityInformation.Activate();
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
            }
            Location movement = new Location(0, 0, 0);
            if (Leftward)
            {
                movement.Y = -1;
            }
            if (Rightward)
            {
                movement.Y = 1;
            }
            if (Backward)
            {
                movement.X = 1;
            }
            if (Forward)
            {
                movement.X = -1;
            }
            bool Slow = false;
            if (movement.LengthSquared() > 0)
            {
                movement = Utilities.RotateVector(movement, Direction.X * Utilities.PI180, fly ? Direction.Y * Utilities.PI180 : 0).Normalize();
            }
            Location intent_vel = movement * MoveSpeed * (Slow || Downward ? 0.5f : 1f);
            Location pvel = intent_vel - (fly ? Location.Zero : GetVelocity());
            if (pvel.LengthSquared() > 4 * MoveSpeed * MoveSpeed)
            {
                pvel = pvel.Normalize() * 2 * MoveSpeed;
            }
            pvel *= MoveSpeed * (Slow || Downward ? 0.5f : 1f);
            if (!fly && on_ground)
            {
                Body.ApplyImpulse(new Vector3(0, 0, 0), new Vector3((float)pvel.X, (float)pvel.Y, 0));
                Body.ActivityInformation.Activate();
            }
            if (fly)
            {
                SetPosition(GetPosition() + pvel / 200);
            }
        }

        public float MoveSpeed = 10;

        public override Location GetAngles()
        {
            return Direction;
        }

        public override void SetAngles(Location rot)
        {
            Direction = rot;
        }

        public override Location GetPosition()
        {
            return base.GetPosition() - new Location(0, 0, HalfSize.Z);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, HalfSize.Z));
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            Recalculate();
        }

        public List<VBO> VBOs = new List<VBO>();

        public void Recalculate()
        {
            for (int i = 0; i < VBOs.Count; i++)
            {
                VBOs[i].Destroy();
            }
            VBOs.Clear();
            GetVBOFor(TheClient.Textures.GetTexture("top")).AddSide(new Location(0, 0, 1), new TextureCoordinates());
            GetVBOFor(TheClient.Textures.GetTexture("top")).AddSide(new Location(0, 0, -1), new TextureCoordinates());
            GetVBOFor(TheClient.Textures.GetTexture("top")).AddSide(new Location(1, 0, 0), new TextureCoordinates());
            GetVBOFor(TheClient.Textures.GetTexture("top")).AddSide(new Location(-1, 0, 0), new TextureCoordinates());
            GetVBOFor(TheClient.Textures.GetTexture("top")).AddSide(new Location(0, 1, 0), new TextureCoordinates());
            GetVBOFor(TheClient.Textures.GetTexture("top")).AddSide(new Location(0, -1, 0), new TextureCoordinates());
            for (int i = 0; i < VBOs.Count; i++)
            {
                VBOs[i].GenerateVBO();
            }
        }

        VBO GetVBOFor(Texture tex)
        {
            for (int i = 0; i < VBOs.Count; i++)
            {
                if (VBOs[i].Tex.Original_InternalID == tex.Original_InternalID)
                {
                    return VBOs[i];
                }
            }
            VBO vbo = new VBO();
            vbo.Tex = tex;
            vbo.Prepare();
            VBOs.Add(vbo);
            return vbo;
        }

        public override void Render()
        {
            OpenTK.Matrix4 mat = OpenTK.Matrix4.CreateScale(HalfSize.ToOVector())
                * OpenTK.Matrix4.CreateRotationZ((float)(Direction.X * Utilities.PI180))
                * OpenTK.Matrix4.CreateTranslation(base.GetPosition().ToOVector());
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            for (int i = 0; i < VBOs.Count; i++)
            {
                VBOs[i].Render(true);
            }
        }
    }
}