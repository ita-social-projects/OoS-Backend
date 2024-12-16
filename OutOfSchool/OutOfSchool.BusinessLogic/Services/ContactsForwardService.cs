using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class ContactsForwardService : IContactsForwardService
{
    private readonly IEntityRepositorySoftDeleted<long, ContactsForward> contactForwardRepository;
    private readonly ILogger<FavoriteService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;

    public ContactsForwardService(
        IEntityRepositorySoftDeleted<long, ContactsForward> contactForwardRepository,
        ILogger<FavoriteService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper)
    {
        this.contactForwardRepository = contactForwardRepository ?? throw new ArgumentNullException(nameof(contactForwardRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ContactsForwardDTO>> GetAll()
    {
        logger.LogInformation("Getting all Contacts started.");

        var contacts = await contactForwardRepository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!contacts.Any()
            ? "Contacts table is empty."
            : $"All {contacts.Count()} records were successfully received from the Contacts table");

        return contacts.Select(contact => mapper.Map<ContactsForwardDTO>(contact)).ToList();
    }

    /// <inheritdoc/>
    public async Task<ContactsForwardDTO> GetById(long id)
    {
        logger.LogInformation($"Getting Contact by Id started. Looking Id = {id}.");

        var contact = await contactForwardRepository.GetById(id).ConfigureAwait(false);

        if (contact == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation($"Successfully got a Contact with Id = {id}.");

        return mapper.Map<ContactsForwardDTO>(contact);
    }

    public async Task<ContactsForwardDTO> Create(ContactsForward dto)
    {
        logger.LogInformation("Contact creating was started.");

        var contact = mapper.Map<ContactsForward>(dto);

        var newContact = await contactForwardRepository.Create(contact).ConfigureAwait(false);

        logger.LogInformation($"Contact with Id = {newContact?.Id} created successfully.");

        return mapper.Map<ContactsForwardDTO>(newContact);
    }

    public async Task<ContactsForwardDTO> Update(ContactsForward dto)
    {
        logger.LogInformation($"Updating Contact with Id = {dto?.Id} started.");

        ArgumentNullException.ThrowIfNull(dto);

        var contact = await contactForwardRepository.GetById(dto.Id).ConfigureAwait(false);

        if (contact is null)
        {
            var message = $"Updating failed. Contact with Id = {dto.Id} doesn't exist in the system.";
            logger.LogError(message);
            throw new DbUpdateConcurrencyException(message);
        }

        mapper.Map(dto, contact);
        contact = await contactForwardRepository.Update(contact).ConfigureAwait(false);

        logger.LogInformation($"Contact with Id = {contact?.Id} updated succesfully.");

        return mapper.Map<ContactsForwardDTO>(contact);
    }
    
    public async Task Delete(long id)
    {
        logger.LogInformation($"Deleting Contact with Id = {id} started.");

        var contact = await contactForwardRepository.GetById(id).ConfigureAwait(false);

        if (contact == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer[$"Contact with Id = {id} doesn't exist in the system"]);
        }

        await contactForwardRepository.Delete(contact).ConfigureAwait(false);

        logger.LogInformation($"Contact with Id = {id} succesfully deleted.");
    }
}