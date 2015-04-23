using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoHibernater.Logic.Events;
using PeanutButter.TinyEventAggregator;

namespace AutoHibernater.Logic
{
    public class MessageParser: IDisposable
    {
        private MessageReceivedEvent _messageEvent;
        private HibernationSignalEvent _hibernateEvent;
        private SubscriptionToken _subscriptionToken;

        public MessageParser(EventAggregator eventAggregator)
        {
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            _messageEvent = eventAggregator.GetEvent<MessageReceivedEvent>();
            _hibernateEvent = eventAggregator.GetEvent<HibernationSignalEvent>();
            _subscriptionToken = _messageEvent.Subscribe(Parse);
        }

        private void Parse(string message)
        {
            if (IsACFailureMessage(message))
                _hibernateEvent.Publish(true);
            else if (IsACRecoveryMessage(message))
                _hibernateEvent.Publish(false);
        }

        private bool IsACRecoveryMessage(string message)
        {
            return message.SplitOn("\n")
                .Any(l => l.Contains("Event: AC recovery;"));
        }

        private bool IsACFailureMessage(string message)
        {
            return message.SplitOn("\n")
                .Any(l => l.Contains("Event: AC failure;"));
        }

        public void Dispose()
        {
            _messageEvent.Unsubscribe(_subscriptionToken);
        }
    }

    public static class StringExtensions
    {
        public static IEnumerable<string> SplitOn(this string input, params string[] delimiters)
        {
            return input.Split(delimiters, StringSplitOptions.None);
        }
    }
}
