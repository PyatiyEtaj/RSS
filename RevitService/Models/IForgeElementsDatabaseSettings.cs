using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Models
{
    public class ForgeElementsDatabaseSettings : IForgeElementsDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IForgeElementsDatabaseSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
