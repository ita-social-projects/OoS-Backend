using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models;

public abstract class BusinessEntity : IKeyedEntity<Guid>, ISoftDeleted
{
    protected string _createdBy;

    protected string _modifiedBy;

    protected string _deletedBy;

    protected DateTime _createdAt;

    protected DateTime? _updatedAt;

    protected DateTime? _deleteDate;

    protected bool _isSystemProtected = false;

    public Guid Id { get; set; }

    public string Document { get; set; }

    public string File { get; set; }

    public DateOnly ActiveFrom { get; set; }

    public DateOnly ActiveTo { get; set; }

    public bool IsBlocked { get; set; } = false;

    public bool IsSystemProtected => _isSystemProtected;

    [Column(TypeName = "char")]
    [MaxLength(36)]
    public string CreatedBy => _createdBy;

    [Column(TypeName = "char")]
    [MaxLength(36)]
    public string ModifiedBy => _modifiedBy;

    [Column(TypeName = "char")]
    [MaxLength(36)]
    public string DeletedBy => _deletedBy;

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt => _createdAt;

    [DataType(DataType.DateTime)]
    public DateTime? UpdatedAt => _updatedAt;

    [DataType(DataType.DateTime)]
    public DateTime? DeleteDate => _deleteDate;

    public bool IsDeleted { get; set; }
}