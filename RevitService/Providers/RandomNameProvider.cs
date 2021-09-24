using RevitService.Util;
using System.IO;

namespace RevitService.Providers
{
    public class RandomNameProvider : IRandomNameProvider
    {
        public (string name, string fullpath) Next(string extension)
        {
            string name = $"{Path.GetRandomFileName().Replace('.', '_')}.{extension}";
            string fullpath = FileManager.GetFilePath(name);
            return (name, fullpath);
        }
    }
}
