using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Services.Models;

public interface IKeyedEntity<TKey> : IKeyedEntity
{
    TKey Id { get; set; }
}

public interface IKeyedEntity
{
}