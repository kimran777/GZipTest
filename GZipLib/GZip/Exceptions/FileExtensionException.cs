using System;
using System.Runtime.Serialization;

namespace GZipLib.GZip.Exceptions
{
    public class FileExtensionException : Exception
    {
        public FileExtensionException() : base("Ошибка расширения файла")
        {
            
        }

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
