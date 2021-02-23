using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tools
{
    public static class GenericModelDTOExtension 
    {
        public static T ToDomain<T,U>(this U obj , IMapper mapper) 
            where T : class
            where U : class
        {
            return mapper.Map<U, T>(obj);
        }


        public static U ToModel<U, T>(this T obj, IMapper mapper)
           where T : class
           where U : class
        {
            return mapper.Map<T, U>(obj);
        }
    }


}
