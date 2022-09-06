using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.ElasticsearchData.Models;
public class CodeficatorAddressES
{
    public long Id { get; set; }

    public string Code { get; set; }

    public long? ParentId { get; set; }

    public string Category { get; set; }

    public string Name { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int Order { get; set; } = default;

    public CodeficatorAddressES Parent { get; set; }
}
