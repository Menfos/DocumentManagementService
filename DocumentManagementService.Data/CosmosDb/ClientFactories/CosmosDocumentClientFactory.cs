using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace DocumentManagementService.Data.CosmosDb.ClientFactories
{
    public class CosmosDocumentClientFactory : ICosmosDocumentClientFactory
    {
        private readonly DocumentClient _documentClient;

        public CosmosDocumentClientFactory(string serviceEndpoint, string authenticationKey)
        {
            var serviceEndpointUriParsed = Uri.TryCreate(serviceEndpoint, UriKind.Absolute, out var serviceEndpointUri);
            if (string.IsNullOrEmpty(serviceEndpoint) || !serviceEndpointUriParsed)
                throw new ArgumentException("Service endpoint is not valid", nameof(serviceEndpoint));

            if (string.IsNullOrEmpty(authenticationKey))
                throw new ArgumentException("Authentication key should not be null or empty", nameof(authenticationKey));

            _documentClient = new DocumentClient(serviceEndpointUri, authenticationKey);
        }

        public IDocumentClient GetClient() => _documentClient;

        public void Dispose()
        {
            _documentClient?.Dispose();
        }
    }
}