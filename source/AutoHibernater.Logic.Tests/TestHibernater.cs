using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoHibernater.Logic.Events;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.TinyEventAggregator;

namespace AutoHibernater.Logic.Tests
{
    [TestFixture]
    public class TestHibernater
    {
        [Test]
        public void Type_ShouldImplement_IDisposable()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (Hibernater);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<IDisposable>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_GivenNullConfig_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentNullException>(() => CreateWith(null, null));

            //---------------Test Result -----------------------
            Assert.AreEqual("config", ex.ParamName);
        }

        [Test]
        public void Construct_GivenNullEventAggregator_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentNullException>(() => CreateWith(Substitute.For<IConfig>(), null));

            //---------------Test Result -----------------------
            Assert.AreEqual("eventAggregator", ex.ParamName);
        }

        private Hibernater CreateWith(IConfig config, EventAggregator eventAggregator)
        {
            return new Hibernater(config, eventAggregator, true);
        }

        [Test]
        public void OnPositiveHibernationEvent_ShouldAttemptToHibernate()
        {
            //---------------Set up test pack-------------------
            var config = Substitute.For<IConfig>();
            config.GracePeriodInSeconds.Returns(1);
            var ea = new EventAggregator();
            var called = false;
            ea.GetEvent<HibernationStartedEvent>().Subscribe(o => called = o);

            var sut = CreateWith(config, ea);
            
            //---------------Assert Precondition----------------
            Assert.IsFalse(called);

            //---------------Execute Test ----------------------
            ea.GetEvent<HibernationSignalEvent>().Publish(true);
            var totalSleep = 0;
            var interval = 100;
            while (!called && totalSleep < 5000)
            {
                totalSleep += interval;
                Thread.Sleep(interval);
            }

            //---------------Test Result -----------------------
            Assert.IsTrue(called);
        }

        [Test]
        public void OnNegativeHibernationEvent_ShouldCancelHibernationAttempt()
        {
            //---------------Set up test pack-------------------
            var config = Substitute.For<IConfig>();
            config.GracePeriodInSeconds.Returns(1);
            var ea = new EventAggregator();
            var called = false;
            ea.GetEvent<HibernationStartedEvent>().Subscribe(o => called = o);

            var sut = CreateWith(config, ea);
            
            //---------------Assert Precondition----------------
            Assert.IsFalse(called);

            //---------------Execute Test ----------------------
            ea.GetEvent<HibernationSignalEvent>().Publish(true);
            ea.GetEvent<HibernationSignalEvent>().Publish(false);
            var totalSleep = 0;
            var interval = 100;
            while (!called && totalSleep < 5000)
            {
                totalSleep += interval;
                Thread.Sleep(interval);
            }

            //---------------Test Result -----------------------
            Assert.IsFalse(called);
        }

        [Test]
        public void OnNegativeHibernationEvent_WhenNotInHibernationWait_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var config = Substitute.For<IConfig>();
            config.GracePeriodInSeconds.Returns(1);
            var ea = new EventAggregator();
            var called = false;
            ea.GetEvent<HibernationStartedEvent>().Subscribe(o => called = o);

            var sut = CreateWith(config, ea);
            
            //---------------Assert Precondition----------------
            Assert.IsFalse(called);

            //---------------Execute Test ----------------------
            ea.GetEvent<HibernationSignalEvent>().Publish(false);

            //---------------Test Result -----------------------
            Assert.IsFalse(called);
        }
        [Test]
        public void OnPositiveHibernationEvent_WhenInHibernationWait_ShouldNotStartNewAttempt()
        {
            //---------------Set up test pack-------------------
            var config = Substitute.For<IConfig>();
            config.GracePeriodInSeconds.Returns(1);
            var ea = new EventAggregator();
            var callCount = 0;
            ea.GetEvent<HibernationStartedEvent>().Subscribe(o => callCount++);

            var sut = CreateWith(config, ea);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            ea.GetEvent<HibernationSignalEvent>().Publish(true);
            ea.GetEvent<HibernationSignalEvent>().Publish(true);
            var totalSleep = 0;
            var interval = 100;
            while (totalSleep < 5000)
            {
                totalSleep += interval;
                Thread.Sleep(interval);
            }

            //---------------Test Result -----------------------
            Assert.AreEqual(1, callCount);
        }
    }
}
