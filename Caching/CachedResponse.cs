using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace IzendaCustomBootstrapper.Caching
{
    /// <summary>
    /// Wraps a regular response in a cached response
    /// The cached response invokes the original response
    /// </summary>
    public class CachedResponse : Response
    {
        private readonly Response response;

        public CachedResponse(Response response)
        {
            this.response = response;

            this.ContentType = response.ContentType;
            this.Headers = response.Headers;
            this.StatusCode = response.StatusCode;
            this.Contents = this.GetContents();
        }

        public override Task PreExecute(NancyContext context)
        {
            return this.response.PreExecute(context);
        }

        private Action<Stream> GetContents()
        {
            return stream =>
            {
                using (var memory = new MemoryStream())
                {
                    this.response.Contents.Invoke(memory);
                    var contents = Encoding.ASCII.GetString(memory.ToArray());

                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(contents);
                        writer.Flush();
                    }
                }
            };
        }
    }
}