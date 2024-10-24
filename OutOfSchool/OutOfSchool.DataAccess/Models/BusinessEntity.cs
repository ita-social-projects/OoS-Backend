using System;
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

    public bool IsSystemProtected
    {
        get { return _isSystemProtected; }
        private set { _isSystemProtected = value; }
    }

    [Column(TypeName = "char")]
    [MaxLength(36)]
    public string CreatedBy
    {
        get { return _createdBy; }
        private set { _createdBy = value; }
    }

    [Column(TypeName = "char")]
    [MaxLength(36)]
    public string ModifiedBy
    {
        get { return _modifiedBy; }
        private set { _modifiedBy = value; }
    }

    [Column(TypeName = "char")]
    [MaxLength(36)]
    public string DeletedBy
    {
        get { return _deletedBy; }
        private set { _deletedBy = value; }
    }

    [DataType(DataType.DateTime)]
    public DateTime CreatedAt
    {
        get { return _createdAt; }
        private set { _createdAt = value; }
    }

    [DataType(DataType.DateTime)]
    public DateTime? UpdatedAt
    {
        get { return _updatedAt; }
        private set { _updatedAt = value; }
    }

    [DataType(DataType.DateTime)]
    public DateTime? DeleteDate
    {
        get { return _deleteDate; }
        private set { _deleteDate = value; }
    }

    public bool IsDeleted { get; set; }
}