using System;
using Microsoft.Azure.Documents;

namespace DocumentManagementService.Data.CosmosDb.ClientFactories
{
    public interface ICosmosDocumentClientFactory : IDisposable
    {
        IDocumentClient GetClient();
    }
}