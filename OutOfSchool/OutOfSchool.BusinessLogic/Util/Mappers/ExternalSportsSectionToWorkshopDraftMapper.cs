using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using OutOfSchool.SportsRegistryApiClient.Models.External;
using System.Text.Json;

namespace OutOfSchool.BusinessLogic.Util.Mappers;

public static class ExternalSportsSectionToWorkshopDraftMapper
{
    private const int MinSportMaxStudentsLimit = 1000;

    public static WorkshopDraft ToWorkshopDraft(this ExternalSportsSectionDto externalDto, Guid providerId)
    {
       
        long catottgId = 0;
       
        var phones = new List<PhoneNumber>();
        if (!string.IsNullOrWhiteSpace(externalDto.SectionPhones))
        {
            try
            {
                var parsedPhones = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(externalDto.SectionPhones);
                if (parsedPhones is not null)
                {
                    phones = parsedPhones
                        .Select(p => new PhoneNumber
                        {
                            Type = "Основний",
                            Number = p.GetValueOrDefault("phone") ?? string.Empty
                        })
                        .ToList();
                }
            }
            catch
            {
                // if string is not valid JSON, ignore phones
            }
        }

        var socialNetworks = new List<SocialNetwork>();
        if (!string.IsNullOrWhiteSpace(externalDto.SectionFacebookUrl))
        {
            socialNetworks.Add(new SocialNetwork
            {
                Type = SocialNetworkContactType.Facebook,
                Url = externalDto.SectionFacebookUrl
            });
        }
        if (!string.IsNullOrWhiteSpace(externalDto.SectionInstagramUrl))
        {
            socialNetworks.Add(new SocialNetwork
            {
                Type = SocialNetworkContactType.Instagram,
                Url = externalDto.SectionInstagramUrl
            });
        }
        if (!string.IsNullOrWhiteSpace(externalDto.SectionUrl))
        {
            socialNetworks.Add(new SocialNetwork
            {
                Type = SocialNetworkContactType.Website,
                Url = externalDto.SectionUrl
            });
        }

        return new WorkshopDraft
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            MinsportSectionId = externalDto.SectionId,
            CATOTTGId = catottgId,

            WorkshopDraftContent = new WorkshopDraftContent
            {
                Title = externalDto.SectionName,
                ShortTitle = externalDto.SectionName,
                EnrollmentProcedureDescription = externalDto.SectionRegistrationFlow ?? string.Empty,
                CompetitiveSelectionDescription = externalDto.SectionSelectionCriteria ?? string.Empty,

                FormOfLearning = ToFormLearning(externalDto.SectionPracticeFormat),
                Price = externalDto.SectionPracticeCost,
                AvailableSeats = (uint)Math.Min(externalDto.SectionMaxStudentsAmount, MinSportMaxStudentsLimit),

                MinAge = externalDto.SectionAgeFrom,
                MaxAge = externalDto.SectionAgeTo,
                IsChampionPath = externalDto.SectionIsInShlyahProject,

                StudyPeriodStartDate = DateOnly.FromDateTime(externalDto.SectionPracticePeriodDateFrom),
                StudyPeriodEndDate = DateOnly.FromDateTime(externalDto.SectionPracticePeriodDateTo),

                WorkshopDescriptionItems = new List<WorkshopDescriptionItemDraft>
                {
                    new WorkshopDescriptionItemDraft
                    {
                        Description = externalDto.SectionDescription ?? string.Empty
                    }
                },

                // TODO: map SectionSchedule JSON  DateTimeRangeDraft
                DateTimeRanges = new List<DateTimeRangeDraft>(),

                Contacts = new List<Contacts>
                {
                    new Contacts
                    {
                        Title = "Контакт секції",
                        IsDefault = true,
                        Address = new ContactsAddress
                        {
                            Street = externalDto.SectionAddressStreet ?? string.Empty,
                            BuildingNumber = externalDto.SectionAddressHouse ?? string.Empty,
                            CATOTTGId = catottgId
                        },
                        Phones = phones,
                        Emails = new List<Email>
                        {
                            new Email
                            {
                                Type = "Основний",
                                Address = externalDto.SectionEmail ?? string.Empty
                            }
                        },
                        SocialNetworks = socialNetworks
                    }
                },
            },

            // TO DO: map images
            CoverImageId = externalDto.SectionTitlePhoto?.FirstOrDefault()?.Id.ToString() ?? string.Empty,
            //ImageIds = externalDto.SectionPhotos?.Select(p => p.Id.ToString()).ToList() ?? new List<string>(),
            DraftStatus = WorkshopDraftStatus.Draft
        };
    }

    public static WorkshopDraft MapToExistingDraft(this ExternalSportsSectionDto section, WorkshopDraft draft)
    {
        draft.WorkshopDraftContent.Title = section.SectionName;
        draft.WorkshopDraftContent.ShortTitle = section.SectionName;
        draft.WorkshopDraftContent.EnrollmentProcedureDescription = section.SectionRegistrationFlow;
        draft.WorkshopDraftContent.Price = section.SectionPracticeCost;
        draft.WorkshopDraftContent.MinAge = section.SectionAgeFrom;
        draft.WorkshopDraftContent.MaxAge = section.SectionAgeTo;

        // TO DO: others fields  Contacts, ...
        return draft;
    }

    private static FormOfLearning ToFormLearning(string? sectionPracticeFormat)
        => sectionPracticeFormat?.ToUpperInvariant() switch
        {
            "ONLINE" => FormOfLearning.Online,
            "OFFLINE" => FormOfLearning.Offline,
            "HYBRID" => FormOfLearning.Mixed,
            _ => FormOfLearning.Offline // default fallback
        };
}

