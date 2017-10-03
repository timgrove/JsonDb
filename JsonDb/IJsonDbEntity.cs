using Newtonsoft.Json;
using System;

namespace JsonDb
{
    public interface IJsonDbEntity
    {
        [JsonProperty(PropertyName = "id")]
        int Id { get; set; }
    }
}
