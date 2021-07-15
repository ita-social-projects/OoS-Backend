using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.ModelBinders
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

            var ages = query["Ages"];

            var bindingResults = new List<AgeRange>();
            foreach (var item in ages)
            {
                var strings = item.Trim().Split(':', ',', '}');
                var ageRange = new AgeRange();
                ageRange.MinAge = int.Parse(strings[1], CultureInfo.CurrentCulture);
                ageRange.MaxAge = int.Parse(strings[3], CultureInfo.CurrentCulture);
                bindingResults.Add(ageRange);
            }

            if (bindingResults.Count == 0)
            {
                bindingResults.Add(new AgeRange() { MinAge = 0, MaxAge = 100 });
            }

            bindingContext.Result = ModelBindingResult.Success(bindingResults);
            return Task.CompletedTask;
        }
    }
}
