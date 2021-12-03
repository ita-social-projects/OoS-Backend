using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using OutOfSchool.Services.Contexts.Configuration;

namespace OutOfSchool.Services.Contexts
{
    /// <summary>
    /// An instance for getting gridFs contexts.
    /// </summary>
    public class MongoDb // will be deleted because of moving into mysql
    {
        private readonly string serverName;

        private readonly string databaseName;

        public MongoDb(IOptions<ExternalImageSourceConfig> config)
        {
            if (string.IsNullOrEmpty(config.Value.ServerName))
            {
                throw new InvalidOperationException("Server name is null or invalid");
            }

            if (string.IsNullOrEmpty(config.Value.DatabaseName))
            {
                throw new InvalidOperationException("Database name is null or invalid");
            }

            serverName = config.Value.ServerName;
            databaseName = config.Value.DatabaseName;
        }

        public IGridFSBucket GetContext()
        {
            var client = new MongoClient(serverName);
            var databaseInstance = client.GetDatabase(databaseName) ?? throw new InvalidOperationException($"Cannot get an instance of MongoDb databaseName: {databaseName}");

            return new GridFSBucket(databaseInstance);
        }
    }
}
