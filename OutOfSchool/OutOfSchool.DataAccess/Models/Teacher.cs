using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Models;

public class Teacher : IKeyedEntity<Guid>, IImageDependentEntity<Teacher>, ISoftDeleted
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    [MaxLength(60)]
    public string FirstName { get; set; }

    [MaxLength(60)]
    public string LastName { get; set; }

    [MaxLength(60)]
    public string MiddleName { get; set; }

    public Gender Gender { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string Description { get; set; }

    public string CoverImageId { get; set; }

    public Guid? WorkshopId { get; set; }

    public virtual List<Image<Teacher>> Images { get; set; }

    public virtual Workshop Workshop { get; set; }
}