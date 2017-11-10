using Izenda.BI.API.Bootstrappers;
using Izenda.BI.Framework.Models;
using Izenda.BI.Framework.Models.Contexts;
using Izenda.BI.Framework.Models.Paging;
using IzendaCustomBootstrapper.Models;
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
                // Modifies the response from the neccessary category endpoints
                ModifyReportListCategories(ctx);
                ModifyRolesAvailableCategories(ctx);
            });

            base.RequestStartup(container, pipelines, context);
        }

        /// <summary>
        /// Modifies the reponse from 'report/allcategories' & 'report/list2' endpoints to update a category's name based on tenant
        /// </summary>
        /// <param name="ctx">The nancy context</param>
        private void ModifyReportListCategories(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/report/allcategories") && !ctx.Request.Url.Path.Contains($"/{ApiPrefix}/report/list2"))
            {
                return;
            }

            const string tenantId = "DELDG";
            const string catNameToChange = "Global Cat";
            const string updatedCatName = "Custom Alias Cat";
            var currentUser = UserContext.Current;
            ReportList list;

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                list = JsonConvert.DeserializeObject<ReportList>(json);
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    #warning Depending on your use case, this may need additonal conditionals to check for a list that does not have all the assumed fields populated.
                    if (currentUser?.CurrentTenant?.TenantID == tenantId && list.Data.Any())
                    {
                        var subCategories = list.Data.FirstOrDefault(d => d.IsGlobal).SubCategories;
                        foreach (var cat in subCategories.Where(s => s.Name.Contains(catNameToChange)))
                        {
                            cat.Name = updatedCatName;
                        }
                    }

                    var json = JsonConvert.SerializeObject(list, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }

        /// <summary>
        /// Modifies the reponse of the 'report/availableCategory' endpoint to update a category's name based on tenant
        /// </summary>
        /// <param name="ctx">the context</param>
        private void ModifyRolesAvailableCategories(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/role/availableCategory"))
                return;

            const string tenantId = "DELDG";
            const string catNameToChange = "Global Cat";
            const string updatedCatName = "Custom Alias Cat";
            var currentUser = UserContext.Current;
            PagedResult<List<Category>> categoryResult;

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                categoryResult = JsonConvert.DeserializeObject<PagedResult<List<Category>>>(json);
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    #warning Depending on your use case, this may need additonal conditionals to check for a list that does not have all the assumed fields populated.
                    if (currentUser?.CurrentTenant?.TenantID == tenantId && categoryResult.Result.Any())
                    {
                        var subCategories = categoryResult.Result.FirstOrDefault(d => d.IsGlobal).SubCategories;
                        foreach (var cat in subCategories.Where(s => s.Name.Contains(catNameToChange)))
                        {
                            cat.Name = updatedCatName;
                        }
                    }

                    var json = JsonConvert.SerializeObject(categoryResult, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
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
    }
}
