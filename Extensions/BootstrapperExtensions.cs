using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IzendaCustomBootstrapper.Extensions
{
    /// <summary>
    /// The Custom Bootstrapper Extensions
    /// </summary>
    public static class BootstrapperExtensions
    {
        /// <summary>
        /// Extension to bind the request body of the Nancy Context.
        /// </summary>
        /// <typeparam name="T">The object type to be binded</typeparam>
        /// <param name="bootstrapper">The custom bootstrapper to be extended</param>
        /// <param name="ctx">The Nancy Context</param>
        /// <returns>The request object</returns>
        public static T BindRequest<T>(this CustomBootstrapper bootstrapper, NancyContext ctx)
        {
            var requestObject = default(T);

            var requestBody = RequestStream.FromStream(ctx.Request.Body).AsString();
            requestObject = JsonConvert.DeserializeObject<T>(requestBody);

            return requestObject;
        }

        /// <summary>
        /// Extension to bind the response contents of the Nancy Context. 
        /// </summary>
        /// <typeparam name="T">The object type to be binded</typeparam>
        /// <param name="bootstrapper">The custom bootstrapper to be extended</param>
        /// <param name="ctx">The Nancy Context</param>
        /// <returns>The response object</returns>
        public static T BindResponse<T>(this CustomBootstrapper bootstrapper, NancyContext ctx)
        {
            var responseObject = default(T);

            using (var memory = new MemoryStream())
            {
                ctx.Response.Contents.Invoke(memory);

                var json = Encoding.UTF8.GetString(memory.ToArray());
                responseObject = JsonConvert.DeserializeObject<T>(json);
            }

            return responseObject;
        }
    }
}
