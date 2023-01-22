using LmpMasterServer.Properties;
using System.IO;
using System.Collections.Generic;

namespace LmpMasterServer.Http.Handlers
{
    public class WebHandler
    {
        public static readonly string BasePath = Path.Combine(Directory.GetCurrentDirectory(), "web");

        public static void InitWebFiles()
        {
            Directory.CreateDirectory(BasePath);

            var files = new List<(string, string, string)>()
            {
                ("js_jquery", "js", "jquery-3.6.3.min.js"),
                ("js_jquery_metadata", "js", "jquery.metadata.js"),
                ("js_tablesorter", "js", "jquery.tablesorter-2.31.3.min.js"),
                ("js_lmp", "js", "lmp.js"),
                ("css_tablesorter", "css", "jquery.tablesorter-theme-2.31.3.default.min.css"),
                ("css_style", "css", "style.css"),
                ("favicon", "", "favicon.ico"),
            };

            foreach (var triple in files)
            {
                var resObj = (byte[])Resources.ResourceManager.GetObject(triple.Item1, Resources.Culture);
                using (var stream = new MemoryStream(resObj))
                {
                    var folder = Path.Combine(BasePath, triple.Item2);
                    Directory.CreateDirectory(folder);
                    using var file = File.Create(Path.Combine(folder, triple.Item3));
                    stream.WriteTo(file);
                }
            }

            // if (!File.Exists(Path.Combine(BasePath, "favicon.ico")))
            // {
            //     using (var stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "favicon.ico"), FileMode.Create))
            //     using (var iconStream = new MemoryStream(Resources.favicon))
            //     {
            //         iconStream.CopyTo(stream);
            //     }
            // }
        }
    }
}
