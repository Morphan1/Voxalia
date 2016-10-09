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
using System.IO;
using Assimp;
using Assimp.Configs;

namespace SkeletalAnimationExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] strs = Directory.GetFiles(Environment.CurrentDirectory, "*.dae", SearchOption.TopDirectoryOnly);
            foreach (string path in strs)
            {
                try
                {
                    using (AssimpContext ACont = new AssimpContext())
                    {
                        ACont.SetConfig(new NormalSmoothingAngleConfig(66f));
                        Scene scene = ACont.ImportFile(path, PostProcessSteps.Triangulate);
                        if (!scene.HasAnimations)
                        {
                            Console.WriteLine("Skipping " + path);
                            continue;
                        }
                        Animation anim = scene.Animations[0];
                        if (!anim.HasNodeAnimations)
                        {
                            Console.WriteLine("Invalid animation in " + path);
                            continue;
                        }
                        StringBuilder sb = new StringBuilder();
                        sb.Append("// mcmonkey's animation details file format v0.2\n");
                        sb.Append("general\n");
                        sb.Append("{\n");
                        sb.Append("\tlength: ").Append((anim.DurationInTicks / anim.TicksPerSecond).ToString()).Append(";\n");
                        sb.Append("}\n");
                        for (int i = 0; i < anim.NodeAnimationChannelCount; i++)
                        {
                            NodeAnimationChannel node = anim.NodeAnimationChannels[i];
                            sb.Append(node.NodeName).Append('\n');
                            sb.Append("{\n");
                            Node found = LookForMatch(scene.RootNode, node.NodeName);
                            string pNode = "<none>";
                            if (found != null)
                            {
                                pNode = found.Parent.Name;
                            }
                            sb.Append("\tparent: " + pNode + ";\n");
                            /*Bone bfound = LookForBone(scene, node.NodeName);
                            Matrix4x4 mat = Matrix4x4.Identity;
                            if (bfound != null)
                            {
                                mat = bfound.OffsetMatrix;
                                //bpos = bfound.OffsetMatrix.C1 + "=" + bfound.OffsetMatrix.C2 + "=" + bfound.OffsetMatrix.C3;
                                //bpos = bfound.OffsetMatrix.A4 + "=" + bfound.OffsetMatrix.B4 + "=" + bfound.OffsetMatrix.C4;
                            }*/
                            /*sb.Append("\toffset: " + mat.A1 + "=" + mat.A2 + "=" + mat.A3 + "=" + mat.A4 + "=" +
                                mat.B1 + "=" + mat.B2 + "=" + mat.B3 + "=" + mat.B4 + "=" +
                                mat.C1 + "=" + mat.C2 + "=" + mat.C3 + "=" + mat.C4 + "=" +
                                mat.D1 + "=" + mat.D2 + "=" + mat.D3 + "=" + mat.D4 +";\n");*/
                            if (node.HasPositionKeys)
                            {
                                sb.Append("\tpositions: ");
                                for (int x = 0; x < node.PositionKeyCount; x++)
                                {
                                    VectorKey key = node.PositionKeys[x];
                                    sb.Append(key.Time.ToString() + "=" + key.Value.X.ToString() + "=" + key.Value.Y.ToString() + "=" + key.Value.Z.ToString() + " ");
                                }
                                sb.Append(";\n");
                            }
                            if (node.HasRotationKeys)
                            {
                                sb.Append("\trotations: ");
                                for (int x = 0; x < node.RotationKeyCount; x++)
                                {
                                    QuaternionKey key = node.RotationKeys[x];
                                    sb.Append(key.Time.ToString() + "=" + key.Value.X.ToString() + "=" + key.Value.Y.ToString() + "=" + key.Value.Z.ToString() + "=" + key.Value.W + " ");
                                }
                                sb.Append(";\n");
                            }
                            // TODO: Should scaling matter? Probably not.
                            sb.Append("}\n");
                        }
                        File.WriteAllText(path + ".anim", sb.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Processing " + path + ": " + ex.ToString());
                }
            }
        }

        static Node LookForMatch(Node root, string name)
        {
            string namelow = name.ToLower();
            if (root.Name.ToLower() == namelow)
            {
                return root;
            }
            for (int i = 0; i < root.ChildCount; i++)
            {
                Node found = LookForMatch(root.Children[i], name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        static Bone LookForBone(Scene root, string name)
        {
            string namelow = name.ToLower();
            for (int i = 0; i < root.Meshes.Count; i++)
            {
                for (int x = 0; x < root.Meshes[i].Bones.Count; x++)
                {
                    if (root.Meshes[i].Bones[x].Name.ToLower() == namelow)
                    {
                        return root.Meshes[i].Bones[x];
                    }
                }
            }
            return null;
        }
    }
}
