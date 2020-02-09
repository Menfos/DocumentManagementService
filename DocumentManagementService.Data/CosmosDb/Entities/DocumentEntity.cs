using Newtonsoft.Json;

namespace DocumentManagementService.Data.CosmosDb.Entities
{
    public class DocumentEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "contentType")]
        public string ContentType { get; set; }

        public string Path { get; set; }

        public double FileSizeInKilobytes { get; set; }
    }
}