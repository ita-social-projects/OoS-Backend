using System;
using System.Collections.Generic;
using System.Linq;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class ImagesGenerator
{
    public static List<string> CreateRandomImageIds(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentException($"{nameof(count)} cannot be less or equal 0.");
        }

        return Enumerable.Range(0, count)
            .Select(r => Guid.NewGuid().ToString())
            .ToList();
    }

    public static List<DateTimeOffset> CreateRandomImageDateTimes(int count, DateTimeOffset minDateTime, DateTimeOffset maxDateTime)
    {
        if (count <= 0)
        {
            throw new ArgumentException($"{nameof(count)} cannot be less or equal 0.");
        }

        if (maxDateTime < minDateTime)
        {
            throw new InvalidOperationException(
                $"{nameof(maxDateTime)} must be greater than {nameof(minDateTime)} at least for 1 millisecond.");
        }

        var random = new Random();
        var offset = maxDateTime - minDateTime;

        return Enumerable.Range(0, count)
            .Select(r => minDateTime + TimeSpan.FromMinutes(random.Next(0, (int)offset.TotalMinutes)))
            .ToList();
    }

    public static List<StorageObject> CreateGcpEmptyObjects(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentException($"{nameof(count)} cannot be less or equal 0.");
        }

        var objects = new List<StorageObject>();
        for (var i = 0; i < count; i++)
        {
            objects.Add(new StorageObject());
        }

        return objects;
    }

    public static Image<T> Generate<T>(T entity = null)
        where T : class, IKeyedEntity<Guid>
        => new Image<T>
        {
            EntityId = entity?.Id ?? Guid.NewGuid(),
            Entity = entity,
            ExternalStorageId = Guid.NewGuid().ToString("N")
        };

    public static List<Image<T>> Generate<T>(int count, T entity = null)
        where T : class, IKeyedEntity<Guid>
        => Enumerable.Range(1, count)
        .Select(x => Generate(entity))
        .ToList();
}
