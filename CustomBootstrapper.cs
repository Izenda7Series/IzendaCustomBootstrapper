using Izenda.BI.API.Bootstrappers;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Izenda.BI.API.Helper;
using Models;

namespace Mvc5StarterKit.IzendaBoundary
{
    public class CustomBootstrapper : IzendaBootstraper
    {
        const string ApiPrefix = "api";

        private JsonSerializerSettings _serializer;

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                ModifyLoadPartialEndpoint(ctx);

            });

            _serializer = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                ContractResolver = new IzendaSerializerContractResolver()
            };

            base.RequestStartup(container, pipelines, context);
        }

        /// <summary>
        /// Modifies the response from the 'report/loadPartialDataSourceCategory' endpoint
        /// </summary>
        /// <param name="ctx">The nancy context.</param>
        private void ModifyLoadPartialEndpoint(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/report/loadPartialDataSourceCategory"))
                return;

            // DataSource to remove
            const string sourceToRemove = "Employees";
            DataSourceCategoryResponse dataSourceCategoryResponse = new DataSourceCategoryResponse();

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);
                var json = Encoding.UTF8.GetString(memory.ToArray());
                dataSourceCategoryResponse = JsonConvert.DeserializeObject<DataSourceCategoryResponse>(json);
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var dataSourceCat in dataSourceCategoryResponse.Data)
                    {
                        foreach (var querySource in dataSourceCat.QuerySource.ToList())
                        {
                            if (querySource.Name == sourceToRemove)
                            {
                                //Maybe make a list to remove all after the fact
                                dataSourceCat.QuerySource.Remove(querySource);
                            }
                        }
                    }

                    var json = JsonConvert.SerializeObject(dataSourceCategoryResponse, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }

    }
}
