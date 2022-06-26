using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Codeficator;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Codeficator entity.
    /// </summary>
    public class CodeficatorService : ICodeficatorService
    {
        private readonly ICodeficatorRepository codeficatorRepository;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeficatorService"/> class.
        /// </summary>
        /// <param name="codeficatorRepository">СodeficatorRepository repository.</param>
        /// <param name="mapper">Automapper DI service.</param>
        public CodeficatorService(
            ICodeficatorRepository codeficatorRepository,
            IMapper mapper)
        {
            this.codeficatorRepository = codeficatorRepository ?? throw new ArgumentNullException(nameof(codeficatorRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CodeficatorDto>> GetChildrenByParentId(long? id = null)
        {
            var filter = GetFilter(id, CodeficatorCategory.Level1);

            var codeficators = await codeficatorRepository.GetByFilter(filter).ConfigureAwait(false);

            return mapper.Map<IEnumerable<CodeficatorDto>>(codeficators);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<KeyValuePair<long, string>>> GetChildrenNamesByParentId(long? id = null)
        {
            var filter = GetFilter(id, CodeficatorCategory.Level1);

            return await codeficatorRepository.GetNamesByFilter(filter).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<AllAddressPartsDto> GetAllAddressPartsById(long id)
        {
            var codeficator = await codeficatorRepository.GetById(id).ConfigureAwait(false);

            if (codeficator == null)
            {
                return null;
            }

            return new AllAddressPartsDto { AddressParts = mapper.Map<CodeficatorWithParentDto>(codeficator) };
        }

        /// <inheritdoc/>
        public async Task<Dictionary<long, string>> GetFullAddressesByPartOfName(string namePart)
        {
            var fullAddresses = await codeficatorRepository.GetFullAddressesByPartOfName(namePart).ConfigureAwait(false);

            return fullAddresses.ToDictionary(key => key.Id, value => value.FullName);
        }

        #region privateMethods

        private Expression<Func<Codeficator, bool>> GetFilter(long? parentId, CodeficatorCategory level)
        {
            if (parentId.HasValue)
            {
                return c => c.ParentId == parentId.Value;
            }

            return c => level.Name.Contains(c.Category, StringComparison.Ordinal);
        }

        #endregion
    }
}
