using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Providers
{
    public interface IServiceKeyProvider
    {
        string Next();
    }
}
