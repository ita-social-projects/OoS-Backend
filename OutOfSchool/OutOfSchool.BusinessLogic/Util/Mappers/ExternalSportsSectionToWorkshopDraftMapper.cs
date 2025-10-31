using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using OutOfSchool.SportsRegistryApiClient.Models.External;
using System.Diagnostics;
using System.Text.Json;
using static System.Collections.Specialized.BitVector32;

namespace OutOfSchool.BusinessLogic.Util.Mappers;

public static class ExternalSportsSectionToWorkshopDraftMapper
{
    private const int MinSportMaxStudentsLimit = 1000;
    public static WorkshopDraft ToWorkshopDraft(this ExternalSportsSectionDto section, Guid providerId, long catottgId)
    {
        return new WorkshopDraft
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            MinsportSectionId = section.SectionId,
            CATOTTGId = catottgId,
            WorkshopDraftContent = MapContentFromExternal(section, null, catottgId),
            CoverImageId = section.SectionTitlePhoto?.FirstOrDefault()?.Id.ToString() ?? string.Empty,
            DraftStatus = WorkshopDraftStatus.PendingModeration
        };
    }

    public static WorkshopDraft MapToExistingDraft(this ExternalSportsSectionDto section, WorkshopDraft draft, long catottgId)
    {
        draft.WorkshopDraftContent = MapContentFromExternal(section, draft.WorkshopDraftContent, catottgId);
        draft.DraftStatus = WorkshopDraftStatus.PendingModeration;
        return draft;
    }


    //public static WorkshopDraft ToWorkshopDraft(this ExternalSportsSectionDto externalDto, Guid providerId)
    //{

    //   // long catottgId = 0;

    //    var phones = new List<PhoneNumber>();
    //    if (!string.IsNullOrWhiteSpace(externalDto.SectionPhones))
    //    {
    //        try
    //        {
    //            var parsedPhones = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(externalDto.SectionPhones);
    //            if (parsedPhones is not null)
    //            {
    //                phones = parsedPhones
    //                    .Select(p => new PhoneNumber
    //                    {
    //                        Type = "Основний",
    //                        Number = p.GetValueOrDefault("phone") ?? string.Empty
    //                    })
    //                    .ToList();
    //            }
    //        }
    //        catch
    //        {
    //            // if string is not valid JSON, ignore phones
    //        }
    //    }

    //    var socialNetworks = new List<SocialNetwork>();
    //    if (!string.IsNullOrWhiteSpace(externalDto.SectionFacebookUrl))
    //    {
    //        socialNetworks.Add(new SocialNetwork
    //        {
    //            Type = SocialNetworkContactType.Facebook,
    //            Url = externalDto.SectionFacebookUrl
    //        });
    //    }
    //    if (!string.IsNullOrWhiteSpace(externalDto.SectionInstagramUrl))
    //    {
    //        socialNetworks.Add(new SocialNetwork
    //        {
    //            Type = SocialNetworkContactType.Instagram,
    //            Url = externalDto.SectionInstagramUrl
    //        });
    //    }
    //    if (!string.IsNullOrWhiteSpace(externalDto.SectionUrl))
    //    {
    //        socialNetworks.Add(new SocialNetwork
    //        {
    //            Type = SocialNetworkContactType.Website,
    //            Url = externalDto.SectionUrl
    //        });
    //    }

    //    //if need to create keywords from sport kind name
    //    List<string> keywords = string.IsNullOrWhiteSpace(externalDto.SectionSportKindDictName)
    //    ? new()
    //    : new() { externalDto.SectionSportKindDictName };

    //    return new WorkshopDraft
    //    {
    //        Id = Guid.NewGuid(),
    //        ProviderId = providerId,
    //        MinsportSectionId = externalDto.SectionId,
    //       // CATOTTGId = catottgId,

    //        WorkshopDraftContent = new WorkshopDraftContent
    //        {
    //            Title = externalDto.SectionName,
    //            ShortTitle = externalDto.SectionName,
    //            EnrollmentProcedureDescription = externalDto.SectionRegistrationFlow ?? string.Empty,
    //            CompetitiveSelectionDescription = externalDto.SectionSelectionCriteria ?? string.Empty,

    //            FormOfLearning = ToFormLearning(externalDto.SectionPracticeFormat),
    //            Price = externalDto.SectionPracticeCost,
    //            AvailableSeats = (uint)Math.Min(externalDto.SectionMaxStudentsAmount, MinSportMaxStudentsLimit),

    //            MinAge = externalDto.SectionAgeFrom,
    //            MaxAge = externalDto.SectionAgeTo,
    //            IsChampionPath = externalDto.SectionIsInShlyahProject,

    //            StudyPeriodStartDate = DateOnly.FromDateTime(externalDto.SectionPracticePeriodDateFrom),
    //            StudyPeriodEndDate = DateOnly.FromDateTime(externalDto.SectionPracticePeriodDateTo),

    //            WorkshopDescriptionItems = new List<WorkshopDescriptionItemDraft>
    //            {
    //                new WorkshopDescriptionItemDraft
    //                {
    //                    Description = externalDto.SectionDescription ?? string.Empty
    //                }
    //            },

    //            Keywords = keywords,

    //            DateTimeRanges = MapSchedule(externalDto.SectionSchedule),

    //            Contacts = new List<Contacts>
    //            {
    //                new Contacts
    //                {
    //                    Title = "Контакт секції",
    //                    IsDefault = true,
    //                    Address = new ContactsAddress
    //                    {
    //                        Street = externalDto.SectionAddressStreet ?? string.Empty,
    //                        BuildingNumber = externalDto.SectionAddressHouse ?? string.Empty,
    //                        //CATOTTGId = catottgId // ????
    //                    },
    //                    Phones = phones,
    //                    Emails = new List<Email>
    //                    {
    //                        new Email
    //                        {
    //                            Type = "Основний",
    //                            Address = externalDto.SectionEmail ?? string.Empty
    //                        }
    //                    },
    //                    SocialNetworks = socialNetworks
    //                }
    //            },
    //        },

    //        // TO DO: map images
    //        CoverImageId = externalDto.SectionTitlePhoto?.FirstOrDefault()?.Id.ToString() ?? string.Empty,
    //        //ImageIds = externalDto.SectionPhotos?.Select(p => p.Id.ToString()).ToList() ?? new List<string>(),
    //        DraftStatus = WorkshopDraftStatus.PendingModeration
    //    };
    //}

    //public static WorkshopDraft MapToExistingDraft(this ExternalSportsSectionDto section, WorkshopDraft draft)
    //{
    //    draft.WorkshopDraftContent.Title = section.SectionName;
    //    draft.WorkshopDraftContent.ShortTitle = section.SectionName;
    //    draft.WorkshopDraftContent.EnrollmentProcedureDescription = section.SectionRegistrationFlow;
    //    draft.WorkshopDraftContent.Price = section.SectionPracticeCost;
    //    draft.WorkshopDraftContent.MinAge = section.SectionAgeFrom;
    //    draft.WorkshopDraftContent.MaxAge = section.SectionAgeTo;

    //    // TO DO: others fields  Contacts, ...
    //    return draft;
    //}

    private static WorkshopDraftContent MapContentFromExternal(
    ExternalSportsSectionDto section,
    WorkshopDraftContent? existingContent = null,
    long catottgId = 0)
    {
        var content = existingContent ?? new WorkshopDraftContent();

        // Загальні поля
        content.Title = section.SectionName;
        content.ShortTitle = section.SectionName;
        content.EnrollmentProcedureDescription = section.SectionRegistrationFlow ?? string.Empty;
        content.CompetitiveSelectionDescription = section.SectionSelectionCriteria ?? string.Empty;
        content.Price = section.SectionPracticeCost;
        content.MinAge = section.SectionAgeFrom;
        content.MaxAge = section.SectionAgeTo;
        content.IsChampionPath = section.SectionIsInShlyahProject;

        // Keywords
        content.Keywords = string.IsNullOrWhiteSpace(section.SectionSportKindDictName)
            ? new List<string>()
            : new List<string> { section.SectionSportKindDictName };

        // Study period
        content.StudyPeriodStartDate = DateOnly.FromDateTime(section.SectionPracticePeriodDateFrom);
        content.StudyPeriodEndDate = DateOnly.FromDateTime(section.SectionPracticePeriodDateTo);

        // WorkshopDescriptionItems
        content.WorkshopDescriptionItems = new List<WorkshopDescriptionItemDraft>
    {
        new WorkshopDescriptionItemDraft
        {
            Description = section.SectionDescription ?? string.Empty
        }
    };

        // DateTimeRanges
        content.DateTimeRanges = MapSchedule(section.SectionSchedule);

        // Contacts
        var phones = new List<PhoneNumber>();
        if (!string.IsNullOrWhiteSpace(section.SectionPhones))
        {
            try
            {
                var parsedPhones = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(section.SectionPhones);
                if (parsedPhones != null)
                {
                    phones = parsedPhones.Select(p => new PhoneNumber
                    {
                        Type = "Основний",
                        Number = p.GetValueOrDefault("phone") ?? string.Empty
                    }).ToList();
                }
            }
            catch
            {
                // некоректний JSON – ігноруємо телефони
            }
        }

        var socialNetworks = new List<SocialNetwork>();
        if (!string.IsNullOrWhiteSpace(section.SectionFacebookUrl))
        {
            socialNetworks.Add(new SocialNetwork
            {
                Type = SocialNetworkContactType.Facebook,
                Url = section.SectionFacebookUrl
            });
        }
        if (!string.IsNullOrWhiteSpace(section.SectionInstagramUrl))
        {
            socialNetworks.Add(new SocialNetwork
            {
                Type = SocialNetworkContactType.Instagram,
                Url = section.SectionInstagramUrl
            });
        }
        if (!string.IsNullOrWhiteSpace(section.SectionUrl))
        {
            socialNetworks.Add(new SocialNetwork
            {
                Type = SocialNetworkContactType.Website,
                Url = section.SectionUrl
            });
        }

        content.Contacts = new List<Contacts>
    {
        new Contacts
        {
            Title = "Контакт секції",
            IsDefault = true,
            Address = new ContactsAddress
            {
                Street = section.SectionAddressStreet ?? string.Empty,
                BuildingNumber = section.SectionAddressHouse ?? string.Empty,
                CATOTTGId = catottgId
            },
            Phones = phones,
            Emails = new List<Email>
            {
                new Email { Type = "Основний", Address = section.SectionEmail ?? string.Empty }
            },
            SocialNetworks = socialNetworks
        }
    };

        // Можна додати додаткові поля (Coverage, WorkshopType, FormOfLearning тощо)
        content.FormOfLearning = ToFormLearning(section.SectionPracticeFormat);

        return content;
    }


    private static FormOfLearning ToFormLearning(string? sectionPracticeFormat)
        => sectionPracticeFormat?.ToUpperInvariant() switch
        {
            "ONLINE" => FormOfLearning.Online,
            "OFFLINE" => FormOfLearning.Offline,
            "HYBRID" => FormOfLearning.Mixed,
            _ => FormOfLearning.Offline // default fallback
        };
    private static List<DateTimeRangeDraft> MapSchedule(string? sectionScheduleJson)
    {
        if (string.IsNullOrWhiteSpace(sectionScheduleJson))
            return new List<DateTimeRangeDraft>();

        try
        {
            var items = JsonSerializer.Deserialize<List<ExternalScheduleItem>>(sectionScheduleJson);
            if (items == null)
                return new List<DateTimeRangeDraft>();

            var result = new List<DateTimeRangeDraft>();

            foreach (var item in items)
            {
                if (!TimeOnly.TryParse(item.SectionScheduleTimeFrom, out var startTime) ||
                    !TimeOnly.TryParse(item.SectionScheduleTimeTo, out var endTime))
                {
                    Debug.WriteLine($"Invalid time format in schedule item: From '{item.SectionScheduleTimeFrom}' To '{item.SectionScheduleTimeTo}'");
                    continue; // skip invalid time formats
                }

                var dayMask = ToDaysBitMask(item.SectionScheduleWeekday);
                if (dayMask == DaysBitMask.None)
                    continue;

                result.Add(new DateTimeRangeDraft
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    Workdays = new HashSet<DaysBitMask> { dayMask }
                });
            }

            return result;
        }
        catch
        {
            return new List<DateTimeRangeDraft>();
        }
    }

    //private static List<DateTimeRangeDraft> MapSchedule(string? sectionScheduleJson)
    //{
    //    if (string.IsNullOrWhiteSpace(sectionScheduleJson))
    //        return new List<DateTimeRangeDraft>();

    //    try
    //    {
    //        var items = JsonSerializer.Deserialize<List<ExternalScheduleItem>>(sectionScheduleJson);
    //        return items?.Select(i => new DateTimeRangeDraft
    //        {
    //            StartTime = TimeOnly.Parse(i.SectionScheduleTimeFrom),
    //            EndTime = TimeOnly.Parse(i.SectionScheduleTimeTo),
    //            Workdays = new HashSet<DaysBitMask> { ToDaysBitMask(i.SectionScheduleWeekday) }
    //        }).ToList() ?? new List<DateTimeRangeDraft>();
    //    }
    //    catch
    //    {
    //        return new List<DateTimeRangeDraft>();
    //    }
    //}

    private static DaysBitMask ToDaysBitMask(string weekday) => weekday.ToUpperInvariant() switch
    {
        "MONDAY" => DaysBitMask.Monday,
        "TUESDAY" => DaysBitMask.Tuesday,
        "WEDNESDAY" => DaysBitMask.Wednesday,
        "THURSDAY" => DaysBitMask.Thursday,
        "FRIDAY" => DaysBitMask.Friday,
        "SATURDAY" => DaysBitMask.Saturday,
        "SUNDAY" => DaysBitMask.Sunday,
        _ => DaysBitMask.None
    };

}

