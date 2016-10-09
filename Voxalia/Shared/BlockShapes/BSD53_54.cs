//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using BEPUutilities;

namespace Voxalia.Shared.BlockShapes
{
    public class BSD53_54 : BlockShapeDetails
    {
        public double LowerCoordinate;
        public double UpperCoordinate;
        public double ZTop;

        public BSD53_54(double low, double high, double zhigh)
        {
            LowerCoordinate = low;
            UpperCoordinate = high;
            ZTop = zhigh;
            BackTextureAllowed = false;
        }
        
        public override List<Vector3> GetVertices(Vector3 pos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> Vertices = new List<Vector3>();
            // Sprite 1: 0,0 -> 1,1, side one
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + LowerCoordinate, pos.Z));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + UpperCoordinate, pos.Z));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + UpperCoordinate, pos.Z));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop));
            // Sprite 2: 0,0 -> 1,1, side two
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + UpperCoordinate, pos.Z));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + UpperCoordinate, pos.Z));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + LowerCoordinate, pos.Z));
            // Sprite 3: 0,1 -> 1,0, side one
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + UpperCoordinate, pos.Z));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + LowerCoordinate, pos.Z));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + LowerCoordinate, pos.Z));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop));
            // Sprite 4: 0,1 -> 1,0, side two
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + LowerCoordinate, pos.Z));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + LowerCoordinate, pos.Z));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + UpperCoordinate, pos.Z));
            // Sprite 5: 0,0,0.5 -> 1,1,0.5 side one
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop * 0.5f));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop * 0.5f));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop * 0.5f));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop * 0.5f));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop * 0.5f));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop * 0.5f));
            // Sprite 6: 0,0,0.5 -> 1,1,0.5 side two
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop * 0.5f));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop * 0.5f));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop * 0.5f));
            Vertices.Add(new Vector3(pos.X + UpperCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop * 0.5f));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + UpperCoordinate, pos.Z + ZTop * 0.5f));
            Vertices.Add(new Vector3(pos.X + LowerCoordinate, pos.Y + LowerCoordinate, pos.Z + ZTop * 0.5f));
            return Vertices;
        }

        public override List<Vector3> GetNormals(Vector3 blockPos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> Norms = new List<Vector3>();
            // Sprite 1
            for (int i = 0; i < 6; i++)
            {
                Norms.Add(new Vector3(-0.7071f, 0.7071f, 0));
            }
            // Sprite 2
            for (int i = 0; i < 6; i++)
            {
                Norms.Add(new Vector3(0.7071f, -0.7071f, 0));
            }
            // Sprite 3
            for (int i = 0; i < 6; i++)
            {
                Norms.Add(new Vector3(0.7071f, 0.7071f, 0));
            }
            // Sprite 4
            for (int i = 0; i < 6; i++)
            {
                Norms.Add(new Vector3(-0.7071f, -0.7071f, 0));
            }
            // Sprite 5
            for (int i = 0; i < 6; i++)
            {
                Norms.Add(new Vector3(0f, 0f, 1f));
            }
            // Sprite 6
            for (int i = 0; i < 6; i++)
            {
                Norms.Add(new Vector3(0f, 0f, -1f));
            }
            return Norms;
        }

        public override List<Vector3> GetTCoords(Vector3 blockPos, Material mat, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> TCoords = new List<Vector3>();
            int tID_XP = mat.TextureID(MaterialSide.XP);
            int tID_XM = mat.TextureID(MaterialSide.XM);
            int tID_YP = mat.TextureID(MaterialSide.YP);
            int tID_YM = mat.TextureID(MaterialSide.YM);
            int tID_ZP = mat.TextureID(MaterialSide.TOP);
            //int tID_ZM = mat.TextureID(MaterialSide.BOTTOM);
            // Sprite 1: 0,0 -> 1,1, side one
            TCoords.Add(new Vector3(0, 1, tID_XM));
            TCoords.Add(new Vector3(1, 1, tID_XM));
            TCoords.Add(new Vector3(0, 0, tID_XM));
            TCoords.Add(new Vector3(1, 1, tID_XM));
            TCoords.Add(new Vector3(1, 0, tID_XM));
            TCoords.Add(new Vector3(0, 0, tID_XM));
            // Sprite 2: 0,0 -> 1,1, side two
            TCoords.Add(new Vector3(0, 0, tID_XP));
            TCoords.Add(new Vector3(1, 0, tID_XP));
            TCoords.Add(new Vector3(1, 1, tID_XP));
            TCoords.Add(new Vector3(0, 0, tID_XP));
            TCoords.Add(new Vector3(1, 1, tID_XP));
            TCoords.Add(new Vector3(0, 1, tID_XP));
            // Sprite 3: 0,1 -> 1,0, side one
            TCoords.Add(new Vector3(0, 1, tID_YP));
            TCoords.Add(new Vector3(1, 1, tID_YP));
            TCoords.Add(new Vector3(0, 0, tID_YP));
            TCoords.Add(new Vector3(1, 1, tID_YP));
            TCoords.Add(new Vector3(1, 0, tID_YP));
            TCoords.Add(new Vector3(0, 0, tID_YP));
            // Sprite 4: 0,1 -> 1,0, side two
            TCoords.Add(new Vector3(0, 0, tID_YM));
            TCoords.Add(new Vector3(1, 0, tID_YM));
            TCoords.Add(new Vector3(1, 1, tID_YM));
            TCoords.Add(new Vector3(0, 0, tID_YM));
            TCoords.Add(new Vector3(1, 1, tID_YM));
            TCoords.Add(new Vector3(0, 1, tID_YM));
            // Sprite 5: 0,0,0.5 -> 1,1,0.5 side one
            TCoords.Add(new Vector3(0, 0, tID_ZP));
            TCoords.Add(new Vector3(0, 1, tID_ZP));
            TCoords.Add(new Vector3(1, 1, tID_ZP));
            TCoords.Add(new Vector3(0, 0, tID_ZP));
            TCoords.Add(new Vector3(1, 1, tID_ZP));
            TCoords.Add(new Vector3(1, 0, tID_ZP));
            // Sprite 6: 0,0,0.5 -> 1,1,0.5 side two
            TCoords.Add(new Vector3(1, 0, tID_ZP));
            TCoords.Add(new Vector3(1, 1, tID_ZP));
            TCoords.Add(new Vector3(0, 0, tID_ZP));
            TCoords.Add(new Vector3(1, 1, tID_ZP));
            TCoords.Add(new Vector3(0, 1, tID_ZP));
            TCoords.Add(new Vector3(0, 0, tID_ZP));
            return TCoords;
        }

        public override bool OccupiesXP()
        {
            return false;
        }

        public override bool OccupiesYP()
        {
            return false;
        }

        public override bool OccupiesXM()
        {
            return false;
        }

        public override bool OccupiesYM()
        {
            return false;
        }

        public override bool OccupiesTOP()
        {
            return false;
        }

        public override bool OccupiesBOTTOM()
        {
            return false;
        }
    }
}
