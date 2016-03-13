using System.Collections.Generic;
using BEPUutilities;
using System;
using BEPUphysics.CollisionShapes;
using Voxalia.Shared.Files;
using BEPUphysics.CollisionShapes.ConvexShapes;

namespace Voxalia.Shared
{
    /// <summary>
    /// Handles abstract 3D models. Can be purposed for both collision systems and rendering.
    /// </summary>
    public class ModelHandler
    {
        /// <summary>
        /// Loads a model from .VMD (Voxalia Model Data) input.
        /// </summary>
        public Model3D LoadModel(byte[] data)
        {
            if (data.Length < 3 || data[0] != 'V' || data[1] != 'M' || data[2] != 'D')
            {
                throw new Exception("Model3D: Invalid header bits.");
            }
            byte[] dat_filt = new byte[data.Length - "VMD001".Length];
            Array.ConstrainedCopy(data, "VMD001".Length, dat_filt, 0, dat_filt.Length);
            dat_filt = FileHandler.UnGZip(dat_filt);
            DataStream ds = new DataStream(dat_filt);
            DataReader dr = new DataReader(ds);
            Model3D mod = new Model3D();
            mod.MatrixA = ReadMat(dr);
            int meshCount = dr.ReadInt();
            mod.Meshes = new List<Model3DMesh>(meshCount);
            for (int m = 0; m < meshCount; m++)
            {
                Model3DMesh mesh = new Model3DMesh();
                mod.Meshes.Add(mesh);
                mesh.Name = dr.ReadFullString();
                int vertexCount = dr.ReadInt();
                mesh.Vertices = new List<Vector3>(vertexCount);
                for (int v = 0; v < vertexCount; v++)
                {
                    float f1 = dr.ReadFloat();
                    float f2 = dr.ReadFloat();
                    float f3 = dr.ReadFloat();
                    mesh.Vertices.Add(new Vector3(f1, f2, f3));
                }
                int indiceCount = dr.ReadInt() * 3;
                mesh.Indices = new List<int>(indiceCount);
                for (int i = 0; i < indiceCount; i++)
                {
                    mesh.Indices.Add(dr.ReadInt());
                }
                int tcCount = dr.ReadInt();
                mesh.TexCoords = new List<Vector2>(tcCount);
                for (int t = 0; t < tcCount; t++)
                {
                    float f1 = dr.ReadFloat();
                    float f2 = dr.ReadFloat();
                    mesh.TexCoords.Add(new Vector2(f1, f2));
                }
                int normCount = dr.ReadInt();
                mesh.Normals = new List<Vector3>(normCount);
                for (int n = 0; n < normCount; n++)
                {
                    float f1 = dr.ReadFloat();
                    float f2 = dr.ReadFloat();
                    float f3 = dr.ReadFloat();
                    mesh.Normals.Add(new Vector3(f1, f2, f3));
                }
                int boneCount = dr.ReadInt();
                mesh.Bones = new List<Model3DBone>(boneCount);
                for (int b = 0; b < boneCount; b++)
                {
                    Model3DBone bone = new Model3DBone();
                    mesh.Bones.Add(bone);
                    bone.Name = dr.ReadFullString();
                    int weights = dr.ReadInt();
                    bone.IDs = new List<int>(weights);
                    bone.Weights = new List<float>(weights);
                    for (int w = 0; w < weights; w++)
                    {
                        bone.IDs.Add(dr.ReadInt());
                        bone.Weights.Add(dr.ReadFloat());
                    }
                    bone.MatrixA = ReadMat(dr);
                }
            }
            mod.RootNode = ReadSingleNode(null, dr);
            return mod;
        }

        public Model3DNode ReadSingleNode(Model3DNode root, DataReader dr)
        {
            Model3DNode n = new Model3DNode();
            n.Parent = root;
            n.Name = dr.ReadFullString();
            n.MatrixA = ReadMat(dr);
            int cCount = dr.ReadInt();
            n.Children = new List<Model3DNode>(cCount);
            for (int i = 0; i < cCount; i++)
            {
                n.Children.Add(ReadSingleNode(n, dr));
            }
            return n;
        }

        public Matrix ReadMat(DataReader reader)
        {
            float a1 = reader.ReadFloat();
            float a2 = reader.ReadFloat();
            float a3 = reader.ReadFloat();
            float a4 = reader.ReadFloat();
            float b1 = reader.ReadFloat();
            float b2 = reader.ReadFloat();
            float b3 = reader.ReadFloat();
            float b4 = reader.ReadFloat();
            float c1 = reader.ReadFloat();
            float c2 = reader.ReadFloat();
            float c3 = reader.ReadFloat();
            float c4 = reader.ReadFloat();
            float d1 = reader.ReadFloat();
            float d2 = reader.ReadFloat();
            float d3 = reader.ReadFloat();
            float d4 = reader.ReadFloat();
            return new Matrix(a1, a2, a3, a4, b1, b2, b3, b4, c1, c2, c3, c4, d1, d2, d3, d4);
            //return new Matrix(a1, b1, c1, d1, a2, b2, c2, d2, a3, b3, c3, d3, a4, b4, c4, d4);
        }

        public List<Vector3> GetVertices(Model3D input)
        {
            List<Vector3> vertices = new List<Vector3>(input.Meshes.Count * 100);
            foreach (Model3DMesh mesh in input.Meshes)
            {
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    vertices.Add(mesh.Vertices[i]);
                }
            }
            return vertices;
        }

        public List<Vector3> GetCollisionVertices(Model3D input)
        {
            List<Vector3> vertices = new List<Vector3>(input.Meshes.Count * 100);
            bool colOnly = false;
            foreach (Model3DMesh mesh in input.Meshes)
            {
                if (mesh.Name.ToLowerInvariant().Contains("collision"))
                {
                    colOnly = true;
                    break;
                }
            }
            foreach (Model3DMesh mesh in input.Meshes)
            {
                if ((!colOnly || mesh.Name.ToLowerInvariant().Contains("collision")) && !mesh.Name.ToLowerInvariant().Contains("nocollide"))
                {
                    for (int i = 0; i < mesh.Indices.Count; i ++)
                    {
                        vertices.Add(mesh.Vertices[mesh.Indices[i]]);
                    }
                }
            }
            return vertices;
        }

        public MobileMeshShape MeshToBepu(Model3D input)
        {
            List<Vector3> vertices = GetCollisionVertices(input);
            List<int> indices = new List<int>(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                indices.Add(indices.Count);
            }
            return new MobileMeshShape(vertices.ToArray(), indices.ToArray(), AffineTransform.Identity, MobileMeshSolidity.DoubleSided);
        }

        public ConvexHullShape MeshToBepuConvex(Model3D input)
        {
            List<Vector3> vertices = GetCollisionVertices(input);
            ConvexHullHelper.RemoveRedundantPoints(vertices);
            return new ConvexHullShape(vertices);
        }
    }
}
