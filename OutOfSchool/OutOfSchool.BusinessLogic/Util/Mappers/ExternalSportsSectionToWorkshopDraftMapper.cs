using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.SportsRegistryApiClient.Models.External;
using System.Text.Json;

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

    private static WorkshopDraftContent MapContentFromExternal(
    ExternalSportsSectionDto section,
    WorkshopDraftContent? existingContent = null,
    long catottgId = 0)
    {
        var content = existingContent ?? new WorkshopDraftContent();

        content.Title = section.SectionName;
        content.ShortTitle = section.SectionName;
        content.EnrollmentProcedureDescription = section.SectionRegistrationFlow ?? string.Empty;
        content.CompetitiveSelectionDescription = section.SectionSelectionCriteria ?? string.Empty;
        content.MinAge = section.SectionAgeFrom;
        content.MaxAge = section.SectionAgeTo;
        content.IsChampionPath = section.SectionIsInShlyahProject;

        content.IsPaid = section.SectionPracticeCost > 0;
        content.Price = content.IsPaid ? section.SectionPracticeCost : 0;
        content.PayRate = PayRateType.Month;
        
        var seats = Math.Clamp(section.SectionMaxStudentsAmount, 0, MinSportMaxStudentsLimit);
        content.AvailableSeats = (uint)seats;

        content.Keywords = string.IsNullOrWhiteSpace(section.SectionSportKindDictName)
            ? new List<string>()
            : new List<string> { section.SectionSportKindDictName };

        content.StudyPeriodStartDate = DateOnly.FromDateTime(section.SectionPracticePeriodDateFrom);
        content.StudyPeriodEndDate = DateOnly.FromDateTime(section.SectionPracticePeriodDateTo);

        content.WorkshopDescriptionItems = new List<WorkshopDescriptionItemDraft>
        {
            new WorkshopDescriptionItemDraft
            {
                SectionName = "Опис",
                Description = section.SectionDescription ?? string.Empty
            }
        };

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
                // invalid JSON – ignore phones
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

        content.FormOfLearning = ToFormLearning(section.SectionPracticeFormat);
        content.LanguageOfEducationId = 2;
        content.LanguageOfEducationName = "Українська";
       
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

        var format = "HH:mm";
        var provider = CultureInfo.InvariantCulture;

        try
        {
            var items = JsonSerializer.Deserialize<List<ExternalScheduleItem>>(sectionScheduleJson);
            if (items == null)
                return new List<DateTimeRangeDraft>();

            var result = new List<DateTimeRangeDraft>();
            
            foreach (var item in items)
            {
                if (!TimeOnly.TryParseExact(item.SectionScheduleTimeFrom, format, provider, DateTimeStyles.None, out var startTime) ||
                    !TimeOnly.TryParseExact(item.SectionScheduleTimeTo, format, provider, DateTimeStyles.None, out var endTime))
                {
                    Log.Warning($"Invalid time format in schedule item: From '{item.SectionScheduleTimeFrom}' To '{item.SectionScheduleTimeTo}'");
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

