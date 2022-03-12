using System;

namespace ModRewriter.Core.Exceptions
{
    public class SyntaxRootNotFoundException : Exception
    {
        public SyntaxRootNotFoundException(string? message = null, Exception? innerException = null) : base(
            message,
            innerException
        )
        {
        }
    }
}