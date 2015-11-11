using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using YamlDotNet.Serialization;

namespace Voxalia.Shared
{
    public class YAMLConfiguration
    {
        public YAMLConfiguration(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                Data = new Dictionary<object, object>();
            }
            else
            {
                Deserializer des = new Deserializer();
                Data = des.Deserialize<Dictionary<object, object>>(new StringReader(input));
            }
        }

        public YAMLConfiguration(Dictionary<object, object> datas)
        {
            Data = datas;
        }

        public Dictionary<object, object> Data;

        public bool IsList(string path)
        {
            List<object> res = ReadList(path);
            return res != null;
        }

        public List<string> ReadStringList(string path)
        {
            List<object> data = ReadList(path);
            if (data == null)
            {
                return null;
            }
            List<string> ndata = new List<string>(data.Count);
            for (int i = 0; i < data.Count; i++)
            {
                ndata.Add(data[i] + "");
            }
            return ndata;
        }

        public List<object> ReadList(string path)
        {
            string[] data = path.Split('.');
            int i = 0;
            dynamic obj = Data;
            while (i < data.Length - 1)
            {
                dynamic nobj = obj.ContainsKey(data[i]) ? obj[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return null;
                }
                obj = nobj;
                i++;
            }
            if (!obj.ContainsKey(data[i]) || !(obj[data[i]] is List<string> || obj[data[i]] is List<object>))
            {
                return null;
            }
            if (obj[data[i]] is List<object>)
            {
                List<object> objs = (List<object>)obj[data[i]];
                return objs;
            }
            return null;
        }

        public float ReadFloat(string path, float def)
        {
            return (float)ReadDouble(path, def);
        }

        public double ReadDouble(string path, double def)
        {
            string[] data = path.Split('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                // TODO: TryGetValue?
                object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return def;
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                return def;
            }
            Object iobj = ((Dictionary<object, object>)obj)[data[i]];
            if (iobj is Double)
            {
                return (Double)iobj;
            }
            if (iobj is Single)
            {
                return (Single)iobj;
            }
            if (iobj is Int64)
            {
                return (double)((Int64)iobj);
            }
            if (iobj is Int32)
            {
                return (double)((Int32)iobj);
            }
            double xtemp;
            if (double.TryParse(iobj.ToString(), out xtemp))
            {
                return xtemp;
            }
            return def;
        }

        public int ReadInt(string path, int def)
        {
            return (int)ReadLong(path, def);
        }

        public long ReadLong(string path, long def)
        {
            string[] data = path.Split('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                // TODO: TryGetValue?
                object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return def;
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                return def;
            }
            Object iobj = ((Dictionary<object, object>)obj)[data[i]];
            if (iobj is Int64)
            {
                return (Int64)iobj;
            }
            if (iobj is Int32)
            {
                return (Int32)iobj;
            }
            if (iobj is Double)
            {
                return (long)((Double)iobj);
            }
            if (iobj is Single)
            {
                return (long)((Single)iobj);
            }
            long xtemp;
            if (long.TryParse(iobj.ToString(), out xtemp))
            {
                return xtemp;
            }
            return def;
        }

        public string ReadString(string path, string def)
        {
            object obj = Read(path, def);
            if (obj == null)
            {
                return def;
            }
            return obj.ToString();
        }

        public object Read(string path, object def)
        {
            string[] data = path.Split('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                // TODO: TryGetValue?
                object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return def;
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                return def;
            }
            return ((Dictionary<object, object>)obj)[data[i]];
        }

        public bool HasKey(string path, string key)
        {
            return GetKeys(path).Contains(key);
        }

        public List<string> GetKeys(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                List<string> atemp = new List<string>();
                foreach (object xtobj in Data.Keys)
                {
                    atemp.Add(xtobj + "");
                }
                return atemp;
            }
            string[] data = path.Split('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                dynamic nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return new List<string>();
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                return new List<string>();
            }
            object tobj = ((Dictionary<object, object>)obj)[data[i]];
            if (tobj is Dictionary<object, object>)
            {
                Dictionary<object, object>.KeyCollection objs = ((Dictionary<object, object>)tobj).Keys;
                List<string> toret = new List<string>();
                foreach (object o in objs)
                {
                    toret.Add(o + "");
                }
                return toret;
            }
            if (!(tobj is Dictionary<string, dynamic> || tobj is Dictionary<string, object>))
            {
                return new List<string>();
            }
            List<string> temp = new List<string>();
            foreach (object xtobj in ((Dictionary<object, object>)tobj).Keys)
            {
                temp.Add(xtobj + "");
            }
            return temp;
        }

        public YAMLConfiguration GetConfigurationSection(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new YAMLConfiguration(Data);
            }
            string[] data = path.Split('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                // TODO: TryGetValue?
                object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return null;
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                return null;
            }
            object tobj = ((Dictionary<object, object>)obj)[data[i]];
            if (!(tobj is Dictionary<object, object>))
            {
                return null;
            }
            return new YAMLConfiguration((Dictionary<object, object>)tobj);
        }

        public void Default(string path, object val)
        {
            string[] data = path.Split('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                dynamic nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    nobj = new Dictionary<dynamic, dynamic>();
                    ((Dictionary<object, object>)obj)[data[i]] = nobj;
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                ((Dictionary<object, object>)obj)[data[i]] = val;
                if (Changed != null)
                {
                    Changed.Invoke(this, new EventArgs());
                }
            }
        }

        public void Set(string path, object val)
        {
            string[] data = path.Split('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                dynamic nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    nobj = new Dictionary<dynamic, dynamic>();
                    ((Dictionary<object, object>)obj)[data[i]] = nobj;
                }
                obj = nobj;
                i++;
            }
            if (val == null)
            {
                ((Dictionary<object, object>)obj).Remove(data[i]);
            }
            else
            {
                ((Dictionary<object, object>)obj)[data[i]] = val;
            }
            if (Changed != null)
            {
                Changed.Invoke(this, new EventArgs());
            }
        }

        public EventHandler Changed;

        public string SaveToString()
        {
            Serializer ser = new Serializer();
            StringWriter sw = new StringWriter();
            ser.Serialize(sw, Data);
            return sw.ToString();
        }
    }
}
