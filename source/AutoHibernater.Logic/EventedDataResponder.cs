using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoHibernater.Logic.Events;
using PeanutButter.TinyEventAggregator;
using Simple.MailServer.Smtp;
using Simple.MailServer.Smtp.Config;

namespace AutoHibernater.Logic
{
    public class EventedDataResponder : DefaultSmtpDataResponder<ISmtpServerConfiguration>
    {
        public const int MAX_MESSAGE_SIZE = 4096;   // prevent arbitrary attack
        private readonly string _mailDir;
        private Dictionary<SmtpSessionInfo, int> _sessionMessageLengths;
        private EventAggregator _eventAggregator;

        public EventedDataResponder(ISmtpServerConfiguration configuration, EventAggregator eventAggregator)
            : base(configuration)
        {
            _sessionMessageLengths = new Dictionary<SmtpSessionInfo, int>();
            _eventAggregator = eventAggregator;
        }

        public override SmtpResponse DataStart(SmtpSessionInfo sessionInfo)
        {
            return SmtpResponse.DataStart;
        }

        public override SmtpResponse DataLine(SmtpSessionInfo sessionInfo, byte[] lineBuf)
        {
            var stringBuffer = Encoding.UTF8.GetString(lineBuf.Take(MAX_MESSAGE_SIZE).ToArray());
            _sessionMessageLengths[sessionInfo] = stringBuffer.Length;
            _eventAggregator.GetEvent<MessageReceivedEvent>().Publish(stringBuffer);
            return SmtpResponse.None;
        }

        public override SmtpResponse DataEnd(SmtpSessionInfo sessionInfo)
        {
            var size = -1;
            if (_sessionMessageLengths.ContainsKey(sessionInfo))
            {
                size = _sessionMessageLengths[sessionInfo];
                _sessionMessageLengths.Remove(sessionInfo);
            }
            var successMessage = String.Format("{0} bytes received", size);
            var response = SmtpResponse.OK.CloneAndChange(successMessage);

            return response;
        }

    }
}