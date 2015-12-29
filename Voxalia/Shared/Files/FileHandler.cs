using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.IO.Compression;

namespace Voxalia.Shared.Files
{
    /// <summary>
    /// Handles the file system cleanly.
    /// </summary>
    public class FileHandler
    {
        public List<PakFile> Paks = new List<PakFile>();

        public List<PakkedFile> Files = new List<PakkedFile>();

        /// <summary>
        /// The default text encoding.
        /// </summary>
        public static Encoding encoding = new UTF8Encoding(false);

        /// <summary>
        /// The base directory in which all data is stored.
        /// </summary>
        public string BaseDirectory = Environment.CurrentDirectory.Replace('\\', '/') + "/data/";

        public List<string> SubDirectories = new List<string>();

        public void LoadDir(string dir)
        {
            dir = dir.Replace('\\', '/');
            dir = dir.Replace("..", "__error__");
            string fdir = Environment.CurrentDirectory.Replace('\\', '/') + "/" + dir.ToLowerInvariant() + "/";
            if (SubDirectories.Contains(fdir))
            {
                SysConsole.Output(OutputType.WARNING, "Ignoring attempt to add same directory twice.");
                return;
            }
            SubDirectories.Add(fdir);
            foreach (PakFile pf in Paks)
            {
                pf.Storer.Dispose();
            }
            Paks.Clear();
            Files.Clear();
            Init();
        }

        public void Init()
        {
            foreach (string str in SubDirectories)
            {
                load(str, Directory.GetFiles(str, "*.*", SearchOption.AllDirectories));
            }
            load(BaseDirectory, Directory.GetFiles(BaseDirectory, "*.*", SearchOption.AllDirectories));
        }

        void load(string pth, string[] allfiles)
        {
            foreach (string tfile in allfiles)
            {
                string file = tfile.Replace('\\', '/');
                if (file.Length == 0 || file[file.Length - 1] == '/')
                {
                    continue;
                }
                if (file.EndsWith(".pak"))
                {
                    Paks.Add(new PakFile(file.Replace(pth, "").ToLowerInvariant(), file));
                }
                else
                {
                    Files.Add(new PakkedFile(file.Replace(pth, "").ToLowerInvariant(), file));
                }
            }
            int id = 0;
            foreach (PakFile pak in Paks)
            {
                List<ZipStorer.ZipFileEntry> zents = pak.Storer.ReadCentralDir();
                pak.FileListIndex = Files.Count;
                foreach (ZipStorer.ZipFileEntry zent in zents)
                {
                    string name = zent.FilenameInZip.Replace('\\', '/').Replace("..", ".").Replace(":", "").ToLowerInvariant();
                    if (name.Length == 0 || name[name.Length - 1] == '/')
                    {
                        continue;
                    }
                    Files.Add(new PakkedFile(name, "", id, zent));
                }
                id++;
            }
        }

        /// <summary>
        /// Cleans a file name for direct system calls.
        /// </summary>
        /// <param name="input">The original file name.</param>
        /// <returns>The cleaned file name.</returns>
        public static string CleanFileName(string input)
        {
            StringBuilder output = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                // Remove double slashes, or "./"
                if ((input[i] == '/' || input[i] == '\\') && output.Length > 0 && (output[output.Length - 1] == '/' || output[output.Length - 1] == '.'))
                {
                    continue;
                }
                // Fix backslashes to forward slashes for cross-platform folders
                if (input[i] == '\\')
                {
                    output.Append('/');
                    continue;
                }
                // Remove ".." (up-a-level) folders, or "/."
                if (input[i] == '.' && output.Length > 0 && (output[output.Length - 1] == '.' || output[output.Length - 1] == '/'))
                {
                    continue;
                }
                // Clean spaces to underscores
                if (input[i] == (char)0x00A0 || input[i] == ' ')
                {
                    output.Append('_');
                    continue;
                }
                // Remove non-ASCII symbols, ASCII control codes, and Windows control symbols
                if (input[i] < 32 || input[i] > 126 || input[i] == '?' ||
                    input[i] == ':' || input[i] == '*' || input[i] == '|' ||
                    input[i] == '"' || input[i] == '<' || input[i] == '>')
                {
                    output.Append('_');
                    continue;
                }
                // Lower-case letters only
                if (input[i] >= 'A' && input[i] <= 'Z')
                {
                    output.Append((char)(input[i] - ('A' - 'a')));
                    continue;
                }
                // All others normal
                output.Append(input[i]);
            }
            // Limit length
            if (output.Length > 100)
            {
                // Also, trim leading/trailing spaces.
                return output.ToString().Substring(0, 100).Trim();
            }
            // Also, trim leading/trailing spaces.
            return output.ToString().Trim();
        }

        public int FileIndex(string filename)
        {
            string cleaned = CleanFileName(filename);
            for (int i = 0; i < Files.Count; i++)
            {
                if (Files[i].Name == cleaned)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns whether a file exists.
        /// </summary>
        /// <param name="filename">The name of the file to look for.</param>
        /// <returns>Whether the file exists.</returns>
        public bool Exists(string filename)
        {
            string cleaned = CleanFileName(filename);
            return FileIndex(cleaned) != -1 || File.Exists(BaseDirectory + cleaned);
        }

        /// <summary>
        /// Returns all the byte data in a file.
        /// </summary>
        /// <param name="filename">The name of the file to read.</param>
        /// <returns>The file's data, as a byte array.</returns>
        public byte[] ReadBytes(string filename)
        {
            string fname = CleanFileName(filename);
            int ind = FileIndex(fname);
            if (ind == -1)
            {
                if (File.Exists(BaseDirectory + fname))
                {
                    return File.ReadAllBytes(BaseDirectory + fname);
                }
                else
                {
                    throw new UnknownFileException(fname);
                }
            }
            PakkedFile file = Files[ind];
            if (file.IsPakked)
            {
                MemoryStream ms = new MemoryStream();
                Paks[file.PakIndex].Storer.ExtractFile(file.Entry, ms);
                byte[] toret = ms.ToArray();
                ms.Close();
                return toret;
            }
            else
            {
                return File.ReadAllBytes(file.Handle);
            }
        }

        /// <summary>
        /// Returns a stream of the byte data in a file.
        /// </summary>
        /// <param name="filename">The name of the file to read.</param>
        /// <returns>The file's data, as a stream.</returns>
        public DataStream ReadToStream(string filename)
        {
            return new DataStream(ReadBytes(filename));
        }

        /// <summary>
        /// Returns all the text data in a file.
        /// </summary>
        /// <param name="filename">The name of the file to read.</param>
        /// <returns>The file's data, as a string.</returns>
        public string ReadText(string filename)
        {
            return encoding.GetString(ReadBytes(filename)).Replace("\r", "");
        }
        
        /// <summary>
        /// Returns a list of all folders that contain the filepath.
        /// </summary>
        public List<string> ListFolders(string filepath)
        {
            List<string> folds = new List<string>();
            string fname = "/" + CleanFileName("/" + filepath);
            while (fname.Contains("//"))
            {
                fname = fname.Replace("//", "/");
            }
            if (fname.EndsWith("/"))
            {
                fname = fname.Substring(0, fname.Length - 1);
            }
            string fn2 = fname + "/";
            if (fn2 == "//")
            {
                fn2 = "/";
            }
            for (int i = 0; i < Files.Count; i++)
            {
                string fina = "/" + Files[i].Name;
                if (fina.StartsWith(fn2))
                {
                    string fold = "/" + (Files[i].Name.LastIndexOf('/') <= 0 ? "": Files[i].Name.Substring(0, Files[i].Name.LastIndexOf('/')));
                    if (fold.StartsWith(fn2) && !folds.Contains(fold))
                    {
                        folds.Add(fold);
                    }
                }
            }
            return folds;
        }

        /// <summary>
        /// Writes bytes to a file.
        /// </summary>
        /// <param name="filename">The name of the file to write to.</param>
        /// <param name="bytes">The byte data to write.</param>
        public void WriteBytes(string filename, byte[] bytes)
        {
            string fname = CleanFileName(filename);
            string finname;
            if (SubDirectories.Count > 0)
            {
                finname = SubDirectories[0] + fname;
            }
            else
            {
                finname = BaseDirectory + fname;
            }
            string dir = Path.GetDirectoryName(finname);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllBytes(finname, bytes);
            for (int i = 0; i < Files.Count; i++)
            {
                if (Files[i] == null)
                {
                    // TODO: Find out why this can happen
                    continue;
                }
                if (Files[i].Handle == finname)
                {
                    return;
                }
            }
            // TODO: Files.Insert(0, new PakkedFile(fname, finname));
        }

        /// <summary>
        /// Writes text to a file.
        /// </summary>
        /// <param name="filename">The name of the file to write to.</param>
        /// <param name="text">The text data to write.</param>
        public void WriteText(string filename, string text)
        {
            WriteBytes(filename, encoding.GetBytes(text.Replace('\r', ' ')));
        }

        /// <summary>
        /// Adds text to a file.
        /// </summary>
        /// <param name="filename">The name of the file to add to.</param>
        /// <param name="text">The text data to add.</param>
        public void AppendText(string filename, string text)
        {
            string textoutput = ReadText(filename);
            WriteText(filename, textoutput + text);
        }

        /// <summary>
        /// Compresses a byte array using the GZip algorithm.
        /// </summary>
        /// <param name="input">Uncompressed data.</param>
        /// <returns>Compressed data.</returns>
        public static byte[] GZip(byte[] input)
        {
            MemoryStream memstream = new MemoryStream();
            GZipStream GZStream = new GZipStream(memstream, CompressionMode.Compress);
            GZStream.Write(input, 0, input.Length);
            GZStream.Close();
            byte[] finaldata = memstream.ToArray();
            memstream.Close();
            return finaldata;
        }

        /// <summary>
        /// Decompress a byte array using the GZip algorithm.
        /// </summary>
        /// <param name="input">Compressed data.</param>
        /// <returns>Uncompressed data.</returns>
        public static byte[] UnGZip(byte[] input)
        {
            using (MemoryStream output = new MemoryStream())
            {
                MemoryStream memstream = new MemoryStream(input);
                GZipStream GZStream = new GZipStream(memstream, CompressionMode.Decompress);
                GZStream.CopyTo(output);
                GZStream.Close();
                memstream.Close();
                return output.ToArray();
            }
        }
    }

    public class PakkedFile
    {
        public string Name = null;

        public string Handle = null;

        public bool IsPakked = false;

        public int PakIndex = -1;

        public ZipStorer.ZipFileEntry Entry;

        public PakkedFile(string name, string handle)
        {
            Name = name;
            Handle = handle;
        }

        public PakkedFile(string name, string handle, int index, ZipStorer.ZipFileEntry entry)
        {
            Name = name;
            Handle = handle;
            IsPakked = true;
            PakIndex = index;
            Entry = entry;
        }
    }

    public class PakFile
    {
        public string Name = null;
        public string Handle = null;
        public ZipStorer Storer = null;
        public int FileListIndex = 0;
        public PakFile(string name, string handle)
        {
            Handle = handle;
            Name = name;
            Storer = ZipStorer.Open(handle, FileAccess.Read);
        }
    }
}
