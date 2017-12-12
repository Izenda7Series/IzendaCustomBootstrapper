using Izenda.BI.API.Bootstrappers;
using Izenda.BI.Framework.Models;
using Izenda.BI.Framework.Models.Contexts;
using IzendaCustomBootstrapper.Models;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IzendaCustomBootstrapper
{
    public class CustomBootstrapper : IzendaBootstraper
    {
        private static readonly List<string> endpointsToIntercept = new List<string> { "/api/report/list2", "/api/report/allcategories" };

        private JsonSerializerSettings _serializer;

        public CustomBootstrapper() 
            : base()
        {
            _serializer = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                // Add any additional conditionals based on your use case here
                if (endpointsToIntercept.Any(ctx.Request.Url.Path.Contains))
                {
                    ModifyResponseFromSpecifiedEndpoints(ctx);
                }
            });

            base.RequestStartup(container, pipelines, context);
        }


        /// <summary>
        /// Modifies the response from the specified endpoints
        /// </summary>
        /// <param name="ctx">The nancy context.</param>
        private void ModifyResponseFromSpecifiedEndpoints(NancyContext ctx)
        {
            ReportListResponse response;

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                response = JsonConvert.DeserializeObject<ReportListResponse>(json);
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    RemoveMatchedCategories(response.Data);

                    var json = JsonConvert.SerializeObject(response, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }

        /// <summary>
        /// Removes all categories and subcategories that match the conditional
        /// </summary>
        /// <param name="categories">The categories.</param>
        private void RemoveMatchedCategories(List<Category> categories)
        {
            // Removes categories that start with 'subreport'
            categories.RemoveAll(d => d.Name != null && d.Name.StartsWith("subreport", StringComparison.CurrentCultureIgnoreCase));

            foreach (var cat in categories.Where(c => c.SubCategories != null && c.SubCategories.Any()))
            {
                RemoveMatchedCategories(cat.SubCategories);
                cat.NumOfChilds = cat.SubCategories.Count();
            }
        }
    }
}
