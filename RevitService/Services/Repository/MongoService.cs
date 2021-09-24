using RevitService.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitService.Services.Repository
{
    public class MongoService
    {
        private readonly ILogger<MongoService> _logger;

        public IMongoDatabase Db;

        public MongoService(ILogger<MongoService> logger, IForgeElementsDatabaseSettings settings)
        {
            _logger = logger;

            var client = new MongoClient(
                settings.ConnectionString
            );

            _logger.LogInformation("Succesful get Mongo Client");

            Db = client.GetDatabase(settings.DatabaseName);

            _logger.LogInformation($"Succesful get collection {settings.DatabaseName}");
        }
    }
}
