using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assimp;
using Assimp.Configs;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;

namespace ShadowOperations.Shared
{
    public class ModelHandler
    {
        public static string CubeData = "o Cube\nv 1.000000 0.000000 0.000000\nv 1.000000 0.000000 1.000000\nv 0.000000 0.000000 1.000000\n" +
            "v 0.000000 0.000000 0.000000\nv 1.000000 1.000000 0.000000\nv 1.000000 1.000000 1.000000\nv 0.000000 1.000000 1.000000\n" +
            "v 0.000000 1.000000 0.000000\nvt 1.000000 0.000000\nvt 1.000000 1.000000\nvt 0.000000 1.000000\nvt 0.000000 0.000000\n" +
            "f 2/1 3/2 4/3\nf 8/1 7/2 6/3\nf 1/4 5/1 6/2\nf 2/4 6/1 7/2\nf 7/1 8/2 4/3\nf 1/1 4/2 8/3\nf 1/4 2/1 4/3\nf 5/4 8/1 6/3\n" +
            "f 2/3 1/4 6/2\nf 3/3 2/4 7/2\nf 3/4 7/1 4/3\nf 5/4 1/1 8/3\n"; // TODO: Normals!

        public Scene LoadModel(byte[] data, string ext)
        {
            if (ext == null || ext == "")
            {
                ext = "obj";
            }
            using (AssimpContext ACont = new AssimpContext())
            {
                ACont.SetConfig(new NormalSmoothingAngleConfig(66f));
                return ACont.ImportFileFromStream(new DataStream(data), PostProcessSteps.Triangulate, ext);
            }
        }

        public List<Vector3> GetCollisionVertices(Scene input)
        {
            List<Vector3> vertices = new List<Vector3>();
            bool colOnly = false;
            foreach (Mesh mesh in input.Meshes)
            {
                if (mesh.Name.ToLower().Contains("collision"))
                {
                    colOnly = true;
                    break;
                }
            }
            foreach (Mesh mesh in input.Meshes)
            {
                if (!colOnly || mesh.Name.ToLower().Contains("collision"))
                {
                    AddMesh(mesh, vertices);
                }
            }
            return vertices;
        }

        public MobileMeshShape MeshToBepu(Scene input)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            bool colOnly = false;
            foreach (Mesh mesh in input.Meshes)
            {
                if (mesh.Name.ToLower().Contains("collision"))
                {
                    colOnly = true;
                    break;
                }
            }
            foreach (Mesh mesh in input.Meshes)
            {
                if (!colOnly || mesh.Name.ToLower().Contains("collision"))
                {
                    AddMesh(mesh, vertices, indices);
                }
            }
            return new MobileMeshShape(vertices.ToArray(), indices.ToArray(), AffineTransform.Identity, MobileMeshSolidity.DoubleSided);
        }

        void AddMesh(Mesh mesh, List<Vector3> vertices)
        {
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                Vector3D vert = mesh.Vertices[i];
                vertices.Add(new Vector3(vert.X, vert.Y, vert.Z));
            }
        }

        void AddMesh(Mesh mesh, List<Vector3> vertices, List<int> indices)
        {
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                Vector3D vert = mesh.Vertices[i];
                vertices.Add(new Vector3(vert.X, vert.Y, vert.Z));
            }
            foreach (Face face in mesh.Faces)
            {
                if (face.Indices.Count == 3)
                {
                    for (int i = 2; i >= 0; i--)
                    {
                        indices.Add(face.Indices[i]);
                    }
                }
                else
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has face with " + face.Indices.Count + " faces!");
                }
            }
        }
    }
}
