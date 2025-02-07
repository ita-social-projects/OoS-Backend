using OutOfSchool.Services.Repository.Api;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.BusinessLogic.Util;
public class TakenSeatsMappingHelper
{
    public static async Task FillTakenSeatsForCards(IEnumerable<WorkshopBaseCard> cards, IApplicationRepository applicationRepository)
    {
        var ids = cards.Select(w => w.Id).ToList();
        var takenSeats = await applicationRepository.CountTakenSeatsForWorkshops(ids).ConfigureAwait(false);
        foreach (var card in cards)
        {
            var takenSeatsNumber = takenSeats?.SingleOrDefault(w => w.WorkshopId == card.Id)?.TakenSeats;
            card.TakenSeats = (uint?)takenSeatsNumber ?? 0;
        }
    }

    public static async Task FillTakenSeatsForWorkshopDto(WorkshopDto workshopDto, IApplicationRepository applicationRepository)
    {
        var takenSeats = (await applicationRepository.CountTakenSeatsForWorkshops(new() { workshopDto.Id }).ConfigureAwait(false))
            ?.SingleOrDefault(w => w.WorkshopId == workshopDto.Id)
            ?.TakenSeats ?? 0;

        workshopDto.TakenSeats = (uint)takenSeats;
    }

    public static async Task FillTakenSeatsForWorkshopDto(IEnumerable<WorkshopDto> workshopDtos, IApplicationRepository applicationRepository)
    {
        var ids = workshopDtos.Select(w => w.Id).ToList();
        var takenSeats = await applicationRepository.CountTakenSeatsForWorkshops(ids).ConfigureAwait(false);
        foreach (var card in workshopDtos)
        {
            var takenSeatsNumber = takenSeats?.SingleOrDefault(w => w.WorkshopId == card.Id)?.TakenSeats;
            card.TakenSeats = (uint?)takenSeatsNumber ?? 0;
        }
    }
}
