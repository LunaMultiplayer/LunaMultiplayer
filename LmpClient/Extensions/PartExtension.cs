namespace LmpClient.Extensions
{
    public static class PartExtension
    {
        public static PartResource FindResource(this Part part, string resourceName)
        {
            for (var i = 0; i < part.Resources.Count; i++)
            {
                if (part.Resources[i].resourceName == resourceName)
                    return part.Resources[i];
            }

            return null;
        }
    }
}
