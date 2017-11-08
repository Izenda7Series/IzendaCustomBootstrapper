using Izenda.BI.API.Bootstrappers;
using Izenda.BI.API.Results;
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
                ModifyLoadSchemaEndpoint(ctx);

                ModifyConnectionDetailEndpoint(ctx);
            });

            base.RequestStartup(container, pipelines, context);
        }

        /// <summary>
        /// Modifies the response from the 'connection/reloadRemoteSchema' endpoint
        /// </summary>
        /// <param name="ctx">The nancy context.</param>
        private void ModifyLoadSchemaEndpoint(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/connection/reloadRemoteSchema"))
                return;

            // List of schemas to keep from response
            const string schemaToKeep = "TEST2";

            SchemaResult schemas;

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                schemas = JsonConvert.DeserializeObject<SchemaResult>(json);
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var db in schemas.DBSource.QuerySources)
                    {
                        foreach (var source in db.QuerySources)
                        {
                            foreach (var field in source.QuerySourceFields)
                            {
                                field.Id = Guid.Empty;
                            }
                        }
                    }
                    // schemas.DBSource.QuerySources.RemoveAll(t => t.Name != schemaToKeep);

                    var json = JsonConvert.SerializeObject(schemas, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }

        /// <summary>
        /// Modifies the response from the 'connection/detail' endpoint
        /// </summary>
        /// <param name="ctx">The nancy context.</param>
        private void ModifyConnectionDetailEndpoint(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/connection/detail"))
                return;

            // List of schemas to keep from response
            const string schemaToKeep = "TEST2";

            ConnectionResult connectionResult;

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                connectionResult = JsonConvert.DeserializeObject<ConnectionResult>(json);
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    connectionResult.Connection.DBSource.QuerySources.RemoveAll(t => t.Name != schemaToKeep);

                    var json = JsonConvert.SerializeObject(connectionResult, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }
    }
}
