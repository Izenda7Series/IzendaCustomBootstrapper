using Izenda.BI.API.Bootstrappers;
using Izenda.BI.Framework.Models;
using Izenda.BI.Framework.Models.Contexts;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IzendaCustomBootstrapper
{
    public class CustomBootstrapper : IzendaBootstraper
    {
        const string ApiPrefix = "api";

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
                // 'report/field/dataFormat/{dataType}' endpoint
                LoadDataFormatsData(ctx);
            });

            base.RequestStartup(container, pipelines, context);
        }

        /// <summary>
        /// Modifies the response from the 'report/field/dataFormat/{dataType}' endpoint
        /// </summary>
        /// <param name="ctx">The nancy context.</param>
        private void LoadDataFormatsData(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/report/field/dataFormat/"))
                return;

            // Checks the Current Tenant's ID
            if (UserContext.Current?.CurrentTenant?.TenantID != "DELDG")
            {
                return;
            }

            List<DataFormat> dataFormats;

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                dataFormats = JsonConvert.DeserializeObject<List<DataFormat>>(json);
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    // Re-orders the list based on the category
                    var reorderedFormats = dataFormats.Where(d => d.Category == "Custom Format").ToList();
                    reorderedFormats.AddRange(dataFormats.Where(d => d.Category != "Custom Format"));

                    var json = JsonConvert.SerializeObject(reorderedFormats, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }
    }
}
