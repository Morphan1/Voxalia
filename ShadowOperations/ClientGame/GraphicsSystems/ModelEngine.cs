using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using Frenetic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Assimp;

namespace ShadowOperations.ClientGame.GraphicsSystems
{
    public class ModelEngine
    {
        /// <summary>
        /// All currently loaded models.
        /// </summary>
        public List<Model> LoadedModels;

        public ModelHandler Handler;

        public Model Cube;

        /// <summary>
        /// Prepares the model system.
        /// </summary>
        public void Init()
        {
            Handler = new ModelHandler();
            LoadedModels = new List<Model>();
            Cube = FromBytes("cube", FileHandler.encoding.GetBytes(CubeData));
            LoadedModels.Add(Cube);
        }

        static string CubeData = "o Cube\nv 1.000000 0.000000 0.000000\nv 1.000000 0.000000 1.000000\nv 0.000000 0.000000 1.000000\n" +
            "v 0.000000 0.000000 0.000000\nv 1.000000 1.000000 0.000000\nv 1.000000 1.000000 1.000000\nv 0.000000 1.000000 1.000000\n" +
            "v 0.000000 1.000000 0.000000\nvt 1.000000 0.000000\nvt 1.000000 1.000000\nvt 0.000000 1.000000\nvt 0.000000 0.000000\n" +
            "f 2/1 3/2 4/3\nf 8/1 7/2 6/3\nf 1/4 5/1 6/2\nf 2/4 6/1 7/2\nf 7/1 8/2 4/3\nf 1/1 4/2 8/3\nf 1/4 2/1 4/3\nf 5/4 8/1 6/3\n" +
            "f 2/3 1/4 6/2\nf 3/3 2/4 7/2\nf 3/4 7/1 4/3\nf 5/4 1/1 8/3\n"; // TODO: Normals!

        public Model LoadModel(string filename)
        {
            try
            {
                filename = FileHandler.CleanFileName(filename);
                if (!Program.Files.Exists("models/" + filename))
                {
                    SysConsole.Output(OutputType.WARNING, "Cannot load model, file '" +
                        TextStyle.Color_Standout + "models/" + filename  + TextStyle.Color_Warning +
                        "' does not exist.");
                    return null;
                }
                return FromBytes(filename, Program.Files.ReadBytes("models/" + filename));
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to load model from filename '" +
                    TextStyle.Color_Standout + "models/" + filename + TextStyle.Color_Error + "'" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Gets the texture object for a specific texture name.
        /// </summary>
        /// <param name="texturename">The name of the texture</param>
        /// <returns>A valid texture object</returns>
        public Model GetModel(string modelname)
        {
            modelname = FileHandler.CleanFileName(modelname);
            for (int i = 0; i < LoadedModels.Count; i++)
            {
                if (LoadedModels[i].Name == modelname)
                {
                    return LoadedModels[i];
                }
            }
            Model Loaded = null;
            try
            {
                Loaded = LoadModel(modelname);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, ex.ToString());
            }
            if (Loaded == null)
            {
                Loaded = FromBytes("model.obj", FileHandler.encoding.GetBytes(CubeData));
                Loaded.Name = modelname;
            }
            LoadedModels.Add(Loaded);
            return Loaded;
        }

        /// <summary>
        /// loads a model from a file byte array.
        /// </summary>
        /// <param name="name">The name of the model</param>
        /// <param name="data">The .obj file string</param>
        /// <returns>A valid model</returns>
        public Model FromBytes(string name, byte[] data)
        {
            Scene scene = Handler.LoadModel(data, name.Substring(name.LastIndexOf('.') + 1));
            return FromScene(scene, name);
        }

        public Model FromScene(Scene scene, string name)
        {
            if (!scene.HasMeshes)
            {
                throw new Exception("Scene has no meshes!");
            }
            Model model = new Model(name);
            model.OriginalModel = scene;
            foreach (Mesh mesh in scene.Meshes)
            {
                ModelMesh modmesh = new ModelMesh(mesh.Name, mesh);
                modmesh.Base = scene;
                modmesh.vbo.Prepare();
                bool hastc = mesh.HasTextureCoords(0);
                bool hasn = mesh.HasNormals;
                if (!hasn)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has no normals!");
                }
                if (!hastc)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has no texcoords!");
                }
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    Vector3D vertex = mesh.Vertices[i];
                    modmesh.vbo.Vertices.Add(new Vector3(vertex.X, vertex.Y, vertex.Z));
                    if (!hastc)
                    {
                        modmesh.vbo.TexCoords.Add(new Vector3(0, 0, 0));
                    }
                    else
                    {
                        Vector3D texCoord = mesh.TextureCoordinateChannels[0][i];
                        modmesh.vbo.TexCoords.Add(new Vector3(texCoord.X, 1 - texCoord.Y, texCoord.Z));
                    }
                    if (!hasn)
                    {
                        modmesh.vbo.Normals.Add(new Vector3(0, 0, 1));
                    }
                    else
                    {
                        modmesh.vbo.Normals.Add(new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z));
                    }
                    modmesh.vbo.Colors.Add(new Vector4(1, 1, 1, 1)); // TODO: From the mesh?
                }
                foreach (Face face in mesh.Faces)
                {
                    if (face.Indices.Count == 3)
                    {
                        for (int i = 2; i >= 0; i--)
                        {
                            modmesh.vbo.Indices.Add((uint)face.Indices[i]);
                        }
                    }
                    else
                    {
                        SysConsole.Output(OutputType.WARNING, "Mesh has face with " + face.Indices.Count + " vertices!");
                    }
                }
                int bc = mesh.Bones.Count;
                if (bc > 70)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has " + bc + " bones!");
                    bc = 70;
                }
                modmesh.vbo.BoneIDs = new Vector4[modmesh.vbo.Vertices.Count].ToList();
                modmesh.vbo.BoneWeights = new Vector4[modmesh.vbo.Vertices.Count].ToList();
                int[] pos = new int[modmesh.vbo.Vertices.Count];
                for (int i = 0; i < bc; i++)
                {
                    for (int x = 0; x < mesh.Bones[i].VertexWeights.Count; x++)
                    {
                        VertexWeight vw = mesh.Bones[i].VertexWeights[x];
                        int spot = pos[vw.VertexID]++;
                        if (spot > 3)
                        {
                            //SysConsole.Output(OutputType.WARNING, "Too many bones influencing " + vw.VertexID + "!");
                            continue;
                        }
                        ForceSet(modmesh.vbo.BoneIDs, vw.VertexID, spot, i);
                        ForceSet(modmesh.vbo.BoneWeights, vw.VertexID, spot, vw.Weight);
                    }
                    modmesh.Bones.Add(new ModelBone() { Internal = mesh.Bones[i] });
                    string nl = mesh.Bones[i].Name.ToLower();
                    if (!modmesh.BoneLookup.ContainsKey(nl))
                    {
                        modmesh.BoneLookup.Add(nl, modmesh.Bones.Count - 1);
                    }
                    else
                    {
                        SysConsole.Output(OutputType.WARNING, "Bone " + nl + " defined repeatedly!");
                    }
                }
                model.Meshes.Add(modmesh);
                modmesh.GenerateVBO();
            }
            return model;
        }

        void ForceSet(List<Vector4> vecs, int ind, int subind, float val)
        {
            Vector4 vec = vecs[ind];
            vec[subind] = val;
            vecs[ind] = vec;
        }
    }

    /// <summary>
    /// Represents a 3D model.
    /// </summary>
    public class Model
    {
        public Scene OriginalModel;

        public Model(string _name)
        {
            Name = _name;
            Meshes = new List<ModelMesh>();
        }

        /// <summary>
        /// The name of  this model.
        /// </summary>
        public string Name;

        /// <summary>
        /// All the meshes this model has.
        /// </summary>
        public List<ModelMesh> Meshes;

        public ModelMesh MeshFor(string name)
        {
            name = name.ToLower();
            for (int i = 0; i < Meshes.Count; i++)
            {
                if (Meshes[i].Name.StartsWith(name))
                {
                    return Meshes[i];
                }
            }
            return null;
        }

        void SetBones(Matrix4[] mats)
        {
            int bones = 70;
            float[] set = new float[bones * 16];
            for (int i = 0; i < mats.Length; i++)
            {
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        set[i * 16 + x * 4 + y] = mats[i][x, y];
                    }
                }
            }
            for (int i = mats.Length; i < bones; i++)
            {
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        set[i * 16 + x * 4 + y] = Matrix4.Identity[x, y];
                    }
                }
            }
            GL.UniformMatrix4(6, bones, false, set);
        }

        public Matrix4 convert(Matrix4x4 mat)
        {
            return new Matrix4(mat.A1, mat.A2, mat.A3, mat.A4,
                mat.B1, mat.B2, mat.B3, mat.B4,
                mat.C1, mat.C2, mat.C3, mat.C4,
                mat.D1, mat.D2, mat.D3, mat.D4);
        }

        int findPos(double time, SingleAnimationNode nodeAnim)
        {
            for (int i = 0; i < nodeAnim.Positions.Count - 1; i++)
            {
                if (time >= nodeAnim.PosTimes[i] && time < nodeAnim.PosTimes[i + 1])
                {
                    return i;
                }
            }
            return 0;
        }

        Vector3D lerpPos(double aTime, SingleAnimationNode nodeAnim)
        {
            if (nodeAnim.Positions.Count == 0)
            {
                return new Vector3D(0, 0, 0);
            }
            if (nodeAnim.Positions.Count == 1)
            {
                Location pos = nodeAnim.Positions[0];
                return new Vector3D((float)pos.X, (float)pos.Y, (float)pos.Z);
            }
            int index = findPos(aTime, nodeAnim);
            int nextIndex = index + 1;
            if (nextIndex >= nodeAnim.Positions.Count)
            {
                Location pos = nodeAnim.Positions[0];
                return new Vector3D((float)pos.X, (float)pos.Y, (float)pos.Z);
            }
            double deltaT = nodeAnim.PosTimes[nextIndex] - nodeAnim.PosTimes[index];
            double factor = (aTime - nodeAnim.PosTimes[index]) / deltaT;
            if (factor < 0 || factor > 1)
            {
                Location pos = nodeAnim.Positions[0];
                return new Vector3D((float)pos.X, (float)pos.Y, (float)pos.Z);
            }
            Location start = nodeAnim.Positions[index];
            Location end = nodeAnim.Positions[nextIndex];
            Location deltaV = end - start;
            Location npos = start + (float)factor * deltaV;
            return new Vector3D((float)npos.X, (float)npos.Y, (float)npos.Z);
        }

        int findRotate(double time, SingleAnimationNode nodeAnim)
        {
            for (int i = 0; i < nodeAnim.Rotations.Count; i++)
            {
                if (time >= nodeAnim.RotTimes[i] && time < nodeAnim.RotTimes[i + 1])
                {
                    return i;
                }
            }
            return 0;
        }

        Assimp.Quaternion lerpRotate(double aTime, SingleAnimationNode nodeAnim)
        {
            if (nodeAnim.Rotations.Count == 0)
            {
                return new Assimp.Quaternion(0, 0, 0);
            }
            if (nodeAnim.Rotations.Count == 1)
            {
                return nodeAnim.Rotations[0];
            }
            int index = findRotate(aTime, nodeAnim);
            int nextIndex = index + 1;
            if (nextIndex >= nodeAnim.Rotations.Count)
            {
                return nodeAnim.Rotations[0];
            }
            double deltaT = nodeAnim.RotTimes[nextIndex] - nodeAnim.RotTimes[index];
            double factor = (aTime - nodeAnim.RotTimes[index]) / deltaT;
            if (factor < 0 || factor > 1)
            {
                return nodeAnim.Rotations[0];
            }
            Assimp.Quaternion start = nodeAnim.Rotations[index];
            Assimp.Quaternion end = nodeAnim.Rotations[nextIndex];
            Assimp.Quaternion res = Assimp.Quaternion.Slerp(start, end, (float)factor);
            res.Normalize();
            return res;
        }

        Matrix4 globalInverse = Matrix4.Identity;

        public void UpdateTransforms(double aTime, Node pNode, Matrix4 transf, byte mode)
        {
            try
            {
                string nodename = pNode.Name.ToLower();
                Matrix4 nodeTransf = Matrix4.Identity;
                SingleAnimationNode pNodeAnim = FindNodeAnim(nodename, mode);
                if (pNodeAnim != null)
                {
                    nodeTransf = convert(Matrix4x4.FromTranslation(lerpPos(aTime, pNodeAnim))) * convert(new Matrix4x4(lerpRotate(aTime, pNodeAnim).GetMatrix()));
                }
                Matrix4 global = transf * nodeTransf;
                foreach (ModelMesh mesh in Meshes)
                {
                    int pos;
                    if (mesh.BoneLookup.TryGetValue(nodename, out pos))
                    {
                        mesh.Bones[pos].Transform = global * convert(mesh.Bones[pos].Internal.OffsetMatrix);
                    }
                }
                for (int i = 0; i < pNode.ChildCount; i++)
                {
                    UpdateTransforms(aTime, pNode.Children[i], global, mode);
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, ex.ToString());
            }
        }

        SingleAnimationNode FindNodeAnim(string nodeName, int mode)
        {
            List<SingleAnimationNode> nodes;
            if (mode == 0)
            {
                nodes = hAnim.Nodes;
            }
            else if (mode == 1)
            {
                nodes = tAnim.Nodes;
            }
            else
            {
                nodes = lAnim.Nodes;
            }
            for (int i = 0; i < nodes.Count; i++)
            {
                SingleAnimationNode nac = nodes[i];
                if (nac.Name == nodeName)
                {
                    return nac;
                }
            }
            return null;
        }

        SingleAnimation hAnim;
        SingleAnimation tAnim;
        SingleAnimation lAnim;

        /// <summary>
        /// Draws the model.
        /// </summary>
        public void Draw(double aTimeHead = 0, SingleAnimation headanim = null, double aTimeTorso = 0, SingleAnimation torsoanim = null, double aTimeLegs = 0, SingleAnimation legsanim = null)
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                if (Meshes[i].Bones.Count > 0)
                {
                    hAnim = headanim;
                    tAnim = torsoanim;
                    lAnim = legsanim;
                    if (hAnim != null || tAnim != null || lAnim != null)
                    {
                        globalInverse = convert(OriginalModel.RootNode.Transform).Inverted();
                        if (hAnim != null)
                        {
                            UpdateTransforms(aTimeHead, OriginalModel.RootNode, Matrix4.Identity, 0);
                        }
                        if (tAnim != null)
                        {
                            UpdateTransforms(aTimeTorso, OriginalModel.RootNode, Matrix4.Identity, 1);
                        }
                        if (lAnim != null)
                        {
                            UpdateTransforms(aTimeLegs, OriginalModel.RootNode, Matrix4.Identity, 2);
                        }
                        Matrix4[] mats = new Matrix4[Meshes[i].Bones.Count];
                        for (int x = 0; x < Meshes[i].Bones.Count; x++)
                        {
                            mats[x] = Meshes[i].Bones[x].Transform;
                        }
                        SetBones(mats);
                    }
                    else
                    {
                        SetBones(new Matrix4[] { });
                    }
                }
                Meshes[i].Draw();
                if (Meshes[i].Bones.Count > 0)
                {
                    VBO.BonesIdentity();
                }
            }
        }

        public bool Skinned = false;

        public void LoadSkin(TextureEngine texs)
        {
            if (Skinned)
            {
                return;
            }
            if (Program.Files.Exists("models/" + Name + ".skin"))
            {
                string[] data = Program.Files.ReadText("models/" + Name + ".skin").Split('\n');
                foreach (string datum in data)
                {
                    if (datum.Length > 0)
                    {
                        string[] datums = datum.Split('=');
                        if (datums.Length == 2)
                        {
                            Texture tex = texs.GetTexture(datums[1]);
                            bool success = false;
                            for (int i = 0; i < Meshes.Count; i++)
                            {
                                if (Meshes[i].Name == datums[0])
                                {
                                    Meshes[i].vbo.Tex = tex;
                                    success = true;
                                }
                            }
                            if (!success)
                            {
                                SysConsole.Output(OutputType.WARNING, "Unknown skin entry " + datums[0]);
                                StringBuilder all = new StringBuilder(Meshes.Count * 100);
                                for (int i = 0; i < Meshes.Count; i++)
                                {
                                    all.Append(Meshes[i].Name + ", ");
                                }
                                SysConsole.Output(OutputType.WARNING, "Available: " + all.ToString());
                            }
                        }
                    }
                }
                Skinned = true;
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "Can't find models/" + Name + ".skin!");
            }
        }
    }

    public class ModelBone
    {
        public Bone Internal = null;
        public Matrix4 Transform = Matrix4.Identity;
    }

    public class ModelMesh
    {
        /// <summary>
        /// The name of this mesh.
        /// </summary>
        public string Name;

        public Scene Base;

        public Mesh Original;

        public List<ModelBone> Bones;

        public Dictionary<string, int> BoneLookup;

        public ModelMesh(string _name, Mesh orig)
        {
            Original = orig;
            Name = _name.ToLower();
            if (Name.EndsWith(".001"))
            {
                Name = Name.Substring(0, Name.Length - ".001".Length);
            }
            Faces = new List<ModelFace>();
            Bones = new List<ModelBone>();
            BoneLookup = new Dictionary<string, int>();
            vbo = new VBO();
        }

        /// <summary>
        /// All the mesh's faces.
        /// </summary>
        public List<ModelFace> Faces;

        /// <summary>
        /// The VBO for this mesh.
        /// </summary>
        public VBO vbo;

        public void DestroyVBO()
        {
            vbo.Destroy();
        }

        public void GenerateVBO()
        {
            vbo.GenerateVBO();
        }

        /// <summary>
        /// Renders the mesh.
        /// </summary>
        public void Draw()
        {
            vbo.Render(true);
        }
    }

    public class ModelFace
    {
        public ModelFace(int _l1, int _l2, int _l3, int _t1, int _t2, int _t3, Location _normal)
        {
            L1 = _l1;
            L2 = _l2;
            L3 = _l3;
            T1 = _t1;
            T2 = _t2;
            T3 = _t3;
            Normal = _normal;
        }

        public Location Normal;

        public int L1;
        public int L2;
        public int L3;

        public int T1;
        public int T2;
        public int T3;
    }
}
