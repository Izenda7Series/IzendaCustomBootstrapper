using System;
using System.Collections.Generic;
using Nancy;

namespace IzendaCustomBootstrapper.Caching
{
    public class CacheProvider
    {
        private static readonly Dictionary<string, int> endpointsToCacheWithLifetime = new Dictionary<string, int>
        {
            { "/api/tenant/activeTenants", 1000 },
            { "/api/systemSetting/systemMode", 2100},
            { "/api/systemSetting/systemModeSettings", 2100 },
            { "/api/systemSetting/deploymentMode", 2100 },
            { "/api/systemSetting/databaseSetup", 2100 },
            { "/api/databaseSetup/SupportedDatabaseType", 2100 }
        };

        private static readonly Dictionary<string, Tuple<DateTime, Response, int>> cachedResponses = new Dictionary<string, Tuple<DateTime, Response, int>>();

        public static Response CheckCache(NancyContext context)
        {
            Tuple<DateTime, Response, int> cacheEntry;

            if (cachedResponses.TryGetValue(context.Request.Path, out cacheEntry))
            {
                if (cacheEntry.Item1.AddSeconds(cacheEntry.Item3) > DateTime.Now)
                {
                    return cacheEntry.Item2;
                }
            }

            return null;
        }

        public static void SetCache(NancyContext context)
        {
            if (context.Response.StatusCode != HttpStatusCode.OK) { return; }

            int cacheSeconds;
            if (!endpointsToCacheWithLifetime.TryGetValue(context.Request.Path, out cacheSeconds)) { return; }

            var cachedResponse = new CachedResponse(context.Response);

            cachedResponses[context.Request.Path] = new Tuple<DateTime, Response, int>(DateTime.Now, cachedResponse, cacheSeconds);

            context.Response = cachedResponse;
        }
    }
}