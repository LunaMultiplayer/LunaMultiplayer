using Ionic.Zip;
using LMP.MasterServer.Properties;
using System.IO;

namespace LMP.MasterServer.Web
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
                zipFile.ExtractAll(folder, ExtractExistingFileAction.OverwriteSilently);
            }

            using (var stream = new MemoryStream(Resources.js))
            using (var zipFile = ZipFile.Read(stream))
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "js");
                Directory.CreateDirectory(folder);
                zipFile.ExtractAll(folder, ExtractExistingFileAction.OverwriteSilently);
            }

            using (var stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "favicon.ico"), FileMode.Create))
            {
                Resources.favicon.Save(stream);
            }
        }
    }
}
