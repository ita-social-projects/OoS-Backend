using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services
{
    public class AchievementService: IAchievementService
    {
        private readonly ISensitiveEntityRepository<Achievement> repository;
        private readonly ILogger<AchievementService> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly IMapper mapper;
        private readonly OutOfSchoolDbContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementService"/> class.
        /// </summary>
        /// <param name="repository">Repository for Achievement entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="mapper">Mapper.</param>
        public AchievementService(
            ISensitiveEntityRepository<Achievement> repository,
            ILogger<AchievementService> logger,
            IStringLocalizer<SharedResource> localizer,
            IMapper mapper,
            OutOfSchoolDbContext context)
        {
            this.localizer = localizer;
            this.repository = repository;
            this.logger = logger;
            this.mapper = mapper;
            this.context = context;
        }

        /// <inheritdoc/>
        public async Task<AchievementDto> Create(AchievementCreateDTO dto)
        {
            logger.LogInformation("Achievement creating was started.");

            var achievement = mapper.Map<Achievement>(dto);

            achievement.Children = context.Children.Where(w => dto.ChildrenIDs.Contains(w.Id))
                            .ToList();

            achievement.Teachers = new List<AchievementTeacher>();

            foreach (string Teacher in dto.Teachers)
            {
                achievement.Teachers.Add(new AchievementTeacher { Title = Teacher, Achievement = achievement });
            }

            var newAchievement = await repository.Create(achievement).ConfigureAwait(false);

            logger.LogInformation($"Achievement with Id = {newAchievement?.Id} created successfully.");

            return mapper.Map<AchievementDto>(newAchievement);
        }

        /// <inheritdoc/>
        public async Task<Result<AchievementDto>> Delete(Guid id)
        {
            logger.LogInformation($"Deleting Achievement with Id = {id} started.");

            var entity = new Achievement() { Id = id };

            try
            {
                await repository.Delete(entity).ConfigureAwait(false);

                logger.LogInformation($"Achievement with Id = {id} succesfully deleted.");

                return Result<AchievementDto>.Success(mapper.Map<AchievementDto>(entity));
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Deleting failed. Achievement with Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<AchievementDto> GetById(Guid id)
        {
            logger.LogInformation($"Getting Achievement by Id started. Looking Id = {id}.");

            var achievement = await repository.GetById(id).ConfigureAwait(false);

            if (achievement == null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.LogInformation($"Successfully got a Achievement with Id = {id}.");

            return mapper.Map<AchievementDto>(achievement);
        }

        /// <inheritdoc/>
        public async Task<AchievementDto> Update(AchievementCreateDTO dto)
        {
            logger.LogInformation($"Updating Achievement with Id = {dto?.Id} started.");

            try
            {
                var achievement = await repository.GetById(dto.Id).ConfigureAwait(false);

                achievement.AchievementTypeId = dto.AchievementTypeId;
                achievement.Title = dto.Title;
                achievement.AchievementDate = dto.AchievementDate;
                achievement.WorkshopId = dto.WorkshopId;
                achievement.Children.RemoveAll(x => dto.ChildrenIDs.IndexOf(x.Id) < 0);
                achievement.Children.AddRange(context.Children.Where(w => dto.ChildrenIDs.Contains(w.Id)).ToList());

                achievement.Teachers.RemoveAll(x => dto.Teachers.IndexOf(x.Title) < 0);
                foreach (var Teacher in dto.Teachers)
                {
                    if (achievement.Teachers.Find(x => x.Title == Teacher) is null)
                    {
                        achievement.Teachers.Add(new AchievementTeacher { Title = Teacher, Achievement = achievement });
                    }
                }

                var updatedAchievement = await repository.Update(achievement).ConfigureAwait(false);

                logger.LogInformation($"Achievement with Id = {achievement?.Id} updated succesfully.");

                return mapper.Map<AchievementDto>(updatedAchievement);
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError($"Updating failed. Achievement with Id = {dto?.Id} doesn't exist in the system.");
                throw;
            }
        }
    }
}
