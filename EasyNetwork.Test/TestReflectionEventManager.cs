using Moq;
using NUnit.Framework;
using ObjectNetwork.Network;
using ObjectNetwork.Network.Abstract;
using ObjectNetwork.Network.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Test
{

    public class TestReflectionEventManager
    {
        private class TestHandler
        {
            public bool HasOnConnectBeenCalled { get; private set; }
            public bool HasBadOnConnectBeenCalled { get; private set; }
            public bool HasOnDisconnectBeenCalled { get; private set; }
            public bool HasBadOnDisconnectBeenCalled { get; private set; }
            public bool HasOnCommandBeenCalled { get; private set; }
            public bool HasBadOnCommandBeenCalled { get; private set; }

            [Connect]
            public void OnConnect(DefaultObjectConnection con)
            {
                HasOnConnectBeenCalled = true;
            }

            [Connect]
            public void BadOnConnect(DefaultObjectConnection con, string data)
            {
                HasBadOnConnectBeenCalled = true;
            }

            [Disconnect]
            public void OnDisconnect(DefaultObjectConnection con)
            {
                HasOnDisconnectBeenCalled = true;
            }

            [Disconnect]
            public void BadOnDisconnect(DefaultObjectConnection con, int data)
            {
                HasBadOnDisconnectBeenCalled = true;
            }

            [Command]
            public void OnCommand(DefaultObjectConnection con, string connection)
            {
                HasOnCommandBeenCalled = true;
            }

            [Command]
            public void BadOnCommand(string connection, DefaultObjectConnection con)
            {
                HasBadOnCommandBeenCalled = true;
            }
        }

        [Test]
        public void Test_OnConnect()
        {
            var testHandler = new TestHandler();

            var eventManager = new ReflectionEventManager(null);
            eventManager.AddCommandHandler(testHandler);
            eventManager.CallConnect(null);

            Assert.IsTrue(testHandler.HasOnConnectBeenCalled);
            Assert.IsFalse(testHandler.HasBadOnConnectBeenCalled);
        }

        [Test]
        public void Test_2OnConnect()
        {
            var testHandler1 = new TestHandler();
            var testHandler2 = new TestHandler();

            var eventManager = new ReflectionEventManager(null);
            eventManager.AddCommandHandler(testHandler1);
            eventManager.AddCommandHandler(testHandler2);
            eventManager.CallConnect(null);

            Assert.IsTrue(testHandler1.HasOnConnectBeenCalled);
            Assert.IsTrue(testHandler2.HasOnConnectBeenCalled);
        }

        [Test]
        public void Test_OnDisconnect()
        {
            var testHandler = new TestHandler();

            var eventManager = new ReflectionEventManager(null);
            eventManager.AddCommandHandler(testHandler);
            eventManager.CallDisconnect(null);

            Assert.IsTrue(testHandler.HasOnDisconnectBeenCalled);
            Assert.IsFalse(testHandler.HasBadOnDisconnectBeenCalled);
        }

        [Test]
        public void Test_2OnDisconnect()
        {
            var testHandler1 = new TestHandler();
            var testHandler2 = new TestHandler();

            var eventManager = new ReflectionEventManager(null);
            eventManager.AddCommandHandler(testHandler1);
            eventManager.AddCommandHandler(testHandler2);

            eventManager.CallDisconnect(null);

            Assert.IsTrue(testHandler1.HasOnDisconnectBeenCalled);
            Assert.IsTrue(testHandler2.HasOnDisconnectBeenCalled);
        }

        [Test]
        public void Test_OnCommand()
        {
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<string>(It.IsAny<byte[]>())).Returns("some data");

            var testHandler = new TestHandler();

            var eventManager = new ReflectionEventManager(mockSerializer.Object);
            eventManager.AddCommandHandler(testHandler);
            eventManager.CallCommand("System.String", new byte[] { 1, 2, 3 }, null);

            Assert.IsTrue(testHandler.HasOnCommandBeenCalled);
        }

        [Test]
        public void Test_BadOnCommand()
        {
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<string>(It.IsAny<byte[]>())).Returns("some data");

            var testHandler = new TestHandler();

            var eventManager = new ReflectionEventManager(mockSerializer.Object);
            eventManager.AddCommandHandler(testHandler);
            eventManager.CallCommand("String", new byte[] { 1, 2, 3 }, null);

            Assert.IsFalse(testHandler.HasBadOnCommandBeenCalled);
        }
    }
}