using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace OutOfSchool.Services.Contexts
{
    internal static class MongoDb
    {
        public static IGridFSBucket GetContext()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("Pictures") ?? throw new InvalidOperationException();

            return new GridFSBucket(database);
        }
    }
}
