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
            Scene fdata = ac.ImportFile(fname, PostProcessSteps.Triangulate);
            if (File.Exists(fname + ".vmd"))
            {
                File.Delete(fname + ".vmd");
            }
            FileStream fs = File.OpenWrite(fname + ".vmd");
            ExportModelData(fdata, fs);
            fs.Flush();
            fs.Close();
        }

        static void ExportModelData(Scene scene, Stream baseoutstream)
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
            for (int m = 0; m < scene.MeshCount; m++)
            {
                Mesh mesh = scene.Meshes[m];
                byte[] dat = UTF8.GetBytes(mesh.Name);
                outstream.WriteInt(dat.Length);
                outstream.BaseStream.Write(dat, 0, dat.Length);
                outstream.WriteInt(mesh.FaceCount);
                for (int f = 0; f < mesh.FaceCount; f++)
                {
                    Face face = mesh.Faces[f];
                    WriteVector3D(mesh.Vertices[face.Indices[0]], outstream);
                    WriteVector3D(mesh.Vertices[face.Indices[face.IndexCount > 1 ? 1 : 0]], outstream);
                    WriteVector3D(mesh.Vertices[face.Indices[face.IndexCount > 2 ? 2 : 0]], outstream);
                }
                outstream.WriteInt(mesh.TextureCoordinateChannelCount);
                for (int t = 0; t < mesh.TextureCoordinateChannelCount; t++)
                {
                    List<Vector3D> tc = mesh.TextureCoordinateChannels[t];
                    if (tc == null || tc.Count < 1)
                    {
                        outstream.WriteFloat(0f);
                        outstream.WriteFloat(0f);
                    }
                    else
                    {
                        outstream.WriteFloat(tc[0].X);
                        outstream.WriteFloat(tc[0].Y);
                    }
                }
                outstream.WriteInt(mesh.Normals.Count);
                for (int n = 0; n < mesh.Normals.Count; n++)
                {
                    WriteVector3D(mesh.Normals[n], outstream);
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
        }

        static void OutputNode(Node n, StreamWrapper outstream)
        {
            byte[] dat = UTF8.GetBytes(n.Name);
            outstream.WriteInt(dat.Length);
            outstream.BaseStream.Write(dat, 0, dat.Length);
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
                    outstream.WriteFloat(mat[i, j]);
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
