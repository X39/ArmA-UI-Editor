using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;


namespace ArmA_UI_Editor.Code
{
    internal class UpdateManager
    {
        internal struct Update
        {
            public string OrigUrl { get; private set; }
            public bool IsAvailable { get; private set; }
            public string DownloadUrl { get; private set; }
            public string DownloadName { get; private set; }
            public Version NewVersion { get; private set; }

            public Update(string OrigUrl, bool IsAvailable, string DownloadName = default(string), string DownloadUrl = default(string), Version NewVersion = default(Version))
            {
                this.OrigUrl = OrigUrl;
                this.IsAvailable = IsAvailable;
                this.DownloadName = DownloadName;
                this.DownloadUrl = DownloadUrl;
                this.NewVersion = NewVersion;
            }
        }

        private static readonly UpdateManager _Instance = new UpdateManager();
        private HttpClient Client;

        public static UpdateManager Instance { get { return _Instance; } }

        public UpdateManager()
        {
            this.Client = new HttpClient();
        }

        internal async Task<Update> CheckForUpdate(string url)
        {
            var response = await this.Client.GetAsync(url);
            asapJson.JsonNode responseNode = new asapJson.JsonNode(await response.Content.ReadAsStringAsync(), true);
            response.Dispose();

            try
            {
                if (responseNode.getValue_Object()["success"].getValue_Boolean())
                {
                    var version = new Version(responseNode.getValue_Object()["content"].getValue_Object()["version"].getValue_String());
                    var curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                    if(!(version.Major > curVersion.Major || version.Minor > curVersion.Minor || version.Build > curVersion.Build))
                    {
                        return new Update(url, false);
                    }

                    string downloadInfo_Name = string.Empty;
                    string downloadInfo_Url = string.Empty;
                    foreach (var node in responseNode.getValue_Object()["content"].getValue_Object()["download"].getValue_Array())
                    {
                        downloadInfo_Name = node.getValue_Object()["name"].getValue_String();
                        downloadInfo_Url = node.getValue_Object()["link"].getValue_String();
                        break;
                    }
                    return new Update(url, true, downloadInfo_Name, downloadInfo_Url, version);
                }
            }
            catch(Exception ex)
            {
                Logger.Instance.log(Logger.LogLevel.ERROR, ex.Message);
            }
            return new Update(url, false);
        }
    }
}
