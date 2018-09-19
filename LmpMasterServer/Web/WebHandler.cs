using Ionic.Zip;
using LmpMasterServer.Properties;
using System.IO;

namespace LmpMasterServer.Web
{
    public class WebHandler
    {
        public static void InitWebFiles()
        {
            using (var stream = new MemoryStream(Resources.css))
            using (var zipFile = ZipFile.Read(stream))
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "css");
                Directory.CreateDirectory(folder);
                zipFile.ExtractAll(folder, ExtractExistingFileAction.DoNotOverwrite);
            }

            using (var stream = new MemoryStream(Resources.js))
            using (var zipFile = ZipFile.Read(stream))
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "js");
                Directory.CreateDirectory(folder);
                zipFile.ExtractAll(folder, ExtractExistingFileAction.DoNotOverwrite);
            }

            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "favicon.ico")))
            {
                using (var stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "favicon.ico"), FileMode.Create))
                {
                    Resources.favicon.Save(stream);
                }
            }
        }
    }
}
