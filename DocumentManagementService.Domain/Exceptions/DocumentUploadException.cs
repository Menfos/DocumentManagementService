using System;

namespace DocumentManagementService.Domain.Exceptions
{
    public class DocumentUploadException : Exception
    {
        public DocumentUploadException(string message)
            : base(message)
        {
        }
    }
}