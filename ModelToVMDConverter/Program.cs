//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.Compression;
using Assimp;

namespace ModelToVMDConverter
{
    class Program
    {
        public static string EXENAME = "ModelToVMDConverter.exe"; // TODO: An env var for this?

        public static Encoding UTF8 = new UTF8Encoding(false);

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(EXENAME + " <filename>");
                return;
            }
            string fname = Environment.CurrentDirectory + "/" + args[0];
            if (!File.Exists(fname))
            {
                Console.WriteLine("Invalid filename.");
                Console.WriteLine(EXENAME + " <filename>");
                return;
            }
            AssimpContext ac = new AssimpContext();
            PT = args.Length > 1 && args[1].ToLower().Contains("pretrans");
            TEXTURE = args.Length > 1 && args[1].ToLower().Contains("texture");
            Console.WriteLine("Pre-transform = " + PT);
            Console.WriteLine("Texture = " + TEXTURE);
            Scene fdata = ac.ImportFile(fname, PostProcessSteps.Triangulate | PostProcessSteps.FlipWindingOrder);
            if (File.Exists(fname + ".vmd"))
            {
                File.Delete(fname + ".vmd");
            }
            FileStream fs = File.OpenWrite(fname + ".vmd");
            File.WriteAllText(fname + ".skin", ExportModelData(fdata, fs));
            fs.Flush();
            fs.Close();
        }

        static bool PT = false;

        static bool TEXTURE = false;

        static string ExportModelData(Scene scene, Stream baseoutstream)
        {
            baseoutstream.WriteByte((byte)'V');
            baseoutstream.WriteByte((byte)'M');
            baseoutstream.WriteByte((byte)'D');
            baseoutstream.WriteByte((byte)'0');
            baseoutstream.WriteByte((byte)'0');
            baseoutstream.WriteByte((byte)'1');
            MemoryStream ms = new MemoryStream();
            StreamWrapper outstream = new StreamWrapper(ms);
            WriteMatrix4x4(scene.RootNode.Transform, outstream);
            outstream.WriteInt(scene.MeshCount);
            Console.WriteLine("Writing " + scene.MeshCount + " meshes...");
            StringBuilder sb = new StringBuilder();
            for (int m = 0; m < scene.MeshCount; m++)
            {
                Mesh mesh = scene.Meshes[m];
                Console.WriteLine("Writing mesh: " + mesh.Name);
                string nname = mesh.Name.ToLower().Replace('#', '_').Replace('.', '_');
                if (PT && GetNode(scene.RootNode, nname) == null)
                {
                    Console.WriteLine("NO NODE FOR: " + nname);
                    continue;
                }
                Matrix4x4 mat = PT ? GetNode(scene.RootNode, nname).Transform * scene.RootNode.Transform : Matrix4x4.Identity;
                if (TEXTURE)
                {
                    Material mater = scene.Materials[mesh.MaterialIndex];
                    if (mater.HasTextureDiffuse)
                    {
                        sb.Append(mesh.Name + "=" + mater.TextureDiffuse.FilePath + "\n");
                    }
                    if (mater.HasTextureSpecular)
                    {
                        sb.Append(mesh.Name + ":::specular=" + scene.Materials[mesh.MaterialIndex].TextureSpecular.FilePath + "\n");
                    }
                    if (mater.HasTextureReflection)
                    {
                        sb.Append(mesh.Name + ":::reflectivity=" + scene.Materials[mesh.MaterialIndex].TextureReflection.FilePath + "\n");
                    }
                    if (mater.HasTextureNormal)
                    {
                        sb.Append(mesh.Name + ":::normal=" + scene.Materials[mesh.MaterialIndex].TextureNormal.FilePath + "\n");
                    }
                }
                else
                {
                    sb.Append(mesh.Name + "=UNKNOWN\n");
                }
                byte[] dat = UTF8.GetBytes(mesh.Name);
                outstream.WriteInt(dat.Length);
                outstream.BaseStream.Write(dat, 0, dat.Length);
                outstream.WriteInt(mesh.VertexCount);
                for (int v = 0; v < mesh.VertexCount; v++)
                {
                    WriteVector3D(mat * mesh.Vertices[v], outstream);
                }
                outstream.WriteInt(mesh.FaceCount);
                for (int f = 0; f < mesh.FaceCount; f++)
                {
                    Face face = mesh.Faces[f];
                    outstream.WriteInt(face.Indices[0]);
                    outstream.WriteInt(face.Indices[face.IndexCount > 1 ? 1 : 0]);
                    outstream.WriteInt(face.Indices[face.IndexCount > 2 ? 2 : 0]);
                }
                outstream.WriteInt(mesh.TextureCoordinateChannels[0].Count);
                for (int t = 0; t < mesh.TextureCoordinateChannels[0].Count; t++)
                {
                    outstream.WriteFloat(mesh.TextureCoordinateChannels[0][t].X);
                    outstream.WriteFloat(mesh.TextureCoordinateChannels[0][t].Y);
                }
                outstream.WriteInt(mesh.Normals.Count);
                Matrix4x4 nmat = mat;
                nmat.Inverse();
                nmat.Transpose();
                Matrix3x3 nmat3 = new Matrix3x3(nmat);
                for (int n = 0; n < mesh.Normals.Count; n++)
                {
                    WriteVector3D(nmat3 * mesh.Normals[n], outstream);
                }
                outstream.WriteInt(mesh.BoneCount);
                for (int b = 0; b < mesh.BoneCount; b++)
                {
                    Bone bone = mesh.Bones[b];
                    byte[] bdat = UTF8.GetBytes(bone.Name);
                    outstream.WriteInt(bdat.Length);
                    outstream.BaseStream.Write(bdat, 0, bdat.Length);
                    outstream.WriteInt(bone.VertexWeightCount);
                    for (int v = 0; v < bone.VertexWeightCount; v++)
                    {
                        outstream.WriteInt(bone.VertexWeights[v].VertexID);
                        outstream.WriteFloat(bone.VertexWeights[v].Weight);
                    }
                    WriteMatrix4x4(bone.OffsetMatrix, outstream);
                }
            }
            OutputNode(scene.RootNode, outstream);
            byte[] msd = ms.ToArray();
            msd = GZip(msd);
            baseoutstream.Write(msd, 0, msd.Length);
            return sb.ToString();
        }

        static Node GetNode(Node root, string namelow)
        {
            if (root.Name.ToLower() == namelow)
            {
                return root;
            }
            foreach (Node child in root.Children)
            {
                Node res = GetNode(child, namelow);
                if (res != null)
                {
                    return child;
                }
            }
            return null;
        }

        static void OutputNode(Node n, StreamWrapper outstream)
        {
            byte[] dat = UTF8.GetBytes(n.Name);
            outstream.WriteInt(dat.Length);
            outstream.BaseStream.Write(dat, 0, dat.Length);
            Console.WriteLine("Output node: " + n.Name);
            WriteMatrix4x4(n.Transform, outstream);
            outstream.WriteInt(n.ChildCount);
            for (int i = 0; i < n.ChildCount; i++)
            {
                OutputNode(n.Children[i], outstream);
            }
        }

        static void WriteMatrix4x4(Matrix4x4 mat, StreamWrapper outstream)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    outstream.WriteFloat(mat[i + 1, j + 1]);
                }
            }
        }

        static void WriteVector3D(Vector3D vec, StreamWrapper outstream)
        {
            outstream.WriteFloat(vec.X);
            outstream.WriteFloat(vec.Y);
            outstream.WriteFloat(vec.Z);
        }

        public static byte[] GZip(byte[] input)
        {
            MemoryStream memstream = new MemoryStream();
            GZipStream GZStream = new GZipStream(memstream, CompressionMode.Compress);
            GZStream.Write(input, 0, input.Length);
            GZStream.Close();
            byte[] finaldata = memstream.ToArray();
            memstream.Close();
            return finaldata;
        }
    }
}
