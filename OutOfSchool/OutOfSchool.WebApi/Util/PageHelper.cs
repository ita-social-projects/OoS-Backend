using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Util
{
    public static class PageHelper
    {
        private const int MinPageSize = 10;
        private const int MaxPageSize = 100;

        public static (int Skip, int Take) GetSkipTake<TFilter>(TFilter filter, int count)
            where TFilter : OffsetFilter
        {
            if (filter == null)
            {
                return (0, MinPageSize);
            }

            var take = filter.Size switch
            {
                var size when size < 10 => MinPageSize,
                var size when size > 100 => MaxPageSize,
                _ => filter.Size
            };

            var skipLimit = count - take >= 0 ? count - take : 0;

            var skip = filter.From switch
            {
                var from when from < 0 => 0,
                var from when from > skipLimit => skipLimit,
                _ => filter.From
            };

            return (skip, take);
        }
    }
}