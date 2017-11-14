using Izenda.BI.API.Bootstrappers;
using Izenda.BI.Framework.Models;
using Izenda.BI.Framework.Models.Contexts;
using Izenda.BI.Framework.Models.Paging;
using Izenda.BI.Framework.Models.ReportDesigner;
using IzendaCustomBootstrapper.Converters;
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
        const string ApiPrefix = "api";

        const string CategoryNameToChange = "Global Cat";
        const string UpdatedCategoryName = "Custom Alias Cat";
        const string TenantId = "DELDG";

        // Use this Id to check if parent is the Global Category
        private readonly Guid GlobalCategoryId = Guid.Parse("2A83E3CE-F91B-4F14-910D-76CADF42D0FE");        

        private JsonSerializerSettings _serializer;
        private JsonSerializerSettings _deserializerSettings;

        public CustomBootstrapper() 
            : base()
        {
            _serializer = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            // Special settings to deserialize the ReportPartDefinition object
            _deserializerSettings = new JsonSerializerSettings { Converters = new List<JsonConverter> { new ReportPartConverter() } };
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                // Modifies the response from the neccessary category endpoints
                ModifyReportListCategories(ctx);
                ModifyRolesAvailableCategories(ctx);
                ModifyReportAllowedCategories(ctx);
                ModifyReportSearchCategories(ctx);
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
                    list.Data = this.UpdateCategoryNamesBasedOnTenant(list.Data, currentUser);

                    var json = JsonConvert.SerializeObject(list, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }

        /// <summary>
        /// Modifies the reponse of the 'role/availableCategory' endpoint to update a category's name based on tenant
        /// </summary>
        /// <param name="ctx">the context</param>
        private void ModifyRolesAvailableCategories(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/role/availableCategory"))
                return;

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
                    categoryResult.Result = this.UpdateCategoryNamesBasedOnTenant(categoryResult.Result, currentUser);

                    var json = JsonConvert.SerializeObject(categoryResult, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }

        /// <summary>
        /// Modifies the reponse of the 'report/category/0/' endpoint to update a category's name based on tenant
        /// </summary>
        /// <param name="ctx">the context</param>
        private void ModifyReportAllowedCategories(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/report/category/0/"))
                return;

            var currentUser = UserContext.Current;
            List<Category> categories;

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                categories = JsonConvert.DeserializeObject<List<Category>>(json);
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    categories = this.UpdateCategoryNamesBasedOnTenant(categories, currentUser);

                    var json = JsonConvert.SerializeObject(categories, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }

        /// <summary>
        /// Modifies the reponse of the 'report/advancedSearch' endpoint to update a category's name based on tenant
        /// </summary>
        /// <param name="ctx">the context</param>
        private void ModifyReportSearchCategories(NancyContext ctx)
        {
            if (!ctx.Request.Url.Path.Contains($"/{ApiPrefix}/report/advancedSearch"))
                return;

            var currentUser = UserContext.Current;
            PagedResult<List<ReportDefinition>> reportResult; 

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                reportResult = JsonConvert.DeserializeObject<PagedResult<List<ReportDefinition>>>(json, _deserializerSettings);
            }

            ctx.Response.Contents = stream =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    if (currentUser?.CurrentTenant?.TenantID == TenantId)
                    {
                        foreach (var result in reportResult.Result.Where(r => r.CategoryName == CategoryNameToChange))
                        {
                            result.CategoryName = UpdatedCategoryName;
                        }
                    }

                    var json = JsonConvert.SerializeObject(reportResult, _serializer);

                    writer.Write(json);
                    writer.Flush();
                }
            };
        }

        private List<Category> UpdateCategoryNamesBasedOnTenant(List<Category> categories, UserContext currentUser)
        {            
            #warning Depending on the use case, this may need additonal conditionals
            if (currentUser?.CurrentTenant?.TenantID == TenantId)
            {
                this.UpdateCategoryNamesBasedOnTenant(categories);
            }

            return categories;
        }

        private List<Category> UpdateCategoryNamesBasedOnTenant(List<Category> categories)
        {
            foreach (var cat in categories)
            {
                if (cat.Name == CategoryNameToChange)
                {
                    cat.Name = UpdatedCategoryName;
                }

                if (cat.SubCategories.Any())
                {
                    this.UpdateCategoryNamesBasedOnTenant(cat.SubCategories);
                }
            }

            return categories;
        }
    }
}
