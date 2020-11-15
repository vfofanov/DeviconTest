using System;
using System.IO;

namespace DeviconTestFunctionApp
{
    public sealed class ApiEndpointResult
    {
        public ApiEndpointResult(string name, Stream stream, Exception exception)
        {
            Name = name;
            Stream = stream;
            Exception = exception;
        }

        public string Name { get; }
        public Stream Stream { get; }
        public Exception Exception { get; }
        public bool IsFailed => Exception != null;
    }
}