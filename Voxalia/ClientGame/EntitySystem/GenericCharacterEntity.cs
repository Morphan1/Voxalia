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
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.Shared.Files;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.EntitySystem
{
    public class GenericCharacterEntity : CharacterEntity
    {
        public Model model;

        public System.Drawing.Color color;

        public GenericCharacterEntity(Region tregion)
            : base(tregion)
        {
        }

        // TODO: Proper tracked motion?

        public override void SpawnBody()
        {
            base.SpawnBody();
            model.LoadSkin(TheClient.Textures);
        }

        public Matrix4d PreRot = Matrix4d.Identity;

        public override void Render()
        {
            TheClient.SetEnts();
            TheClient.Rendering.SetColor(TheClient.Rendering.AdaptColor(ClientUtilities.Convert(GetPosition()), color));
            TheClient.Rendering.SetMinimumLight(0.0f);
            // TODO: Prevent model flipping (Possibly related to animation?)
            Matrix4d mat = PreRot * Matrix4d.CreateRotationZ((Direction.Yaw * Utilities.PI180)) * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(GetPosition()));
            TheClient.MainWorldView.SetMatrix(2, mat);
            model.CustomAnimationAdjustments = new Dictionary<string, Matrix4>(SavedAdjustmentsOTK);
            model.Draw(aHTime, hAnim, aTTime, tAnim, aLTime, lAnim);
            TheClient.Rendering.SetColor(Color4.White);
            // TODO: Render held item!
            if (IsTyping)
            {
                TheClient.Textures.GetTexture("ui/game/typing").Bind(); // TODO: store!
                TheClient.Rendering.RenderBillboard(GetPosition() + new Location(0, 0, 4), new Location(2), TheClient.MainWorldView.CameraPos);
            }
        }
    }

    public class CharacterEntityConstructor : EntityTypeConstructor
    {
        public override Entity Create(Region tregion, byte[] data)
        {
            DataStream ds = new DataStream(data);
            DataReader dr = new DataReader(ds);
            GenericCharacterEntity ent = new GenericCharacterEntity(tregion);
            ent.SetPosition(Location.FromDoubleBytes(dr.ReadBytes(24), 0));
            ent.SetOrientation(new BEPUutilities.Quaternion(dr.ReadFloat(), dr.ReadFloat(), dr.ReadFloat(), dr.ReadFloat()));
            ent.SetMass(dr.ReadFloat());
            ent.CBAirForce = dr.ReadFloat();
            ent.CBAirSpeed = dr.ReadFloat();
            ent.CBCrouchSpeed = dr.ReadFloat();
            ent.CBDownStepHeight = dr.ReadFloat();
            ent.CBGlueForce = dr.ReadFloat();
            ent.CBHHeight = dr.ReadFloat();
            ent.CBJumpSpeed = dr.ReadFloat();
            ent.CBMargin = dr.ReadFloat();
            ent.CBMaxSupportSlope = dr.ReadFloat();
            ent.CBMaxTractionSlope = dr.ReadFloat();
            ent.CBProneSpeed = dr.ReadFloat();
            ent.CBRadius = dr.ReadFloat();
            ent.CBSlideForce = dr.ReadFloat();
            ent.CBSlideJumpSpeed = dr.ReadFloat();
            ent.CBSlideSpeed = dr.ReadFloat();
            ent.CBStandSpeed = dr.ReadFloat();
            ent.CBStepHeight = dr.ReadFloat();
            ent.CBTractionForce = dr.ReadFloat();
            ent.PreRot *= Matrix4d.CreateRotationX(dr.ReadFloat() * Utilities.PI180);
            ent.PreRot *= Matrix4d.CreateRotationY(dr.ReadFloat() * Utilities.PI180);
            ent.PreRot *= Matrix4d.CreateRotationZ(dr.ReadFloat() * Utilities.PI180);
            ent.mod_scale = dr.ReadFloat();
            ent.PreRot = Matrix4d.Scale(ent.mod_scale) * ent.PreRot;
            ent.color = System.Drawing.Color.FromArgb(dr.ReadInt());
            byte dtx = dr.ReadByte();
            ent.Visible = (dtx & 1) == 1;
            int solidity = (dtx & (2 | 4 | 8));
            if (solidity == 2)
            {
                ent.CGroup = CollisionUtil.Solid;
            }
            else if (solidity == 4)
            {
                ent.CGroup = CollisionUtil.NonSolid;
            }
            else if (solidity == (2 | 4))
            {
                ent.CGroup = CollisionUtil.Item;
            }
            else if (solidity == 8)
            {
                ent.CGroup = CollisionUtil.Player;
            }
            else if (solidity == (2 | 8))
            {
                ent.CGroup = CollisionUtil.Water;
            }
            else if (solidity == (2 | 4 | 8))
            {
                ent.CGroup = CollisionUtil.WorldSolid;
            }
            else if (solidity == 16)
            {
                ent.CGroup = CollisionUtil.Character;
            }
            ent.model = tregion.TheClient.Models.GetModel(tregion.TheClient.Network.Strings.StringForIndex(dr.ReadInt()));
            dr.Close();
            return ent;
        }
    }
}
