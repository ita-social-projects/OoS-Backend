﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class AgeRangeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var query = bindingContext.HttpContext.Request.Query;

            // Examples:
            // In QueryString: Ages={"minAge":0,"maxAge":5}&Ages={"minAge":10,"maxAge":14}
            // ages = 2 elements of StringValues {"minAge":0,"maxAge":5}, {"minAge":10,"maxAge":14}
            var ages = query["Ages"];

            var bindingResults = new List<AgeRangeES>();
            foreach (var item in ages)
            {
                // example of item: {"minAge":0,"maxAge":5}, "minAge":0,"maxAge":5
                var strings = item.Trim().Split(':', ',', '}');
                var ageRange = new AgeRangeES();
                ageRange.MinAge = int.Parse(strings[1]);
                ageRange.MaxAge = int.Parse(strings[3]);
                bindingResults.Add(ageRange);
            }

            if (bindingResults.Count == 0)
            {
                bindingResults.Add(new AgeRangeES() { MinAge = 0, MaxAge = 100 });
            }

            bindingContext.Result = ModelBindingResult.Success(bindingResults);
            return Task.CompletedTask;
        }
    }
}
