using System.Net;

namespace AutoHibernater.Logic
{
    public class Config : IConfig
    {
        public int Port { get; set; }
        public IPAddress IPAddress { get; set; }
        public int GracePeriodInSeconds { get; set; }

        public Config()
        {
            IPAddress = IPAddress.Loopback;
            Port = 2525;
            GracePeriodInSeconds = 60;
        }
    }
}