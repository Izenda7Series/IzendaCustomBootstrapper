using Izenda.BI.API.Bootstrappers;
using Izenda.BI.Framework.Models;
using Izenda.BI.Framework.Models.Common;
using Izenda.BI.Framework.Models.Paging;
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
                // 'report/loadPartialFilterFieldData' endpoint
                LoadPartialFilterFieldData(ctx);

                // 'report/loadFilterDataAsTree' endpoint
                LoadFilterDataAsTree(ctx);

                // 'report/loadFilterFieldData' endpoint
                LoadFilterFieldData(ctx);

                // 'tenants/activeTenants' endpoint
                LoadActiveTenantsData(ctx);
            });

            base.RequestStartup(container, pipelines, context);
        }

        /// <summary>
        /// Modifies the reponse of the 'report/loadPartialFilterFieldData' endpoint
        /// </summary>
        /// <param name="ctx">the context</param>
        private void LoadPartialFilterFieldData(NancyContext ctx)
        {
            //monitor requests for this route that returns a list of tenants for the tenant drop-down
            if (ctx.Request.Url.Path.Contains($"/{ApiPrefix}/report/loadPartialFilterFieldData"))
            {
                var itemsToRemove = new List<string> { "[NULL]", "[BLANK]" };
                var currentFilterValues = new List<string>();

                PagedResult<List<string>> result;

                using (var memory = new MemoryStream())
                {
                    ctx.Response.Contents.Invoke(memory);

                    var json = Encoding.UTF8.GetString(memory.ToArray());
                    result = JsonConvert.DeserializeObject<PagedResult<List<string>>>(json);

                    currentFilterValues = result.Result;
                }

                ctx.Response.Contents = stream =>
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        foreach (var item in itemsToRemove)
                        {
                            result.Result.Remove(item);
                            result.Total -= 1;
                        }

                        var json = JsonConvert.SerializeObject(result, _serializer);

                        writer.Write(json);
                        writer.Flush();
                    }
                };
            }
        }

        /// <summary>
        /// Modifies the reponse of the 'report/loadFilterFieldData' endpoint
        /// </summary>
        /// <param name="ctx">the context</param>
        private void LoadFilterFieldData(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/report/loadFilterFieldData"))
                return;

            var itemsToRemove = new List<string> { "[NULL]", "[BLANK]" };
            var currentFilterValues = new List<string>();

            List<string> result;

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                result = JsonConvert.DeserializeObject<List<string>>(json);

                currentFilterValues = result;
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var item in itemsToRemove)
                    {
                        result.Remove(item);
                    }

                    var json = JsonConvert.SerializeObject(result, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }

        /// <summary>
        /// Modifies the reponse of the 'report/loadFilterDataAsTree' endpoint
        /// </summary>
        /// <param name="ctx">the context</param>
        private void LoadFilterDataAsTree(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/report/loadFilterDataAsTree"))
                return;

            var itemsToRemove = new List<string> { "[NULL]", "[BLANK]" };

            List<ValueTreeNode> result;

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                result = JsonConvert.DeserializeObject<List<ValueTreeNode>>(json);
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var item in itemsToRemove)
                    {
                        result.RemoveAll(x => x.Text == item);
                    }

                    var json = JsonConvert.SerializeObject(result, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }

        /// <summary>
        /// Modifies the response from the 'tenant/activeTenants' endpoint
        /// </summary>
        /// <param name="ctx">The nancy context.</param>
        private void LoadActiveTenantsData(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/tenant/activeTenants"))
                return;

            // List of tenant ids to keep from response
            var tenantIdsToKeep = new List<string> { "A", "B" };

            List<Tenants> tenants;

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                tenants = JsonConvert.DeserializeObject<List<Tenants>>(json);
            }

            #warning If this list does not contain tenants, the 'tenant/activeTenants' endpoint will throw a null error.
            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    // Filter the list of tenants to only those starting with 'A' or 'B'
                    tenants.RemoveAll(t => !tenantIdsToKeep.Any(i => t.TenantID.StartsWith(i)));

                    var json = JsonConvert.SerializeObject(tenants, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }
    }
}
