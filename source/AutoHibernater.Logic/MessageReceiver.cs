using System;
using System.Threading.Tasks;
using PeanutButter.TinyEventAggregator;
using Simple.MailServer.Smtp;
using Simple.MailServer.Smtp.Config;

namespace AutoHibernater.Logic
{
    public class MessageReceiver: IDisposable
    {
        private SmtpServer _smtpServer;

        public MessageReceiver(IConfig config, EventAggregator eventAggregator)
        {
            _smtpServer = new SmtpServer();
            _smtpServer.DefaultResponderFactory =
                new DefaultSmtpResponderFactory<ISmtpServerConfiguration>(_smtpServer.Configuration)
                {
                    DataResponder = new EventedDataResponder(_smtpServer.Configuration, eventAggregator)
                };

            _smtpServer.BindAndListenTo(config.IPAddress, config.Port);
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_smtpServer != null)
                {
                    _smtpServer.Dispose();
                }
            }
        }
    }
}
