using Moq;
using NUnit.Framework;
using ObjectNetwork.Network;
using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Test
{
    public class TestObjectConnection
    {
        [Test]
        public void Test_Start()
        {
            var mockSerializer = new Mock<ISerializer>();
            var mockConnection = new Mock<IConnection>();
            var mockEventManager = new Mock<IEventManager>();

            var objectConnection = new ObjectConnection(mockConnection.Object, mockEventManager.Object, mockSerializer.Object);

            //Act
            objectConnection.Start();
            mockConnection.Raise(m => m.OnConnected += null);

            mockEventManager.Verify(m => m.CallConnect(objectConnection), Times.Once);
            mockConnection.Verify(m => m.Start(), Times.Once);
        }

        [Test]
        public void Test_Start_HasAlreadyBeenStarted()
        {
            var mockSerializer = new Mock<ISerializer>();
            var mockConnection = new Mock<IConnection>();
            var mockEventManager = new Mock<IEventManager>();

            var objectConnection = new ObjectConnection(mockConnection.Object, mockEventManager.Object, mockSerializer.Object);

            //Act
            objectConnection.Start();
            objectConnection.Start();
            mockConnection.Raise(m => m.OnConnected += null);

            mockEventManager.Verify(m => m.CallConnect(objectConnection), Times.Once);
            mockConnection.Verify(m => m.Start(), Times.Once);
        }

        [Test]
        public void Test_Start_HasAlreadyBeenStopped()
        {
            var mockSerializer = new Mock<ISerializer>();
            var mockConnection = new Mock<IConnection>();
            var mockEventManager = new Mock<IEventManager>();

            var objectConnection = new ObjectConnection(mockConnection.Object, mockEventManager.Object, mockSerializer.Object);

            //Act
            objectConnection.Stop();
            objectConnection.Start();
            objectConnection.Stop();

            mockConnection.Raise(m => m.OnDisconnected += null);

            mockEventManager.Verify(m => m.CallDisconnect(objectConnection), Times.Once);
            mockConnection.Verify(m => m.Stop(), Times.Once);
            mockConnection.Verify(m => m.Start(), Times.Never);
        }

        [Test]
        public void Test_Stop()
        {
            var mockSerializer = new Mock<ISerializer>();
            var mockConnection = new Mock<IConnection>();
            var mockEventManager = new Mock<IEventManager>();

            var objectConnection = new ObjectConnection(mockConnection.Object, mockEventManager.Object, mockSerializer.Object);

            //Act
            objectConnection.Stop();
            mockConnection.Raise(m => m.OnDisconnected += null);

            mockEventManager.Verify(m => m.CallDisconnect(objectConnection), Times.Once);
            mockConnection.Verify(m => m.Stop(), Times.Once);
        }

        [Test]
        public void Test_Stop_HasAlreadyBeenStopped()
        {
            var mockSerializer = new Mock<ISerializer>();
            var mockConnection = new Mock<IConnection>();
            var mockEventManager = new Mock<IEventManager>();

            var objectConnection = new ObjectConnection(mockConnection.Object, mockEventManager.Object, mockSerializer.Object);

            //Act
            objectConnection.Stop();
            objectConnection.Stop();
            mockConnection.Raise(m => m.OnDisconnected += null);

            mockEventManager.Verify(m => m.CallDisconnect(objectConnection), Times.Once);
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
            mockSerializer.Setup(m => m.Serialize("String")).Returns(serializedType);

            var mockConnection = new Mock<IConnection>();
            var mockEventManager = new Mock<IEventManager>();

            var objectConnection = new ObjectConnection(mockConnection.Object, mockEventManager.Object, mockSerializer.Object);

            //Act
            objectConnection.SendObject(data);

            mockSerializer.Verify(m => m.Serialize(data), Times.Once);
            mockSerializer.Verify(m => m.Serialize("String"), Times.Once);
            mockConnection.Verify(m => m.SendData(serializedData), Times.Once);
            mockConnection.Verify(m => m.SendData(serializedType), Times.Once);
        }

        [Test]
        public void Test_ReceiveData()
        {
            byte[] serializedData = new byte[] { 1, 2, 3 };
            byte[] serializedType = new byte[] { 2, 3, 4 };

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(m => m.Deserialize<string>(serializedType)).Returns("typeName");

            var mockConnection = new Mock<IConnection>();
            var mockEventManager = new Mock<IEventManager>();

            var objectConnection = new ObjectConnection(mockConnection.Object, mockEventManager.Object, mockSerializer.Object);

            //Act
            mockConnection.Raise(m => m.OnDataReceived += null, serializedType);
            mockConnection.Raise(m => m.OnDataReceived += null, serializedData);

            mockEventManager.Verify(m => m.CallCommand("typeName", serializedData, objectConnection));
        }
    }
}