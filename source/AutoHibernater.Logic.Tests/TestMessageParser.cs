using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoHibernater.Logic.Events;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.TinyEventAggregator;

namespace AutoHibernater.Logic.Tests
{
    [TestFixture]
    public class TestMessageParser
    {
        [Test]
        public void Type_ShouldBeDisposable()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (MessageParser);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<IDisposable>();
            //---------------Test Result -----------------------
        }
        [Test]
        public void Construct_GivenNullEventAggregator_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentNullException>(() => CreateWith(null));

            //---------------Test Result -----------------------
            Assert.AreEqual("eventAggregator", ex.ParamName);
        }

        private MessageParser CreateWith(EventAggregator eventAggregator)
        {
            return new MessageParser(eventAggregator);
        }

        [Test]
        public void WhenMessageReceived_GivenMessageIsArbitrary_ShouldNotEmitHibernationSignalEvent()
        {
            //---------------Set up test pack-------------------
            var ea = new EventAggregator();
            var sut = CreateWith(ea);
            var hibernateEvent = ea.GetEvent<HibernationSignalEvent>();
            bool? eventValue = null;
            hibernateEvent.Subscribe(shouldHibernate => eventValue = shouldHibernate);
            var emailText = RandomValueGen.GetRandomString(5, 10);

            //---------------Assert Precondition----------------
            Assert.IsNull(eventValue);

            //---------------Execute Test ----------------------
            ea.GetEvent<MessageReceivedEvent>().Publish(emailText);

            //---------------Test Result -----------------------
            Assert.IsFalse(eventValue.HasValue);
        }

        [Test]
        public void WhenMessageReceived_GivenMessageIsACFailure_ShouldEmit_HibernationSignalEvent_WithTrue()
        {
            //---------------Set up test pack-------------------
            var ea = new EventAggregator();
            var sut = CreateWith(ea);
            var hibernateEvent = ea.GetEvent<HibernationSignalEvent>();
            bool? eventValue = null;
            hibernateEvent.Subscribe(shouldHibernate => eventValue = shouldHibernate);
            var emailText = Encoding.UTF8.GetString(TestResources.AC_FAILURE_EMAIL);

            //---------------Assert Precondition----------------
            Assert.IsNull(eventValue);

            //---------------Execute Test ----------------------
            ea.GetEvent<MessageReceivedEvent>().Publish(emailText);

            //---------------Test Result -----------------------
            Assert.IsTrue(eventValue.HasValue);
            Assert.IsTrue(eventValue.Value);
        }

        [Test]
        public void WhenMessageReceived_GivenMessageIsACRecovery_ShouldEmit_HibernationSignalEvent_WithFalse()
        {
            //---------------Set up test pack-------------------
            var ea = new EventAggregator();
            var sut = CreateWith(ea);
            var hibernateEvent = ea.GetEvent<HibernationSignalEvent>();
            bool? eventValue = null;
            hibernateEvent.Subscribe(shouldHibernate => eventValue = shouldHibernate);
            var emailText = Encoding.UTF8.GetString(TestResources.AC_RECOVERY_EMAIL);

            //---------------Assert Precondition----------------
            Assert.IsNull(eventValue);

            //---------------Execute Test ----------------------
            ea.GetEvent<MessageReceivedEvent>().Publish(emailText);

            //---------------Test Result -----------------------
            Assert.IsTrue(eventValue.HasValue);
            Assert.IsFalse(eventValue.Value);
        }

        [Test]
        public void Dispose_ShouldUnsubscribe()
        {
            //---------------Set up test pack-------------------
            var ea = new EventAggregator();
            var sut = CreateWith(ea);
            var hibernateEvent = ea.GetEvent<HibernationSignalEvent>();
            var eventValues = new List<bool>();
            hibernateEvent.Subscribe(shouldHibernate => eventValues.Add(shouldHibernate));
            var recoveryEmail = Encoding.UTF8.GetString(TestResources.AC_RECOVERY_EMAIL);
            var failureEmail = Encoding.UTF8.GetString(TestResources.AC_FAILURE_EMAIL);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.Dispose();
            ea.GetEvent<MessageReceivedEvent>().Publish(failureEmail);
            ea.GetEvent<MessageReceivedEvent>().Publish(recoveryEmail);

            //---------------Test Result -----------------------
            Assert.AreEqual(0, eventValues.Count);
        }
    }
}
