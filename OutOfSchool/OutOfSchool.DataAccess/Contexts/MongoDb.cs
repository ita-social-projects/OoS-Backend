using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace OutOfSchool.Services.Contexts
{
    /// <summary>
    /// An instance for getting gridFs contexts.
    /// </summary>
    public class MongoDb // will be deleted because of moving into mysql
    {
        private readonly string server;

        private readonly string database;

        public MongoDb(IOptions<ExternalImageSourceConfig> config)
        {
            server = config.Value.Server ?? throw new InvalidOperationException( "Server name cannot be null");
            database = config.Value.Database ?? throw new InvalidOperationException("Database name cannot be null");
        }

        public IGridFSBucket GetContext()
        {
            var client = new MongoClient(server);
            var databaseInstance = client.GetDatabase(database) ?? throw new InvalidOperationException($"Cannot get an instance of MongoDb database: {database}");

            return new GridFSBucket(databaseInstance);
        }
    }
}
