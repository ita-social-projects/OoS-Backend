﻿using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Changes;

public class ProviderAdminChangesLogDto
{
    public string ProviderAdminId { get; set; }

    public string ProviderAdminFullName { get; set; }

    public string ProviderTitle { get; set; }

    public bool IsDeputy { get; set; }

    public string WorkshopCity { get; set; }

    public OperationType OperationType { get; set; }

    public DateTime OperationDate { get; set; }

    public ShortUserDto User { get; set; }

    public string InstitutionTitle { get; set; }

    public string PropertyName { get; set; }

    public string OldValue { get; set; }

    public string NewValue { get; set; }
}