using System;

namespace DocumentManagementService.Common.Exceptions
{
    public class DocumentUploadException : Exception
    {
        public DocumentUploadException(string message) : base(message)
        {
            
        }
    }
}