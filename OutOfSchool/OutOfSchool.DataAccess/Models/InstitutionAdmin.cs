using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

public class InstitutionAdmin : InstitutionAdminBase, IKeyedEntity<(string, Guid)>
{
    [NotMapped]
    public (string, Guid) Id
    {
        get => (UserId, InstitutionId);
        set
        {
            UserId = value.Item1;
            InstitutionId = value.Item2;
        }
    }
}