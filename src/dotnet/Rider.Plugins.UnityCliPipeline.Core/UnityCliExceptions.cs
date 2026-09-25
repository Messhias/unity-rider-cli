namespace Rider.Plugins.UnityCliPipeline;

public class UnityCliException : Exception
{
    protected UnityCliException(string message) : base(message)
    {
    }

    protected UnityCliException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public sealed class UnityCliNotFoundException(string message) : UnityCliException(message);

public sealed class UnityCliVersionFormatException : UnityCliException
{
    public UnityCliVersionFormatException(string message) : base(message)
    {
    }

    public UnityCliVersionFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public sealed class UnityCliVersionTooLowException(string message) : UnityCliException(message);
