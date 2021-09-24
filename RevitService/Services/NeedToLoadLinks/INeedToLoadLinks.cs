using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Services.NeedToLoadLinks
{
    public interface INeedToLoadLinks
    {
        Task<bool> LoadOrNotAsync(string name);
    }
}
