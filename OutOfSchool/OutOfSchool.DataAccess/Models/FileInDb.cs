using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Models;
public class FileInDb : IKeyedEntity<string>
{
    public string Id { get; set; }

    public string ContentType { get; set; }

    public byte[] Data { get; set; }
}
