using System;
using AutoHibernater.Logic;
using PeanutButter.ServiceShell;
using PeanutButter.TinyEventAggregator;
using PeanutButter.Utils;

namespace AutoHibernater
{
    public class ServiceMain: Shell
    {
        public ServiceMain()
        {
            ServiceName = "AutoHibernater";
            DisplayName = "UPS Automatic hibernation";
            Interval = 3600;
        }

        private AutoDisposer _disposer;

        protected override void RunOnce()
        {
            if (StartListening(true))
            {
                WaitOnUserInput();
                StopListening();
            }
        }

        private void WaitOnUserInput()
        {
            Console.WriteLine("Press any key to end this madness...");
            Console.Read();
        }

        protected override void OnStart(string[] args)
        {
            StartListening(false);
            base.OnStart(args);
        }

        private bool StartListening(bool dummy)
        {
            lock (this)
            {
                if (_disposer != null)
                {
                    Console.WriteLine("Already listening...");
                    return false;
                }
                var config = GetConfig(dummy);
                _disposer = new AutoDisposer();
                _disposer.Add(new MessageReceiver(config, EventAggregator.Instance));
                _disposer.Add(new MessageParser(EventAggregator.Instance));
                _disposer.Add(new Hibernater(config, EventAggregator.Instance, dummy));
                return true;
            }
        }

        protected override void OnStop()
        {
            StopListening();
            base.OnStop();
        }

        private void StopListening()
        {
            lock (this)
            {
                if (_disposer != null)
                {
                    _disposer.Dispose();
                    _disposer = null;
                }
            }
        }


        private IConfig GetConfig(bool isDummy)
        {
            var config = new Config();
            var appSettings = new AppSettings();
            config.GracePeriodInSeconds = appSettings.GracePeriod;
            config.Port = appSettings.SMTPPort;
            if (isDummy)
                config.GracePeriodInSeconds = 5;
            return config;
        }
    }

}
