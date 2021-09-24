using MongoDB.Driver;
using RevitService.Models;
using RevitService.Services.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevitService.Services.KS.Repository
{
    public class ElementRepository : IElementRepository
    {
        private readonly MongoService _mongo;

        public ElementRepository(MongoService mongo)
            => _mongo = mongo;

        public async Task<List<ForgeElement>> GetAsync(string urn, DateTime start, DateTime end)
        {
            return (await _mongo.Db
                .GetCollection<ForgeElement>(urn)
                .FindAsync(x =>
                    x.Date >= start && x.Date <= end &&
                    x.CharMat.Length > 0 &&
                    x.Construction.Length > 0 &&
                    x.Material.Length > 0 &&
                    x.Params.Length > 0)).ToList();
        }
    }
}
