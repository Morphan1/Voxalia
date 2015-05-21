using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShadowOperations.Shared
{
    public class AnimationEngine
    {
        public AnimationEngine()
        {
            Animations = new Dictionary<string, SingleAnimation>();
            TorsoBones = new string[] { "hips", "spine", "chest", "chest1", "shoulder.l", "upper_arm.l", "forearm.l", "hand.l",
                "thumb.01.l", "thumb.02.l", "thumb.03.l", "f_index.01.l", "f_index.02.l", "f_index.03.l", "f_middle.01.l", "f_middle.02.l", "f_middle.03.l",
            "f_pinky.01.l", "f_pinky.02.l", "f_pinky.03.l", "f_ring.01.l", "f_ring.02.l", "f_ringy.03.l",
            "shoulder.r", "upper_arm.r", "forearm.r", "hand.r", "thumb.01.r", "thumb.02.r", "thumb.03.r", "f_index.01.r", "f_index.02.r", "f_index.03.r",
            "f_middle.01.r", "f_middle.02.r", "f_middle.03.r", "f_pinky.01.r", "f_pinky.02.r", "f_pinky.03.r", "f_ring.01.r", "f_ring.02.r", "f_ringy.03.r" };
            HeadBones = new string[] { "neck", "head", "jaw", "tongue_base", "tongue_mod", "tongue_tip", "lolid.l", "lolid.r", "uplid.l", "uplid.r", "eye.l", "eye.r" };
            LegBones = new string[] { "thigh.l", "shin.l", "foot.l", "toe.l", "thigh.r", "shin.r", "foot.r", "toe.r" };
        }

        public string[] HeadBones;
        public string[] TorsoBones;
        public string[] LegBones;

        public Dictionary<string, SingleAnimation> Animations;

        public SingleAnimation GetAnimation(string name)
        {
            string namelow = name.ToLower();
            SingleAnimation sa;
            if (Animations.TryGetValue(namelow, out sa))
            {
                return sa;
            }
            try
            {
                sa = LoadAnimation(namelow);
                Animations.Add(sa.Name, sa);
                return sa;
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Loading an animation: " + ex.ToString());
                sa = new SingleAnimation() { Name = namelow, Length = 1, Engine = this };
                Animations.Add(sa.Name, sa);
                return sa;
            }
        }


        SingleAnimation LoadAnimation(string name)
        {
            if (Program.Files.Exists("animations/" + name + ".anim"))
            {
                SingleAnimation created = new SingleAnimation();
                created.Name = name;
                string[] data = Program.Files.ReadText("animations/" + name + ".anim").Split('\n');
                int entr = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i].StartsWith("//"))
                    {
                        continue;
                    }
                    string type = data[i];
                    if (data.Length <= i + 1 || data[i + 1] != "{")
                    {
                        break;
                    }
                    List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>();
                    for (i += 2; i < data.Length; i++)
                    {
                        if (data[i].Trim().StartsWith("//"))
                        {
                            continue;
                        }
                        if (data[i] == "}")
                        {
                            break;
                        }
                        string[] dat = data[i].Split(':');
                        if (dat.Length <= 1)
                        {
                            SysConsole.Output(OutputType.WARNING, "Invalid key dat: " + dat[0]);
                        }
                        else
                        {
                        string key = dat[0].Trim();
                        string value = dat[1].Substring(0, dat[1].Length - 1).Trim();
                        entries.Add(new KeyValuePair<string, string>(key, value));
                        }
                    }
                    bool isgeneral = type == "general" && entr == 0;
                    SingleAnimationNode node = null;
                    if (!isgeneral)
                    {
                        node = new SingleAnimationNode();
                        node.Name = type.ToLower();
                    }
                    foreach (KeyValuePair<string, string> entry in entries)
                    {
                        if (isgeneral)
                        {
                            if (entry.Key == "length")
                            {
                                created.Length = Utilities.StringToDouble(entry.Value);
                            }
                            else
                            {
                                SysConsole.Output(OutputType.WARNING, "Unknown GENERAL key: " + entry.Key);
                            }
                        }
                        else
                        {
                            if (entry.Key == "positions")
                            {
                                string[] poses = entry.Value.Split(' ');
                                for (int x = 0; x < poses.Length; x++)
                                {
                                    if (poses[x].Length > 0)
                                    {
                                        string[] posdata = poses[x].Split('=');
                                        node.PosTimes.Add(Utilities.StringToDouble(posdata[0]));
                                        node.Positions.Add(new Location(Utilities.StringToFloat(posdata[1]),
                                            Utilities.StringToFloat(posdata[2]), Utilities.StringToFloat(posdata[3])));
                                    }
                                }
                            }
                            else if (entry.Key == "rotations")
                            {
                                string[] rots = entry.Value.Split(' ');
                                for (int x = 0; x < rots.Length; x++)
                                {
                                    if (rots[x].Length > 0)
                                    {
                                        string[] posdata = rots[x].Split('=');
                                        node.RotTimes.Add(Utilities.StringToDouble(posdata[0]));
                                        node.Rotations.Add(new Assimp.Quaternion(Utilities.StringToFloat(posdata[4]), Utilities.StringToFloat(posdata[1]),
                                            Utilities.StringToFloat(posdata[2]), Utilities.StringToFloat(posdata[3])));
                                    }
                                }
                            }
                            else
                            {
                                SysConsole.Output(OutputType.WARNING, "Unknown NODE key: " + entry.Key);
                            }
                        }
                    }
                    if (!isgeneral)
                    {
                        created.Nodes.Add(node);
                    }
                    entr++;
                }
                created.Engine = this;
                return created;
            }
            else
            {
                throw new Exception("Invalid animation file - file not found: animations/" + name + ".anim");
            }
        }
    }

    public class SingleAnimation
    {
        public string Name;

        public double Length;

        public AnimationEngine Engine;

        public List<SingleAnimationNode> Nodes = new List<SingleAnimationNode>();
    }

    public class SingleAnimationNode
    {
        public string Name;

        public List<double> PosTimes = new List<double>();

        public List<Location> Positions = new List<Location>();

        public List<double> RotTimes = new List<double>();

        public List<Assimp.Quaternion> Rotations = new List<Assimp.Quaternion>();
    }
}
