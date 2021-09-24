using RevitService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RevitService.Services.KS.Repository
{
    public interface IElementRepository
    {
        Task<List<ForgeElement>> GetAsync(string urn, DateTime start, DateTime end);
    }
}
