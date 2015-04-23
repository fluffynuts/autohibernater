using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AutoHibernater.Logic.Events;
using PeanutButter.TinyEventAggregator;

namespace AutoHibernater.Logic
{
    public class Hibernater: IDisposable
    {
        private bool _dummy;
        private int _gracePeriod;
        private HibernationSignalEvent _hibernationSignalEvent;
        private SubscriptionToken _subscriptionToken;
        private CancellationTokenSource _cancellationTokenSource;
        private HibernationStartedEvent _hibernationStartedEvent;

        public Hibernater(IConfig config, EventAggregator eventAggregator, bool dummy = false)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            _dummy = dummy;
            _gracePeriod = config.GracePeriodInSeconds;
            _hibernationSignalEvent = eventAggregator.GetEvent<HibernationSignalEvent>();
            _hibernationStartedEvent = eventAggregator.GetEvent<HibernationStartedEvent>();
            _subscriptionToken = _hibernationSignalEvent.Subscribe(OnHibernationSignal);
        }

        private void OnHibernationSignal(bool shouldHibernate)
        {
            if (shouldHibernate)
            {
                if (_cancellationTokenSource != null)
                    return;
                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;
                Task.Factory.StartNew(() => CountdownToHibernationWith(token), token);
            }
            else
            {
                if (_cancellationTokenSource != null)
                {
                    Console.WriteLine("Hibernation cancelled");
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource = null;
                }
            }
        }

        private void CountdownToHibernationWith(CancellationToken token)
        {
            var remaining = _gracePeriod * 1000;
            var interval = 1000;
            Console.WriteLine("Countdown to hibernation in " + _gracePeriod + " seconds");
            while (!token.IsCancellationRequested && remaining > 0)
            {
                remaining -= interval;
                Thread.Sleep(1000);
            }
            if (token.IsCancellationRequested)
                return;
            PerformHibernation();
        }

        private void PerformHibernation()
        {
            Console.WriteLine("Should hibernate now...");
            _hibernationStartedEvent.Publish(true);
            _cancellationTokenSource = null;    // allow this to run multiple times, if required
            if (!_dummy)
                PerformActualHibernation();
        }

        private void PerformActualHibernation()
        {
            var pinfo = new ProcessStartInfo();
            pinfo.FileName = "shutdown";
            pinfo.Arguments = "/h";
            var process = new Process()
            {
                StartInfo = pinfo
            };
            process.Start();
        }

        public void Dispose()
        {
        }
    }
}
