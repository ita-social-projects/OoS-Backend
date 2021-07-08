using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ESTestController : ControllerBase
    {
        private readonly IWorkshopService service;
        private readonly IElasticsearchProvider<WorkshopDTO> esProvider;
        private readonly ILogger logger;

        public ESTestController(IWorkshopService workshopService, IElasticsearchProvider<WorkshopDTO> provider, ILogger logger)
        {
            this.service = workshopService;
            this.esProvider = provider;
            this.logger = logger;
        }

        [HttpPost]
        public async Task Add(WorkshopDTO entity)
        {
            await esProvider.IndexEntityAsync(entity).ConfigureAwait(false);
        }

        [HttpPut]
        public async Task Update(WorkshopDTO entity)
        {
            await esProvider.UpdateEntityAsync(entity).ConfigureAwait(false);
        }

        [HttpDelete]
        public async Task Delete(WorkshopDTO entity)
        {
            await esProvider.DeleteEntityAsync(entity).ConfigureAwait(false);
        }

        [HttpDelete]
        public async Task ReIndex()
        {
            var source = await service.GetAll().ConfigureAwait(false);
            await esProvider.ReIndexAll(source).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            var res = await esProvider.Search(query).ConfigureAwait(false);

            return Ok(res);
        }
    }
}
