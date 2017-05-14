using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GZipLib.GZip.Exceptions
{
    class FileExtensionException : Exception
    {
        public FileExtensionException()
        { }

        public FileExtensionException(string message) : base(message)
        {
        }

        public FileExtensionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FileExtensionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
