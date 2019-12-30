using Moq;
using NUnit.Framework;
using EasyNetwork.Network;
using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Text;
using EasyNetwork.Network.Attributes;

namespace EasyNetwork.Test
{
    public class TestObjectConnection
    {
        [Test]
        public void Test_Start()
        {
            var mockSerializer = new Mock<ISerializer>();
            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };

            //Act
            objectConnection.Start();
            mockConnection.Raise(m => m.OnConnected += null);

            mockConnection.Verify(m => m.Start(), Times.Once);
        }

        [Test]
        public void Test_Start_HasAlreadyBeenStarted()
        {
            var mockSerializer = new Mock<ISerializer>();
            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };

            //Act
            objectConnection.Start();
            objectConnection.Start();
            mockConnection.Raise(m => m.OnConnected += null);

            mockConnection.Verify(m => m.Start(), Times.Once);
        }

        [Test]
        public void Test_Start_HasAlreadyBeenStopped()
        {
            var mockSerializer = new Mock<ISerializer>();
            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };

            //Act
            objectConnection.Stop();
            objectConnection.Start();
            objectConnection.Stop();

            mockConnection.Raise(m => m.OnDisconnected += null);

            mockConnection.Verify(m => m.Stop(), Times.Once);
            mockConnection.Verify(m => m.Start(), Times.Never);
        }

        [Test]
        public void Test_Stop()
        {
            var mockSerializer = new Mock<ISerializer>();
            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };

            //Act
            objectConnection.Stop();
            mockConnection.Raise(m => m.OnDisconnected += null);

            mockConnection.Verify(m => m.Stop(), Times.Once);
        }

        [Test]
        public void Test_Stop_HasAlreadyBeenStopped()
        {
            var mockSerializer = new Mock<ISerializer>();
            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };

            //Act
            objectConnection.Stop();
            objectConnection.Stop();
            mockConnection.Raise(m => m.OnDisconnected += null);

            mockConnection.Verify(m => m.Stop(), Times.Once);
        }

        [Test]
        public void Test_SendObject()
        {
            string data = "some object data";
            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedType = new byte[] { 2, 3, 4 };

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Serialize(data)).Returns(serializedData);
            mockSerializer.Setup(m => m.Serialize("System.String")).Returns(serializedType);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };

            //Act
            objectConnection.SendObject(data);

            mockSerializer.Verify(m => m.Serialize((object)data), Times.Once);
            mockSerializer.Verify(m => m.Serialize(It.IsAny<NetworkMessage>()), Times.Once);
            mockConnection.Verify(m => m.SendData(It.IsAny<byte[]>()), Times.Once);
        }

        [Test]
        public void Test_EventHandlerClass_Command()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            objectConnection.AddEventHandler(testHandler);

            //Act
            mockConnection.Raise(m => m.OnDataReceived += null, serializedMessage);

            Assert.IsTrue(testHandler.HasOnCommandBeenCalled);
            Assert.IsFalse(testHandler.HasBadOnCommandBeenCalled);
        }

        [Test]
        public void Test_EventHandlerClass_Connect()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            objectConnection.AddEventHandler(testHandler);

            //Act
            mockConnection.Raise(m => m.OnConnected += null);

            Assert.IsTrue(testHandler.HasOnConnectBeenCalled);
            Assert.IsFalse(testHandler.HasBadOnConnectBeenCalled);
        }

        [Test]
        public void Test_EventHandlerClass_Disconnect()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            objectConnection.AddEventHandler(testHandler);

            //Act
            mockConnection.Raise(m => m.OnDisconnected += null);

            Assert.IsTrue(testHandler.HasOnDisconnectBeenCalled);
            Assert.IsFalse(testHandler.HasBadOnDisconnectBeenCalled);
        }

        [Test]
        public void Test_EventHandlerClass_RemoveDisconnect()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            objectConnection.AddEventHandler(testHandler);
            objectConnection.RemoveEventHandlers(testHandler);

            //Act
            mockConnection.Raise(m => m.OnDisconnected += null);

            Assert.IsFalse(testHandler.HasOnDisconnectBeenCalled);
        }

        [Test]
        public void Test_EventHandlerClass_RemoveConnect()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            objectConnection.AddEventHandler(testHandler);
            objectConnection.RemoveEventHandlers(testHandler);

            //Act
            mockConnection.Raise(m => m.OnConnected += null);

            Assert.IsFalse(testHandler.HasOnConnectBeenCalled);
        }

        [Test]
        public void Test_EventHandlerClass_RemoveCommand()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            objectConnection.AddEventHandler(testHandler);
            objectConnection.RemoveEventHandlers(testHandler);

            //Act
            mockConnection.Raise(m => m.OnDataReceived += null, serializedMessage);

            Assert.IsFalse(testHandler.HasOnCommandBeenCalled);
        }

        [Test]
        public void Test_Lambda_OnCommand()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            bool OnCommandHasBeenCalled = false;
            objectConnection.OnCommand<string>((c, s) => OnCommandHasBeenCalled = true);

            //Act
            mockConnection.Raise(m => m.OnDataReceived += null, serializedMessage);

            Assert.IsTrue(OnCommandHasBeenCalled);
        }
        
        [Test]
        public void Test_Lambda_OnConnect()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            bool OnCommandHasBeenCalled = false;
            objectConnection.OnConnect((c) => OnCommandHasBeenCalled = true);

            //Act
            mockConnection.Raise(m => m.OnConnected += null);

            Assert.IsTrue(OnCommandHasBeenCalled);
        }

        [Test]
        public void Test_Lambda_OnDisconnect()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            bool OnCommandHasBeenCalled = false;
            objectConnection.OnDisconnect((c) => OnCommandHasBeenCalled = true);

            //Act
            mockConnection.Raise(m => m.OnDisconnected += null);

            Assert.IsTrue(OnCommandHasBeenCalled);
        }

        [Test]
        public void Test_Lambda_RemoveOnCommand()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            bool OnCommandHasBeenCalled = false;
            Action<IObjectConnection, string> del = (c, s) => OnCommandHasBeenCalled = true;
            objectConnection.OnCommand<string>(del);
            objectConnection.RemoveOnCommand<string>(del);

            //Act
            mockConnection.Raise(m => m.OnDataReceived += null, serializedMessage);

            Assert.IsFalse(OnCommandHasBeenCalled);
        }

        [Test]
        public void Test_Lambda_RemoveOnConnect()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            bool OnCommandHasBeenCalled = false;
            Action<IObjectConnection> del = (c) => OnCommandHasBeenCalled = true;
            objectConnection.OnConnect(del);
            objectConnection.RemoveOnConnect(del);

            //Act
            mockConnection.Raise(m => m.OnConnected += null);

            Assert.IsFalse(OnCommandHasBeenCalled);
        }

        [Test]
        public void Test_Lambda_RemoveOnDisconnect()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            bool OnCommandHasBeenCalled = false;
            Action<IObjectConnection> del = (c) => OnCommandHasBeenCalled = true;
            objectConnection.OnDisconnect(del);
            objectConnection.RemoveOnDisconnect(del);

            //Act
            mockConnection.Raise(m => m.OnDisconnected += null);

            Assert.IsFalse(OnCommandHasBeenCalled);
        }

        [Test]
        public void Test_Lambda_RemoveOnCommand_CustomCreator()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            bool OnCommandHasBeenCalled = false;
            Action<IObjectConnection, string> del = (c, s) => OnCommandHasBeenCalled = true;
            string creator = "CreatorObject :)";
            objectConnection.OnCommand<string>(del, creator);
            objectConnection.RemoveOnCommand<string>(creator);

            //Act
            mockConnection.Raise(m => m.OnDataReceived += null, serializedMessage);

            Assert.IsFalse(OnCommandHasBeenCalled);
        }

        [Test]
        public void Test_Lambda_RemoveOnCommand_CustomCreator_NonGeneric()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            bool OnCommandHasBeenCalled = false;
            Action<IObjectConnection, string> del = (c, s) => OnCommandHasBeenCalled = true;
            string creator = "CreatorObject :)";
            objectConnection.OnCommand<string>(del, creator);
            objectConnection.RemoveOnCommand(creator);

            //Act
            mockConnection.Raise(m => m.OnDataReceived += null, serializedMessage);

            Assert.IsFalse(OnCommandHasBeenCalled);
        }

        [Test]
        public void Test_Lambda_RemoveOnConnect_CustomCreator()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            bool OnCommandHasBeenCalled = false;
            Action<IObjectConnection> del = (c) => OnCommandHasBeenCalled = true;
            string creator = "CreatorObject :)";
            objectConnection.OnConnect(del,creator);
            objectConnection.RemoveOnConnect(creator);

            //Act
            mockConnection.Raise(m => m.OnConnected += null);

            Assert.IsFalse(OnCommandHasBeenCalled);
        }

        [Test]
        public void Test_Lambda_RemoveOnDisconnect_CustomCreator()
        {
            var testHandler = new TestHandler();

            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedMessage = new byte[] { 1, 2, 3, 4 };
            string unserializedData = "some data";
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<NetworkMessage>(serializedMessage)).Returns(new NetworkMessage { Data = serializedData, Name = "System.String" });
            mockSerializer.Setup(m => m.Deserialize<string>(serializedData)).Returns(unserializedData);

            var mockConnection = new Mock<IConnection>();

            var objectConnection = new DefaultObjectConnection(mockConnection.Object) { Serializer = mockSerializer.Object };
            bool OnCommandHasBeenCalled = false;
            Action<IObjectConnection> del = (c) => OnCommandHasBeenCalled = true;
            string creator = "CreatorObject :)";
            objectConnection.OnDisconnect(del, creator);
            objectConnection.RemoveOnDisconnect(creator);

            //Act
            mockConnection.Raise(m => m.OnDisconnected += null);

            Assert.IsFalse(OnCommandHasBeenCalled);
        }

        private class TestHandler
        {
            public bool HasOnConnectBeenCalled { get; private set; }
            public bool HasBadOnConnectBeenCalled { get; private set; }
            public bool HasOnDisconnectBeenCalled { get; private set; }
            public bool HasBadOnDisconnectBeenCalled { get; private set; }
            public bool HasOnCommandBeenCalled { get; private set; }
            public bool HasBadOnCommandBeenCalled { get; private set; }

            [Connect]
            public void OnConnect(IObjectConnection con)
            {
                HasOnConnectBeenCalled = true;
            }

            [Connect]
            public void BadOnConnect(IObjectConnection con, string data)
            {
                HasBadOnConnectBeenCalled = true;
            }

            [Disconnect]
            public void OnDisconnect(IObjectConnection con)
            {
                HasOnDisconnectBeenCalled = true;
            }

            [Disconnect]
            public void BadOnDisconnect(IObjectConnection con, int data)
            {
                HasBadOnDisconnectBeenCalled = true;
            }

            [Command]
            public void OnCommand(IObjectConnection con, string connection)
            {
                HasOnCommandBeenCalled = true;
            }

            [Command]
            public void BadOnCommand(string connection, IObjectConnection con)
            {
                HasBadOnCommandBeenCalled = true;
            }
        }
    }
}