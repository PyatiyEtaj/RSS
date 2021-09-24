using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RevitService.Models;
using RevitService.Services.Repository;
using System.Threading;
using System.Threading.Tasks;

namespace RevitService.Services
{
    public class ConfigureMongoDbService : IHostedService
    {
        private readonly MongoService _mongoService;
        private readonly ILogger<ConfigureMongoDbService> _logger;

        public ConfigureMongoDbService(MongoService mongoService, ILogger<ConfigureMongoDbService> logger)
        {
            _mongoService = mongoService;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _mongoService.Db
                .GetCollection<RevitRequest>(MongoCollectionNames.RevitRequests.ToString())
                .Indexes.CreateOneAsync(
                    new CreateIndexModel<RevitRequest>(
                        Builders<RevitRequest>.IndexKeys.Ascending(x => x.UserId)
                    )
                );
            _logger.LogInformation($"Ensure index collection[{MongoCollectionNames.RevitRequests}] on RSOTaskElement.User");
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
