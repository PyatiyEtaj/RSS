using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Util
{
    public static class FileManager
    {
        public static readonly string FilesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "docs", "temp");
        public static string GetFilePath(string f) => Path.Combine(FilesDirectory, f);
        public static bool IsNameSafe(string name) => FilesDirectory == Path.GetDirectoryName(name);        
    }
}
