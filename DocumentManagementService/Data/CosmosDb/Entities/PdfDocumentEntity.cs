using Newtonsoft.Json;

namespace DocumentManagementService.Data.CosmosDb.Entities
{
    public class PdfDocumentEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Path { get; set; }

        public double FileSize { get; set; }

        public string SizeMeasurement { get; set; }
    }
}