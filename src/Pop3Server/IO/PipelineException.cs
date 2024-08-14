using System;

namespace Pop3Server.IO
{
    public abstract class PipelineException : Exception { }

    public sealed class PipelineCancelledException : PipelineException { }
}
