using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;

namespace Quartz.NET.Web.Utility
{
    public class TestCache : MemoryCache
    {
        public TestCache(IOptions<MemoryCacheOptions> optionsAccessor) : base(optionsAccessor)
        {
           
        }
        ~TestCache()
        {

        }

    }
}
