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

            //The data source we want to remove from the list. Replace this with whatever data source(s) you want to hide from the list in the report designer  
            const string sourceToRemove = "Employees";

            //This is a model created to handle the response of this endpoint. It is inside of the Models folder. You will have to create a similar model to handle this information.
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
                                //Here we remove any data source that matches the name(s) specified above. In this example we remove any data sources named "Employees"
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
