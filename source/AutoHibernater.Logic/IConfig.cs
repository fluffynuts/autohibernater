using System.Net;

namespace AutoHibernater.Logic
{
    public interface IConfig
    {
        int Port { get; }
        IPAddress IPAddress { get; }
        int GracePeriodInSeconds { get; }
    }
}