using Izenda.BI.API.Bootstrappers;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Izenda.BI.API.Helper;
using Izenda.BI.Framework.Models;

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
                ModifyDataFormatEndpoint(ctx);
            });

            _serializer = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                ContractResolver = new IzendaSerializerContractResolver()
            };

            base.RequestStartup(container, pipelines, context);
        }


        private void ModifyDataFormatEndpoint(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/report/field/dataFormat/"))
                return;


            List<DataFormat> formatList = new List<DataFormat>();


            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);
                var json = Encoding.UTF8.GetString(memory.ToArray());
                formatList = JsonConvert.DeserializeObject<List<DataFormat>>(json);
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var format in formatList.ToList())
                    {
                        if (format.Name.Contains("$") || format.Format.Contains("$"))
                        {
                            formatList.Remove(format);
                        }
                    }

                    var json = JsonConvert.SerializeObject(formatList, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };

        }

    }
}