using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Izenda.BI.Framework.Constants;
using Izenda.BI.Framework.Models.ReportDesigner;

namespace IzendaCustomBootstrapper.Converters
{
    public class ReportPartConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ReportPartDefinition));
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            return new ReportPartDefinition
            {
                Content = jObject["reportPartContent"].ToString(),
                Height = Convert.ToInt32(jObject["height"]),
                NumberOfRecord = jObject["numberOfRecord"].ParseInt(), 
                PositionX = Convert.ToInt32(jObject["positionX"]),
                PositionY = Convert.ToInt32(jObject["positionY"]),
                ReportId = Guid.Parse(jObject["reportId"].ToString()),
                SourceId = jObject["sourceId"].ParseGuid(),
                Title = jObject["title"].ToString(),
                Width = Convert.ToInt32(jObject["width"]),
                Id = Guid.Parse(jObject["id"].ToString()),
                State = (EntityState)Convert.ToInt32(jObject["state"]),
                Deleted = Convert.ToBoolean(jObject["deleted"]),
                Inserted = Convert.ToBoolean(jObject["inserted"]),
                Version = jObject["version"].ParseInt(),
                Created = Convert.ToDateTime(jObject["created"]),
                CreatedBy = jObject["createdBy"].ToString(),
                Modified = Convert.ToDateTime(jObject["modified"]),
                ModifiedBy = jObject["modifiedBy"].ToString()
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public static class ParsingExtensions
    {
        public static int? ParseInt(this JToken value)
        {
            int parsedInt;
            if (value != null && int.TryParse(value.ToString(), out parsedInt))
            {
                return parsedInt;
            }

            return null;
        }

        public static Guid? ParseGuid(this JToken value)
        {
            Guid parsedGuid;
            if (value != null && Guid.TryParse(value.ToString(), out parsedGuid))
            {
                return parsedGuid;
            }

            return null;
        }
    }
}
