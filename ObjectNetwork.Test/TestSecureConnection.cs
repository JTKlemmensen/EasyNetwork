using Moq;
using NUnit.Framework;
using ObjectNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectNetwork.Test
{
    public class TestSecureConnection
    {
        [Test]
        public void Test_Constructor()
        {
            var data = new byte[] { 2, 4, 6 };
            var encryptedData = new byte[] { 4, 6, 8 };
            var mockAsymmetric = new Mock<IAsymmetricCipher>();

            var mockSymmetric = new Mock<ISymmetricCipher>();
            mockSymmetric.Setup(m => m.Encrypt(data)).Returns(encryptedData).Verifiable();

            var mockConnection = new Mock<IConnection>();
            mockConnection.SetupAdd(m => m.OnDataReceived += It.IsAny<DataReceived>());
            mockConnection.SetupAdd(m => m.OnDisconnected += It.IsAny<Disconnected>());

            var secureConnection = new Mock<SecureConnection>(mockConnection.Object, mockAsymmetric.Object, mockSymmetric.Object);
            secureConnection.CallBase = true;

            //Act
            secureConnection.Object.SendData(data);

            mockConnection.VerifyAdd(m => m.OnDataReceived += It.IsAny<DataReceived>());
            mockConnection.VerifyAdd(m => m.OnDisconnected += It.IsAny<Disconnected>());
        }

        [Test]
        public void Test_SendData()
        {
            var data = new byte[] { 2, 4, 6 };
            var encryptedData = new byte[] { 4, 6, 8 };
            var mockAsymmetric = new Mock<IAsymmetricCipher>();

            var mockSymmetric = new Mock<ISymmetricCipher>();
            mockSymmetric.Setup(m => m.Encrypt(data)).Returns(encryptedData).Verifiable();

            var mockConnection = new Mock<IConnection>();
            var secureConnection = new Mock<SecureConnection>(mockConnection.Object, mockAsymmetric.Object, mockSymmetric.Object);
            secureConnection.CallBase = true;

            //Act
            secureConnection.Object.SendData(data);

            mockSymmetric.Verify(m => m.Encrypt(data));
            mockConnection.Verify(m => m.SendData(encryptedData), Times.Once);
        }

        [Test]
        public void Test_Start()
        {
            var mockAsymmetric = new Mock<IAsymmetricCipher>();
            var mockSymmetric = new Mock<ISymmetricCipher>();
            var mockConnection = new Mock<IConnection>();

            var secureConnection = new Mock<SecureConnection>(mockConnection.Object, mockAsymmetric.Object, mockSymmetric.Object);
            secureConnection.CallBase = true;

            //Act
            secureConnection.Object.Start();

            mockConnection.Verify(m => m.Start(), Times.Once);
        }

        [Test]
        public void Test_Stop()
        {
            var mockAsymmetric = new Mock<IAsymmetricCipher>();
            var mockSymmetric = new Mock<ISymmetricCipher>();
            var mockConnection = new Mock<IConnection>();

            var secureConnection = new Mock<SecureConnection>(mockConnection.Object, mockAsymmetric.Object, mockSymmetric.Object);
            secureConnection.CallBase = true;

            //Act
            secureConnection.Object.Stop();

            mockConnection.Verify(m => m.Stop(), Times.Once);
        }

        [Test]
        public void Test_ReceiveData()
        {
            var data = new byte[] { 2, 4, 6 };
            var encryptedData = new byte[] { 4, 6, 8 };
            var mockAsymmetric = new Mock<IAsymmetricCipher>();

            var mockSymmetric = new Mock<ISymmetricCipher>();
            mockSymmetric.Setup(m => m.Decrypt(encryptedData)).Returns(data).Verifiable();

            var mockConnection = new Mock<IConnection>();

            var secureConnection = new Mock<SecureConnection>(mockConnection.Object, mockAsymmetric.Object, mockSymmetric.Object);
            secureConnection.CallBase = true;

            mockConnection.Raise(i => i.OnDataReceived += null, new byte[] { });
            mockConnection.Raise(i => i.OnDataReceived += null, new byte[] { });
            byte[] receivedData = null;
            secureConnection.Object.OnDataReceived += (data) => receivedData = data;

            //Act
            mockConnection.Raise(i => i.OnDataReceived += null, encryptedData);

            mockSymmetric.Verify(m => m.Decrypt(encryptedData));
            Assert.AreEqual(data, receivedData);
        }
    }
}