using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ESTestController : ControllerBase
    {
        private readonly IWorkshopService service;
        private readonly IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider;

        public ESTestController(IWorkshopService workshopService, IElasticsearchProvider<WorkshopES, WorkshopFilterES> provider)
        {
            this.service = workshopService;
            this.esProvider = provider;
        }

        [HttpPost]
        public async Task Add(WorkshopES entity)
        {
            await esProvider.IndexEntityAsync(entity).ConfigureAwait(false);
        }

        [HttpPut]
        public async Task Update(WorkshopES entity)
        {
            await esProvider.UpdateEntityAsync(entity).ConfigureAwait(false);
        }

        [HttpDelete]
        public async Task Delete(WorkshopES entity)
        {
            await esProvider.DeleteEntityAsync(entity).ConfigureAwait(false);
        }

        [HttpDelete]
        public async Task ReIndex()
        {
            var sourceDto = await service.GetAll().ConfigureAwait(false);

            List<WorkshopES> source = new List<WorkshopES>();
            foreach (var item in sourceDto)
            {
                source.Add(item.ToESModel());
            }

            await esProvider.ReIndexAll(source).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> Search(WorkshopFilterES filter)
        {
            var res = await esProvider.Search(filter).ConfigureAwait(false);

            return Ok(res);
        }
    }
}
