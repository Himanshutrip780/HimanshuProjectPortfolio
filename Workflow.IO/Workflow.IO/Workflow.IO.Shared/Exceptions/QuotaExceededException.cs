using System;

namespace Workflow.IO.Shared.Exceptions
{
    public class QuotaExceededException : Exception
    {
        public QuotaExceededException(string message) : base(message)
        {
        }
    }
}
