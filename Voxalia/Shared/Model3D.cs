using System.Collections.Generic;
using BEPUutilities;

namespace Voxalia.Shared
{
    public class Model3D
    {
        public List<Model3DMesh> Meshes;
        public Model3DNode RootNode;
        public Matrix MatrixA;
    }

    public class Model3DMesh
    {
        public List<Vector3> Vertices;
        public List<Vector3> Normals;
        public List<Vector2> TexCoords;
        public List<Model3DBone> Bones;
        public string Name;
    }

    public class Model3DBone
    {
        public string Name;
        public List<int> IDs;
        public List<float> Weights;
        public Matrix MatrixA;
    }

    public class Model3DNode
    {
        public string Name;
        public Matrix MatrixA;
        public Model3DNode Parent;
        public List<Model3DNode> Children;
    }
}
