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
        }

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
                sa = new SingleAnimation() { Name = namelow, Length = 1 };
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
                        node.Name = type;
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
                    entr++;
                }
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
