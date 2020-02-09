using System.Threading.Tasks;

namespace DocumentManagementService.Data.CosmosDb.Initializers
{
    public interface ICosmosDataInitializer
    {
        Task InitializePdfDocumentCollectionIfNotExistsAsync();
    }
}