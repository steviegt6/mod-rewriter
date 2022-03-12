using System;

namespace ModRewriter.Core.Exceptions
{
    public class SemanticModelNotFoundException : Exception
    {
        public SemanticModelNotFoundException(string? message = null, Exception? innerException = null) : base(
            message,
            innerException
        )
        {
        }
    }
}