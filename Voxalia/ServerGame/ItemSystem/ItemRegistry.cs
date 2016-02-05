using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;
using Voxalia.Shared.Files;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;

namespace Voxalia.ServerGame.ItemSystem
{
    public class ItemRegistry
    {
        public Dictionary<string, ItemStack> BaseItems = new Dictionary<string, ItemStack>();

        public ItemStack Air;

        public Server TheServer;

        public ItemRegistry(Server tserver)
        {
            TheServer = tserver;
            Air = new ItemStack("air", TheServer, 0, "clear", "Air", "Empty air", System.Drawing.Color.White, "none", true);
            BaseItems.Add("air", Air);
        }

        public ItemStack GetItem(string name)
        {
            string low = name.ToLowerInvariant();
            ItemStack ist;
            if (BaseItems.TryGetValue(low, out ist))
            {
                return ist.Duplicate();
            }
            ist = Load(low);
            if (ist == null)
            {
                return Air;
            }
            BaseItems.Add(low, ist);
            return ist.Duplicate();
        }

        private ItemStack Load(string name)
        {
            string fname = "items/" + name + ".itm";
            if (!Program.Files.Exists(fname))
            {
                SysConsole.Output(OutputType.WARNING, "Tried to load non-existent item: " + name);
                return null;
            }
            try
            {
                string fdata = Program.Files.ReadText(fname);
                string[] split = fdata.Replace('\r', '\n').Split('\n');
                string res_type = "";
                string res_icon = "";
                string res_display = "";
                string res_description = "";
                string res_color = "";
                string res_model = "";
                string res_bound = "";
                string res_subtype = null;
                string res_datum = "0";
                List<KeyValuePair<string, string>> attrs = new List<KeyValuePair<string, string>>();
                List<KeyValuePair<string, float>> shared = new List<KeyValuePair<string, float>>();
                foreach (string line in split)
                {
                    if (line.Trim().Length < 3)
                    {
                        continue;
                    }
                    string[] dat = line.Split(new char[] { ':' }, 2);
                    string dat_type = dat[0].Trim().ToLowerInvariant();
                    string dat_val = dat[1].Trim();
                    switch (dat_type)
                    {
                        case "type":
                            res_type = dat_val;
                            break;
                        case "subtype":
                            res_subtype = dat_val;
                            break;
                        case "icon":
                            res_icon = dat_val;
                            break;
                        case "display":
                            res_display = dat_val;
                            break;
                        case "description":
                            res_description = dat_val;
                            break;
                        case "color":
                            res_color = dat_val;
                            break;
                        case "model":
                            res_model = dat_val;
                            break;
                        case "bound":
                            res_bound = dat_val;
                            break;
                        case "datum":
                            res_datum = dat_val;
                            break;
                        default:
                            if (dat_type.StartsWith("shared."))
                            {
                                string opt = dat_type.Substring("shared.".Length).ToLower();
                                shared.Add(new KeyValuePair<string, float>(opt, Utilities.StringToFloat(dat_val)));
                            }
                            else if (dat_type.StartsWith("attributes."))
                            {
                                string opt = dat_type.Substring("attributes.".Length).ToLower();
                                attrs.Add(new KeyValuePair<string, string>(opt, dat_val));
                            }
                            break;
                    }
                }
                ItemStack it = new ItemStack(res_type, res_subtype, TheServer, 1, res_icon, res_display, res_description, ColorTag.For(res_color).Internal, res_model, res_bound.ToLower() == "true")
                {
                    Datum = Utilities.StringToInt(res_datum)
                };
                foreach (KeyValuePair<string, float> key in shared)
                {
                    it.SharedAttributes[key.Key] = key.Value;
                }
                foreach (KeyValuePair<string, string> key in attrs)
                {
                    it.Attributes[key.Key] = key.Value;
                }
                return it;
            }
            catch (Exception ex)
            {
                SysConsole.Output("Loading item '" + name + "'", ex);
                return null;
            }
        }
    }
}
