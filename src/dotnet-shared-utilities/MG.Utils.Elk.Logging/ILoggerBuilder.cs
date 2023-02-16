using Serilog.Core;

namespace MG.Utils.Elk.Logging
{
    public interface ILoggerBuilder
    {
        Exception ThrownException();

        ILoggerBuilder TryCreateLogger();

        bool IsCreated();

        Logger Logger();
    }
}