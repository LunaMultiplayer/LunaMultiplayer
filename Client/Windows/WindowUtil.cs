using System.IO;
using UnityEngine;

namespace LunaClient.Windows
{
    public class WindowUtil
    {
        public static Texture2D LoadIcon(string path, int width, int height )
        {
            var image = new Texture2D(width, height);
            if (File.Exists(path))
            {
                var data = File.ReadAllBytes(path);
                image.LoadImage(data);
            }

            return image;
        }
    }
}
