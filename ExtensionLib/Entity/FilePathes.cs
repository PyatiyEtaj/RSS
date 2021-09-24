using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExtensionLib.Entity
{
    public static class FilePathes
    {        
        public static readonly string ExcelPrices = Path.Combine("docs", "temp", "database_prices_progress.xlsx");
        public static readonly string Template = Path.Combine("docs","KsTemplate.json");
        // public static readonly string Result = Path.Combine("docs","KC.xlsx");
    }
}
