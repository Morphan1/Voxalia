using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared.Files;
using FreneticScript;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem
{
    public class WebPage
    {
        public Server TheServer;

        public Connection Network;

        public WebPage(Server tserver, Connection conn)
        {
            TheServer = tserver;
            Network = conn;
        }

        public string http_request_original;

        public string http_request_mode;

        public string http_request_page;

        public string http_request_version;

        public Dictionary<string, string> http_request_headers = new Dictionary<string, string>();

        public void Init(string request)
        {
            string[] headbits = request.Replace("\r", "").Split('\n');
            http_request_original = headbits[0];
            string[] oreq = http_request_original.Split(' ');
            if (oreq.Length != 3)
            {
                throw new ArgumentException("Invalid web request!");
            }
            http_request_mode = oreq[0];
            http_request_page = oreq[1];
            http_request_version = oreq[1];
            for (int i = 1; i < headbits.Length; i++)
            {
                if (headbits[i].Length < 2)
                {
                    continue;
                }
                string[] dat = headbits[i].Split(new char[] { ':' }, 2);
                if (dat.Length != 2)
                {
                    continue;
                }
                http_request_headers[dat[0].Trim()] = dat[1].Trim();
            }
            string encAllowed;
            if (http_request_headers.TryGetValue("Accept-Encoding", out encAllowed))
            {
                GZip = encAllowed.Contains("gzip");
            }
            try
            {
                GetPage();
            }
            catch (Exception ex)
            {
                SysConsole.Output("Handling webpage", ex); // TODO: DebugWebError cvar?
                http_response_id = 500;
                http_response_itname = "Internal Server Error";
                http_response_contenttype = "text/plain; charset=UTF-8";
                http_response_content = FileHandler.encoding.GetBytes("500 Internal Server Error\n");
            }
            if (GZip)
            {
                http_response_content = FileHandler.GZip(http_response_content);
            }
        }

        public void GetPage()
        {
            string pageLow = http_request_page.ToLowerFast();
            if (pageLow.StartsWith("/map/region/"))
            {
                string after;
                string region = pageLow.Substring("/map/region/".Length).BeforeAndAfter("/", out after);
                for (int i = 0; i < TheServer.LoadedRegions.Count; i++)
                {
                    if (TheServer.LoadedRegions[i].Name == region)
                    {
                        string[] dat = after.Split('/');
                        if (dat[0] == "img" && dat.Length >= 4)
                        {
                            int x = Utilities.StringToInt(dat[1]);
                            int y = Utilities.StringToInt(dat[2]);
                            int z = Utilities.StringToInt(dat[3].Before(".png"));
                            byte[] data = TheServer.LoadedRegions[i].ChunkManager.GetImage(x, y, z);
                            if (data != null)
                            {
                                http_response_contenttype = "image/png";
                                http_response_content = data;
                                return;
                            }
                        }
                        break;
                    }
                }
            }
            Do404();
        }

        public void Do404()
        {
            // Placeholder
            string respcont = "<!doctype HTML>\n<html>";
            respcont += "<head><title>Voxalia Server</title></head>\n";
            respcont += "<body><h1>Hello, this is a test page!</h1>\n<br>";
            respcont += "Online players: " + TheServer.Players.Count + "\n<br>";
            respcont += "</body>\n";
            respcont += "</html>\n";
            http_response_content = FileHandler.encoding.GetBytes(respcont);
        }

        public bool GZip = false;

        public string HTTP_RESPONSE_VERSION = "HTTP/1.1";

        public int http_response_id = 200;

        public string http_response_itname = "OK";

        public string http_response_contenttype = "text/html; charset=UTF-8";

        public int http_response_contentlength = 0;

        public byte[] http_response_content = null;

        public string GetHeaders()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(HTTP_RESPONSE_VERSION + " " + http_response_id + " " + http_response_itname + "\n");
            if (GZip)
            {
                sb.Append("Content-Encoding: gzip\n");
            }
            sb.Append("Content-Type: " + http_response_contenttype + "\n");
            sb.Append("Content-Length: " + http_response_content.Length + "\n");
            sb.Append("\n");
            return sb.ToString();
        }

        public byte[] GetFullData()
        {
            byte[] header = FileHandler.encoding.GetBytes(GetHeaders());
            byte[] result = new byte[http_response_content.Length + header.Length];
            header.CopyTo(result, 0);
            http_response_content.CopyTo(result, header.Length);
            return result;
        }
    }
}
