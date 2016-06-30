using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.Shared.Files;
using FreneticScript;

namespace Voxalia.Shared
{
    public class LanguageEngine
    {
        public Dictionary<string, YAMLConfiguration> EnglishDocuments = new Dictionary<string, YAMLConfiguration>();

        public Dictionary<string, YAMLConfiguration> LanguageDocuments = new Dictionary<string, YAMLConfiguration>();

        public string CurrentLanguage = "en_us";

        public void SetLanguage(string language)
        {
            CurrentLanguage = language.ToLowerFast();
            LanguageDocuments.Clear();
        }

        public YAMLConfiguration GetLangDoc(string id, string lang = null, Dictionary<string, YAMLConfiguration> confs = null)
        {
            if (lang == null)
            {
                lang = CurrentLanguage;
            }
            if (confs == null)
            {
                confs = LanguageDocuments;
            }
            string idlow = id.ToLowerFast();
            YAMLConfiguration doc;
            if (LanguageDocuments.TryGetValue(idlow, out doc))
            {
                return doc;
            }
            string path = "info/text/" + idlow + "_" + lang + ".yml";
            if (Program.Files.Exists(path))
            {
                try
                {
                    string dat = Program.Files.ReadText(path);
                    doc = new YAMLConfiguration(dat);
                    LanguageDocuments[idlow] = doc;
                    return doc;
                }
                catch (Exception ex)
                {
                    Utilities.CheckException(ex);
                    SysConsole.Output("Reading language documents", ex);
                }
            }
            LanguageDocuments[idlow] = null;
            return null;
        }

        const string badkey = "common.languages.badkey";

        public string Handle(string info, string[] pathAndVars)
        {
            info = info.Replace('\r', '\n').Replace("\n", "");
            for (int i = 2; i < pathAndVars.Length; i++)
            {
                info = info.Replace("{{" + (i - 1).ToString() + "}}", pathAndVars[i]);
            }
            return info;
        }

        public string GetText(params string[] pathAndVars)
        {
            if (pathAndVars.Length < 2)
            {
                return GetText("voxalia", "common.languages.badinput");
            }
            string category = pathAndVars[0].ToLowerFast();
            string defPath = pathAndVars[1].ToLowerFast();
            YAMLConfiguration lang = GetLangDoc(category);
            YAMLConfiguration langen = GetLangDoc(category, "en_us", EnglishDocuments);
            string str = null;
            if (lang != null)
            {
                str = lang.ReadString(defPath, null);
                if (str != null)
                {
                    return Handle(str, pathAndVars);
                }
            }
            if (langen != null)
            {
                str = langen.ReadString(defPath, null);
                if (str != null)
                {
                    return Handle(str, pathAndVars);
                }
            }
            if (defPath == badkey)
            {
                return "((Invalid key!))";
            }
            return GetText("voxalia", badkey);
        }
    }
}
