//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Text;
using Voxalia.Shared;
using FreneticScript;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared.Files;
using System.Linq;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.GraphicsSystems
{
    public class ModelEngine
    {
        /// <summary>
        /// All currently loaded models.
        /// </summary>
        public List<Model> LoadedModels;

        public ModelHandler Handler;

        public Model Cube;

        public Model Cylinder;

        public Model Sphere;

        public Client TheClient;

        /// <summary>
        /// Prepares the model system.
        /// </summary>
        public void Init(AnimationEngine engine, Client tclient)
        {
            TheClient = tclient;
            AnimEngine = engine;
            Handler = new ModelHandler();
            LoadedModels = new List<Model>();
            Cube = GetModel("cube");
            Cylinder = GetModel("cylinder");
            Sphere = GetModel("sphere");
        }

        public void Update(double time)
        {
            cTime = time;
        }

        public double cTime = 0;

        public Model LoadModel(string filename)
        {
            try
            {
                filename = FileHandler.CleanFileName(filename);
                if (!TheClient.Files.Exists("models/" + filename + ".vmd"))
                {
                    SysConsole.Output(OutputType.WARNING, "Cannot load model, file '" +
                        TextStyle.Color_Standout + "models/" + filename + ".vmd" + TextStyle.Color_Warning +
                        "' does not exist.");
                    return null;
                }
                return FromBytes(filename, TheClient.Files.ReadBytes("models/" + filename + ".vmd"));
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to load model from filename '" +
                    TextStyle.Color_Standout + "models/" + filename + ".vmd" + TextStyle.Color_Error + "': " + ex.ToString());
                return null;
            }
        }

        Object Locker = new Object();

        /// <summary>
        /// Gets the model object for a specific model name.
        /// </summary>
        /// <param name="modelname">The name of the model.</param>
        /// <returns>A valid model object.</returns>
        public Model GetModel(string modelname)
        {
            lock (Locker)
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
                    if (norecurs)
                    {
                        Loaded = new Model(modelname) { Engine = this, Root = Matrix4.Identity, Meshes = new List<ModelMesh>(), RootNode = null };
                    }
                    else
                    {
                        norecurs = true;
                        Model m = GetModel("cube");
                        norecurs = false;
                        Loaded = new Model(modelname) { Engine = this, Root = m.Root, RootNode = m.RootNode, Meshes = m.Meshes, Original = m.Original };
                    }
                }
                LoadedModels.Add(Loaded);
                return Loaded;
            }
        }

        bool norecurs = false;

        public AnimationEngine AnimEngine;

        /// <summary>
        /// loads a model from a file byte array.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        /// <param name="data">The .obj file string.</param>
        /// <returns>A valid model.</returns>
        public Model FromBytes(string name, byte[] data)
        {
            Model3D scene = Handler.LoadModel(data);
            return FromScene(scene, name, AnimEngine);
        }

        public Model FromScene(Model3D scene, string name, AnimationEngine engine)
        {
            if (scene.Meshes.Count == 0)
            {
                throw new Exception("Scene has no meshes! (" + name + ")");
            }
            Model model = new Model(name);
            model.Engine = this;
            model.Original = scene;
            model.Root = convert(scene.MatrixA);
            foreach (Model3DMesh mesh in scene.Meshes)
            {
                if (mesh.Name.ToLowerFast().Contains("collision") || mesh.Name.ToLowerFast().Contains("norender"))
                {
                    continue;
                }
                ModelMesh modmesh = new ModelMesh(mesh.Name);
                modmesh.vbo.Prepare();
                bool hastc = mesh.TexCoords.Count == mesh.Vertices.Count;
                bool hasn = mesh.Normals.Count == mesh.Vertices.Count;
                if (!hasn)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has no normals! (" + name + ")");
                }
                if (!hastc)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has no texcoords! (" + name + ")");
                }
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    BEPUutilities.Vector3 vertex = mesh.Vertices[i];
                    modmesh.vbo.Vertices.Add(new Vector3((float)vertex.X, (float)vertex.Y, (float)vertex.Z));
                    if (!hastc)
                    {
                        modmesh.vbo.TexCoords.Add(new Vector3(0, 0, 0));
                    }
                    else
                    {
                        modmesh.vbo.TexCoords.Add(new Vector3((float)mesh.TexCoords[i].X, 1 - (float)mesh.TexCoords[i].Y, 0));
                    }
                    if (!hasn)
                    {
                        modmesh.vbo.Normals.Add(new Vector3(0f, 0f, 1f));
                    }
                    else
                    {
                        modmesh.vbo.Normals.Add(new Vector3((float)mesh.Normals[i].X, (float)mesh.Normals[i].Y, (float)mesh.Normals[i].Z));
                    }
                    modmesh.vbo.Colors.Add(new Vector4(1, 1, 1, 1)); // TODO: From the mesh?
                }
                for (int i = 0; i < mesh.Indices.Count; i++)
                {
                    modmesh.vbo.Indices.Add((uint)mesh.Indices[i]);
                }
                int bc = mesh.Bones.Count;
                if (bc > 200)
                {
                    SysConsole.Output(OutputType.WARNING, "Mesh has " + bc + " bones! (" + name + ")");
                    bc = 200;
                }
                modmesh.vbo.BoneIDs = new Vector4[modmesh.vbo.Vertices.Count].ToList();
                modmesh.vbo.BoneWeights = new Vector4[modmesh.vbo.Vertices.Count].ToList();
                modmesh.vbo.BoneIDs2 = new Vector4[modmesh.vbo.Vertices.Count].ToList();
                modmesh.vbo.BoneWeights2 = new Vector4[modmesh.vbo.Vertices.Count].ToList();
                int[] pos = new int[modmesh.vbo.Vertices.Count];
                for (int i = 0; i < bc; i++)
                {
                    for (int x = 0; x < mesh.Bones[i].Weights.Count; x++)
                    {
                        int IDa = mesh.Bones[i].IDs[x];
                        float Weighta = (float)mesh.Bones[i].Weights[x];
                        int spot = pos[IDa]++;
                        if (spot > 7)
                        {
                            //SysConsole.Output(OutputType.WARNING, "Too many bones influencing " + vw.VertexID + "!");
                            ForceSet(modmesh.vbo.BoneWeights, IDa, 3, modmesh.vbo.BoneWeights[IDa][3] + Weighta);
                        }
                        else if (spot > 3)
                        {
                            ForceSet(modmesh.vbo.BoneIDs2, IDa, spot - 4, i);
                            ForceSet(modmesh.vbo.BoneWeights2, IDa, spot - 4, Weighta);
                        }
                        else
                        {
                            ForceSet(modmesh.vbo.BoneIDs, IDa, spot, i);
                            ForceSet(modmesh.vbo.BoneWeights, IDa, spot, Weighta);
                        }
                    }
                }
                model.Meshes.Add(modmesh);
            }
            model.RootNode = new ModelNode() { Parent = null, Name = scene.RootNode.Name.ToLowerFast() };
            List<ModelNode> allNodes = new List<ModelNode>();
            PopulateChildren(model.RootNode, scene.RootNode, model, engine, allNodes);
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                for (int x = 0; x < scene.Meshes[i].Bones.Count; x++)
                {
                    ModelNode nodet = null;
                    string nl = scene.Meshes[i].Bones[x].Name.ToLowerFast();
                    for (int n = 0; n < allNodes.Count; n++)
                    {
                        if (allNodes[n].Name == nl)
                        {
                            nodet = allNodes[n];
                            break;
                        }
                    }
                    ModelBone mb = new ModelBone() { Offset = convert(scene.Meshes[i].Bones[x].MatrixA) };
                    nodet.Bones.Add(mb);
                    model.Meshes[i].Bones.Add(mb);
                }
            }
            return model;
        }

        Matrix4 convert(BEPUutilities.Matrix mat)
        {
            return ClientUtilities.Convert(mat);
        }

        void PopulateChildren(ModelNode node, Model3DNode orin, Model model, AnimationEngine engine, List<ModelNode> allNodes)
        {
            allNodes.Add(node);
            if (engine.HeadBones.Contains(node.Name))
            {
                node.Mode = 0;
            }
            else if (engine.LegBones.Contains(node.Name))
            {
                node.Mode = 2;
            }
            else
            {
                node.Mode = 1;
            }
            for (int i = 0; i < orin.Children.Count; i++)
            {
                ModelNode child = new ModelNode() { Parent = node, Name = orin.Children[i].Name.ToLowerFast() };
                PopulateChildren(child, orin.Children[i], model, engine, allNodes);
                node.Children.Add(child);
            }
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
        public Model3D Original;

        public Model(string _name)
        {
            Name = _name;
            Meshes = new List<ModelMesh>();
        }

        public Matrix4 Root;

        /// <summary>
        /// The name of  this model.
        /// </summary>
        public string Name;

        /// <summary>
        /// All the meshes this model has.
        /// </summary>
        public List<ModelMesh> Meshes;

        public ModelNode RootNode;

        public ModelMesh MeshFor(string name)
        {
            name = name.ToLowerFast();
            for (int i = 0; i < Meshes.Count; i++)
            {
                if (Meshes[i].Name.StartsWith(name))
                {
                    return Meshes[i];
                }
            }
            return null;
        }

        public void SetBones(Matrix4[] mats)
        {
            float[] set = new float[mats.Length * 16];
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
            GL.UniformMatrix4(41, mats.Length, false, set);
        }
        
        public Dictionary<string, Matrix4> CustomAnimationAdjustments = new Dictionary<string, Matrix4>();

        public bool ForceBoneNoOffset = false;
        
        public void UpdateTransforms(ModelNode pNode, Matrix4 transf)
        {
            string nodename = pNode.Name;
            Matrix4 nodeTransf = Matrix4.Identity;
            double time;
            SingleAnimationNode pNodeAnim = FindNodeAnim(nodename, pNode.Mode, out time);
            if (pNodeAnim != null)
            {
                BEPUutilities.Vector3 vec = pNodeAnim.lerpPos(time);
                BEPUutilities.Quaternion quat = pNodeAnim.lerpRotate(time);
                Quaternion oquat = new Quaternion((float)quat.X, (float)quat.Y, (float)quat.Z, (float)quat.W);
                Matrix4 trans;
                Matrix4.CreateTranslation((float)vec.X, (float)vec.Y, (float)vec.Z, out trans);
                trans.Transpose();
                Matrix4 rot;
                Matrix4.CreateFromQuaternion(ref oquat, out rot);
                Matrix4 r2;
                if (CustomAnimationAdjustments.TryGetValue(nodename, out r2))
                {
                    rot *= r2;
                }
                rot.Transpose();
                Matrix4.Mult(ref trans, ref rot, out nodeTransf);
            }
            else
            {
                Matrix4 temp;
                if (CustomAnimationAdjustments.TryGetValue(nodename, out temp))
                {
                    temp.Transpose();
                    nodeTransf = temp;
                }
            }
            Matrix4 global;
            Matrix4.Mult(ref transf, ref nodeTransf, out global);
            for (int i = 0; i < pNode.Bones.Count; i++)
            {
                if (ForceBoneNoOffset)
                {
                    pNode.Bones[i].Transform = global;
                }
                else
                {
                    Matrix4.Mult(ref global, ref pNode.Bones[i].Offset, out pNode.Bones[i].Transform);
                }
            }
            for (int i = 0; i < pNode.Children.Count; i++)
            {
                UpdateTransforms(pNode.Children[i], global);
            }
        }

        public ModelEngine Engine = null;

        SingleAnimationNode FindNodeAnim(string nodeName, int mode, out double time)
        {
            SingleAnimation nodes;
            if (mode == 0)
            {
                nodes = hAnim;
                time = aTHead;
            }
            else if (mode == 1)
            {
                nodes = tAnim;
                time = aTTorso;
            }
            else
            {
                nodes = lAnim;
                time = aTLegs;
            }
            if (nodes == null)
            {
                return null;
            }
            return nodes.GetNode(nodeName);
        }

        SingleAnimation hAnim;
        SingleAnimation tAnim;
        SingleAnimation lAnim;
        double aTHead;
        double aTTorso;
        double aTLegs;

        public double LastDrawTime;
        
        /// <summary>
        /// Draws the model.
        /// </summary>
        public void Draw(double aTimeHead = 0, SingleAnimation headanim = null, double aTimeTorso = 0, SingleAnimation torsoanim = null, double aTimeLegs = 0, SingleAnimation legsanim = null, bool forceBones = false)
        {
            LastDrawTime = Engine.cTime;
            hAnim = headanim;
            tAnim = torsoanim;
            lAnim = legsanim;
            bool any = hAnim != null || tAnim != null || lAnim != null || forceBones;
            if (any)
            {
                // globalInverse = Root.Inverted();
                aTHead = aTimeHead;
                aTTorso = aTimeTorso;
                aTLegs = aTimeLegs;
                UpdateTransforms(RootNode, Matrix4.Identity);
            }
            // TODO: If hasBones && !any { defaultBones() } ?
            for (int i = 0; i < Meshes.Count; i++)
            {
                if (any && Meshes[i].Bones.Count > 0)
                {
                    Matrix4[] mats = new Matrix4[Meshes[i].Bones.Count];
                    for (int x = 0; x < Meshes[i].Bones.Count; x++)
                    {
                        mats[x] = Meshes[i].Bones[x].Transform;
                    }
                    SetBones(mats);
                }
                Meshes[i].Draw();
            }
        }

        public bool Skinned = false;

        public void LoadSkin(TextureEngine texs)
        {
            if (Skinned)
            {
                return;
            }
            Skinned = true;
            if (Engine.TheClient.Files.Exists("models/" + Name + ".skin"))
            {
                string[] data = Engine.TheClient.Files.ReadText("models/" + Name + ".skin").SplitFast('\n');
                foreach (string datum in data)
                {
                    if (datum.Length > 0)
                    {
                        string[] datums = datum.SplitFast('=');
                        if (datums.Length == 2)
                        {
                            Texture tex = texs.GetTexture(datums[1]);
                            bool success = false;
                            string typer;
                            string datic = datums[0].BeforeAndAfter(":::", out typer);
                            typer = typer.ToLowerFast();
                            for (int i = 0; i < Meshes.Count; i++)
                            {
                                if (Meshes[i].Name == datic)
                                {
                                    if (typer == "specular")
                                    {
                                        Meshes[i].vbo.Tex_Specular = tex;
                                    }
                                    else if (typer == "reflectivity")
                                    {
                                        Meshes[i].vbo.Tex_Reflectivity = tex;
                                    }
                                    else if (typer == "normal")
                                    {
                                        Meshes[i].vbo.Tex_Normal = tex;
                                    }
                                    else if (typer == "")
                                    {
                                        Meshes[i].vbo.Tex = tex;
                                    }
                                    else
                                    {
                                        SysConsole.Output(OutputType.WARNING, "Unknown skin entry typer: '" + typer + "', expected reflectivity, specular, or simply no specification!");
                                    }
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
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "Can't find models/" + Name + ".skin!");
            }
        }

        public long GetVRAMUsage()
        {
            long ret = 0;
            foreach (ModelMesh mesh in Meshes)
            {
                ret += mesh.vbo.GetVRAMUsage();
            }
            return ret;
        }
    }

    public class ModelBone
    {
        public Matrix4 Transform = Matrix4.Identity;
        public Matrix4 Offset;
    }

    public class ModelNode
    {
        public ModelNode Parent = null;
        public List<ModelNode> Children = new List<ModelNode>();
        public List<ModelBone> Bones = new List<ModelBone>();
        public byte Mode;
        public string Name;
    }

    public class ModelMesh
    {
        /// <summary>
        /// The name of this mesh.
        /// </summary>
        public string Name;
        
        public List<ModelBone> Bones = new List<ModelBone>();

        public ModelMesh(string _name)
        {
            Name = _name.ToLowerFast();
            if (Name.EndsWith(".001"))
            {
                Name = Name.Substring(0, Name.Length - ".001".Length);
            }
            Faces = new List<ModelFace>();
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
            VBOGenned = true;
        }

        public bool VBOGenned = false;

        /// <summary>
        /// Renders the mesh.
        /// </summary>
        public void Draw()
        {
            if (!VBOGenned)
            {
                GenerateVBO();
            }
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
