using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;
using Voxalia.Shared.Files;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Common;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagBases;
using Voxalia.ServerGame.TagSystem.TagObjects;
using FreneticScript;

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

        public ItemStack GetItem(string name, int count = 1)
        {
            string low = name.ToLowerFast();
            ItemStack ist;
            if (BaseItems.TryGetValue(low, out ist))
            {
                ist = ist.Duplicate();
                ist.Count = count;
                return ist;
            }
            ist = Load(low);
            if (ist == null)
            {
                return Air;
            }
            BaseItems.Add(low, ist);
            ist = ist.Duplicate();
            ist.Count = count;
            return ist;
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
                string[] split = fdata.Replace('\r', '\n').SplitFast('\n');
                string res_type = "";
                string res_icon = "";
                string res_display = "";
                string res_description = "";
                string res_color = "";
                string res_model = "";
                string res_bound = "";
                string res_subtype = null;
                string res_datum = "0";
                string res_weight = "1";
                string res_volume = "1";
                List<KeyValuePair<string, string>> attrs = new List<KeyValuePair<string, string>>();
                List<KeyValuePair<string, string>> shared = new List<KeyValuePair<string, string>>();
                foreach (string line in split)
                {
                    if (line.Trim().Length < 3)
                    {
                        continue;
                    }
                    string[] dat = line.SplitFast(':', 2);
                    string dat_type = dat[0].Trim().ToLowerFast();
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
                        case "weight":
                            res_weight = dat_val;
                            break;
                        case "volume":
                            res_volume = dat_val;
                            break;
                        default:
                            if (dat_type.StartsWith("shared."))
                            {
                                string opt = dat_type.Substring("shared.".Length).ToLower();
                                shared.Add(new KeyValuePair<string, string>(opt, dat_val));
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
                    Datum = Utilities.StringToInt(res_datum),
                    Weight = Utilities.StringToFloat(res_weight),
                    Volume = Utilities.StringToFloat(res_volume)
                };
                foreach (KeyValuePair<string, string> key in shared)
                {
                    string dat = UnescapeTagBase.Unescape(key.Value);
                    string type = dat.Substring(0, 4);
                    string content = dat.Substring(5);
                    TemplateObject togive = ItemStack.TOFor(TheServer, type, content);
                    it.SharedAttributes[key.Key] = togive;
                }
                foreach (KeyValuePair<string, string> key in attrs)
                {
                    string dat = UnescapeTagBase.Unescape(key.Value);
                    string type = dat.Substring(0, 4);
                    string content = dat.Substring(5);
                    TemplateObject togive = ItemStack.TOFor(TheServer, type, content);
                    it.Attributes[key.Key] = togive;
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
