using Ionic.Zip;
using LmpMasterServer.Properties;
using System.IO;

namespace LmpMasterServer.Web
{
    public class WebHandler
    {
        public static readonly string BasePath = Path.Combine(Directory.GetCurrentDirectory(), "web");

        public static void InitWebFiles()
        {
            Directory.CreateDirectory(BasePath);

            using (var stream = new MemoryStream(Resources.css))
            using (var zipFile = ZipFile.Read(stream))
            {
                var folder = Path.Combine(BasePath, "css");
                Directory.CreateDirectory(folder);
                zipFile.ExtractAll(folder, ExtractExistingFileAction.DoNotOverwrite);
            }

            using (var stream = new MemoryStream(Resources.js))
            using (var zipFile = ZipFile.Read(stream))
            {
                var folder = Path.Combine(BasePath, "js");
                Directory.CreateDirectory(folder);
                zipFile.ExtractAll(folder, ExtractExistingFileAction.DoNotOverwrite);
            }

            if (!File.Exists(Path.Combine(BasePath, "favicon.ico")))
            {
                using (var stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "favicon.ico"), FileMode.Create))
                using (var iconStream = new MemoryStream(Resources.favicon))
                {
                    iconStream.CopyTo(stream);
                }
            }
        }
    }
}
