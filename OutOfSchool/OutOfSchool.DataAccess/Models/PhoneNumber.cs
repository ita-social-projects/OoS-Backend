﻿using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.Services.Models;

public class PhoneNumber : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [CustomPhoneNumber(ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.MaxPhoneNumberLengthWithPlusSign)]
    public string Number { get; set; }
}
