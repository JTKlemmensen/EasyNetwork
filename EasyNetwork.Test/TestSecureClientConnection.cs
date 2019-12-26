using Moq;
using NUnit.Framework;
using EasyNetwork.Network;
using EasyNetwork.Network.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyNetwork.Test
{
    public class TestSecureClientConnection
    {
        [Test]
        public void Test_StartHandshake()
        {
            var publicKey = new byte[] { 1, 2, 3 };
            var mockAsymmetric = new Mock<IAsymmetricCipher>();
            mockAsymmetric.Setup(m => m.PublicKey).Returns(publicKey);
            var mockSymmetric = new Mock<ISymmetricCipher>();
            var mockConnection = new Mock<IConnection>();

            var secureClient = new SecureClientConnection(mockConnection.Object, mockAsymmetric.Object, mockSymmetric.Object);
            secureClient.Start();
            bool dataReceivedHasBeenCalled = false;
            secureClient.OnDataReceived += (data) => { dataReceivedHasBeenCalled = true; };

            mockConnection.Verify(m => m.SendData(publicKey));
            Assert.IsFalse(dataReceivedHasBeenCalled);
        }

        [Test]
        public void Test_EndHandshake()
        {
            var publicKey = new byte[] { 1, 2, 3 };
            var iv = new byte[] { 1, 2 };
            var key = new byte[] { 1, 2 };
            var mockAsymmetric = new Mock<IAsymmetricCipher>();
            mockAsymmetric.Setup(m => m.PublicKey).Returns(publicKey);
            mockAsymmetric.Setup(m => m.Decrypt(iv)).Returns(iv);
            mockAsymmetric.Setup(m => m.Decrypt(key)).Returns(key);

            var mockSymmetric = new Mock<ISymmetricCipher>();
            mockSymmetric.SetupSet(m => m.IV = It.IsAny<byte[]>()).Verifiable();
            mockSymmetric.SetupSet(m => m.Key = It.IsAny<byte[]>()).Verifiable();

            var mockConnection = new Mock<IConnection>();

            var secureClient = new SecureClientConnection(mockConnection.Object, mockAsymmetric.Object, mockSymmetric.Object);
            secureClient.Start();

            bool OnConnectedHasBeenCalled = false;
            secureClient.OnConnected += () => { OnConnectedHasBeenCalled = true; };
            // Act
            mockConnection.Raise(i => i.OnDataReceived += null, iv);
            mockConnection.Raise(i => i.OnDataReceived += null, key);

            mockAsymmetric.Verify(m => m.Decrypt(iv));
            mockAsymmetric.Verify(m => m.Decrypt(key));
            mockSymmetric.VerifySet(m => m.IV = iv);
            mockSymmetric.VerifySet(m => m.Key = key);

            Assert.IsTrue(OnConnectedHasBeenCalled);
        }

        [Test]
        public void Test_ReceiveDataAfterHandshake()
        {
            var data = new byte[] { 2, 4, 6 };
            var encryptedData = new byte[] { 4, 6, 8 };
            var mockAsymmetric = new Mock<IAsymmetricCipher>();
            mockAsymmetric.Setup(m => m.Decrypt(encryptedData)).Returns(data).Verifiable();

            var mockSymmetric = new Mock<ISymmetricCipher>();
            mockSymmetric.Setup(m => m.Decrypt(encryptedData)).Returns(data).Verifiable();

            var mockConnection = new Mock<IConnection>();

            var secureClient = new SecureClientConnection(mockConnection.Object, mockAsymmetric.Object, mockSymmetric.Object);

            mockConnection.Raise(i => i.OnDataReceived += null, new byte[] { });
            mockConnection.Raise(i => i.OnDataReceived += null, new byte[] { });
            byte[] receivedData = null;
            secureClient.OnDataReceived += (data) => receivedData = data;

            //Act
            mockConnection.Raise(i => i.OnDataReceived += null, encryptedData);

            mockSymmetric.Verify(m => m.Decrypt(encryptedData));
            Assert.AreEqual(data, receivedData);
            mockAsymmetric.Verify(m => m.Decrypt(It.IsAny<byte[]>()), Times.Exactly(2));
        }
    }
}