using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBDSW
{
    public class Updater
    {
        private const string DOWNLOAD_PAGE_URL = "https://www.minecraft.net/en-us/download/server/bedrock";
        private const string FILENAME_REGEX_PATTERN = @"<a href=""(?<filename>[^\s]*?bin-win[^\s]*?.zip)"".*?>";
        private const string installedFilename = "currentversion.txt";
        public const string serverPath = "bds";

        public static void Update()
        {
            var urlToDownload = GetNewFilename();
            var download = GetUpdate(urlToDownload);
            Extract(download);
            SetCurrentFilename(urlToDownload);
        }

        public static bool NeedsUpdate()
        {
            return !(GetCurrentFilename() == GetNewFilename());
        }

        public static string GetCurrentFilename()
        {
            if (IsInstalled())
            {
                return File.ReadAllText(installedFilename);
            } else
            {
                return "<Not Installed>";
            }
        }

        public static void SetCurrentFilename(string filename)
        {
            File.WriteAllText(installedFilename, filename);
        }

        public static void ClearCurrentFilename()
        {
            File.Delete(installedFilename);
        }

        public static bool IsInstalled()
        {
            return File.Exists(installedFilename);
        }

        public static string GetNewFilename()
        {
            var pageContent = WebGetUtil.GetStringContentFromUrl(DOWNLOAD_PAGE_URL);
            return RegexUtil.FindInString(pageContent, FILENAME_REGEX_PATTERN, "filename");
        }

        public static byte[] GetUpdate(string urlToDownload)
        {
            return WebGetUtil.GetByteContentFromUrl(urlToDownload);
        }

        public static void Extract(byte[] zip)
        {
            var listToNotOverwrite = new List<String>
            {
                "permissions.json",
                "server.properties",
                "whitelist.json"
            };
            var ms = new MemoryStream(zip);
            ms.Position = 0;
            var zipFile = ZipFile.Read(ms);
            foreach(var entry in zipFile)
            {
                var overwriteAction = listToNotOverwrite.Contains(entry.FileName) ? ExtractExistingFileAction.DoNotOverwrite : ExtractExistingFileAction.OverwriteSilently;
                entry.Extract(serverPath, overwriteAction);
            }
        }
    }
}
